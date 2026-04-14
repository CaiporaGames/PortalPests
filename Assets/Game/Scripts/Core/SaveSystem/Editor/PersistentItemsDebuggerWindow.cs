using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class PersistentItemsDebuggerWindow : EditorWindow
{
    private BinarySaveService _saveService;

    private PersistentItemsData _data;
    private int _selectedIndex = -1;

    private Vector2 _listScroll;
    private Vector2 _detailsScroll;

    private string _status = "Idle";
    private bool _isBusy;

    [MenuItem("Tools/Save Debugger/Persistent Items")]
    public static void ShowWindow()
    {
        GetWindow<PersistentItemsDebuggerWindow>("Persistent Items Debugger");
    }

    private void OnEnable()
    {
        _saveService = new BinarySaveService();
        _data = new PersistentItemsData();
    }

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        DrawLeftPanel();
        DrawRightPanel();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(_status, MessageType.Info);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUI.enabled = !_isBusy;
        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            LoadData().Forget();
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            SaveData().Forget();
        }

        if (GUILayout.Button("Create New", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            CreateNewItem();
        }

        if (GUILayout.Button("Delete Selected", EditorStyles.toolbarButton, GUILayout.Width(120)))
        {
            DeleteSelected();
        }

        if (GUILayout.Button("Reveal File", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            RevealFile();
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(320));

        EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

        if (_data == null)
            _data = new PersistentItemsData();

        _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

        for (int i = 0; i < _data.items.Count; i++)
        {
            var item = _data.items[i];
            string label = $"[{i}] {item.itemId} | {item.itemType} | {item.state}";

            GUIStyle style = (i == _selectedIndex) ? EditorStyles.helpBox : EditorStyles.miniButton;
            if (GUILayout.Button(label, style))
            {
                _selectedIndex = i;
                GUI.FocusControl(null);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);

        if (_selectedIndex < 0 || _data == null || _selectedIndex >= _data.items.Count)
        {
            EditorGUILayout.HelpBox("Select an item from the list or create a new one.", MessageType.None);
            EditorGUILayout.EndVertical();
            return;
        }

        var item = _data.items[_selectedIndex];

        _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);

        item.itemId = EditorGUILayout.TextField("Item Id", item.itemId);
        item.itemType = (PersistentItemType)EditorGUILayout.EnumPopup("Item Type", item.itemType);
        item.state = (PersistentItemState)EditorGUILayout.EnumPopup("State", item.state);

        EditorGUILayout.Space(6);
        item.sceneName = EditorGUILayout.TextField("Scene Name", item.sceneName);
        item.slotIndex = EditorGUILayout.IntField("Slot Index", item.slotIndex);

        EditorGUILayout.Space(6);
        item.position = EditorGUILayout.Vector3Field("Position", item.position);

        Vector4 rotationVec = new Vector4(item.rotation.x, item.rotation.y, item.rotation.z, item.rotation.w);
        rotationVec = EditorGUILayout.Vector4Field("Rotation (x,y,z,w)", rotationVec);
        item.rotation = new Quaternion(rotationVec.x, rotationVec.y, rotationVec.z, rotationVec.w);

        EditorGUILayout.Space(6);
        item.consumedByTargetId = EditorGUILayout.TextField("Consumed By Target Id", item.consumedByTargetId);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Duplicate Selected"))
        {
            DuplicateSelected();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private async UniTaskVoid LoadData()
    {
        try
        {
            SetBusy("Loading...");
            var loaded = await _saveService.LoadAsync<PersistentItemsData>(SaveType.PersistentItems);
            _data = loaded ?? new PersistentItemsData();

            if (_data.items == null)
                _data.items = new List<PersistentItemRecord>();

            if (_selectedIndex >= _data.items.Count)
                _selectedIndex = -1;

            _status = $"Loaded {_data.items.Count} items.";
            Repaint();
        }
        catch (Exception ex)
        {
            _status = "Load failed: " + ex.Message;
            Debug.LogException(ex);
        }
        finally
        {
            ClearBusy();
        }
    }

    private async UniTaskVoid SaveData()
    {
        try
        {
            SetBusy("Saving...");
            await _saveService.SaveAsync(SaveType.PersistentItems, _data ?? new PersistentItemsData());
            _status = "Save complete.";
        }
        catch (Exception ex)
        {
            _status = "Save failed: " + ex.Message;
            Debug.LogException(ex);
        }
        finally
        {
            ClearBusy();
        }
    }

    private void CreateNewItem()
    {
        if (_data == null)
            _data = new PersistentItemsData();

        var item = new PersistentItemRecord
        {
            itemId = Guid.NewGuid().ToString(),
            itemType = default,
            state = default,
            sceneName = "",
            slotIndex = -1,
            position = Vector3.zero,
            rotation = Quaternion.identity,
            consumedByTargetId = ""
        };

        _data.items.Add(item);
        _selectedIndex = _data.items.Count - 1;
        _status = "New item created locally. Click Save to persist.";
    }

    private void DeleteSelected()
    {
        if (_data == null || _selectedIndex < 0 || _selectedIndex >= _data.items.Count)
            return;

        _data.items.RemoveAt(_selectedIndex);

        if (_selectedIndex >= _data.items.Count)
            _selectedIndex = _data.items.Count - 1;

        _status = "Item removed locally. Click Save to persist.";
    }

    private void DuplicateSelected()
    {
        if (_data == null || _selectedIndex < 0 || _selectedIndex >= _data.items.Count)
            return;

        var source = _data.items[_selectedIndex];

        var copy = new PersistentItemRecord
        {
            itemId = Guid.NewGuid().ToString(),
            itemType = source.itemType,
            state = source.state,
            sceneName = source.sceneName,
            slotIndex = source.slotIndex,
            position = source.position,
            rotation = source.rotation,
            consumedByTargetId = source.consumedByTargetId
        };

        _data.items.Add(copy);
        _selectedIndex = _data.items.Count - 1;
        _status = "Item duplicated locally. Click Save to persist.";
    }

    private void RevealFile()
    {
        string path = _saveService.GetDebugPath(SaveType.PersistentItems);
        EditorUtility.RevealInFinder(path);
    }

    private void SetBusy(string message)
    {
        _isBusy = true;
        _status = message;
    }

    private void ClearBusy()
    {
        _isBusy = false;
    }
}