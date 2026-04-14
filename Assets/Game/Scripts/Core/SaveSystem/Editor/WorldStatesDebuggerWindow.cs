using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class WorldStatesDebuggerWindow : EditorWindow
{
    private BinarySaveService _saveService;
    private WorldStatesData _data;
    private int _selectedIndex = -1;

    private Vector2 _listScroll;
    private Vector2 _detailsScroll;

    private string _status = "Idle";
    private bool _isBusy;

    [MenuItem("Tools/Save Debugger/World States")]
    public static void ShowWindow()
    {
        GetWindow<WorldStatesDebuggerWindow>("World States Debugger");
    }

    private void OnEnable()
    {
        _saveService = new BinarySaveService();
        _data = new WorldStatesData();
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
            CreateNewObject();
        }

        if (GUILayout.Button("Delete Selected", EditorStyles.toolbarButton, GUILayout.Width(120)))
        {
            DeleteSelected();
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(320));

        EditorGUILayout.LabelField("World Objects", EditorStyles.boldLabel);

        if (_data == null)
            _data = new WorldStatesData();

        _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

        for (int i = 0; i < _data.objects.Count; i++)
        {
            var obj = _data.objects[i];
            string label = $"[{i}] {obj.objectId} | {obj.sceneName}";

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

        if (_selectedIndex < 0 || _data == null || _selectedIndex >= _data.objects.Count)
        {
            EditorGUILayout.HelpBox("Select an object from the list or create a new one.", MessageType.None);
            EditorGUILayout.EndVertical();
            return;
        }

        var obj = _data.objects[_selectedIndex];

        _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);

        obj.objectId = EditorGUILayout.TextField("Object Id", obj.objectId);
        obj.sceneName = EditorGUILayout.TextField("Scene Name", obj.sceneName);
        obj.isDestroyed = EditorGUILayout.Toggle("Is Destroyed", obj.isDestroyed);
        obj.isActivated = EditorGUILayout.Toggle("Is Activated", obj.isActivated);

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
            var loaded = await _saveService.LoadAsync<WorldStatesData>(SaveType.WorldStates);
            _data = loaded ?? new WorldStatesData();

            if (_data.objects == null)
                _data.objects = new List<PersistentWorldObjectState>();

            if (_selectedIndex >= _data.objects.Count)
                _selectedIndex = -1;

            _status = $"Loaded {_data.objects.Count} world states.";
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
            await _saveService.SaveAsync(SaveType.WorldStates, _data ?? new WorldStatesData());
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

    private void CreateNewObject()
    {
        if (_data == null)
            _data = new WorldStatesData();

        var obj = new PersistentWorldObjectState
        {
            objectId = Guid.NewGuid().ToString(),
            sceneName = "",
            isDestroyed = false,
            isActivated = false
        };

        _data.objects.Add(obj);
        _selectedIndex = _data.objects.Count - 1;
        _status = "New world state created locally. Click Save to persist.";
    }

    private void DeleteSelected()
    {
        if (_data == null || _selectedIndex < 0 || _selectedIndex >= _data.objects.Count)
            return;

        _data.objects.RemoveAt(_selectedIndex);

        if (_selectedIndex >= _data.objects.Count)
            _selectedIndex = _data.objects.Count - 1;

        _status = "World state removed locally. Click Save to persist.";
    }

    private void DuplicateSelected()
    {
        if (_data == null || _selectedIndex < 0 || _selectedIndex >= _data.objects.Count)
            return;

        var source = _data.objects[_selectedIndex];

        var copy = new PersistentWorldObjectState
        {
            objectId = Guid.NewGuid().ToString(),
            sceneName = source.sceneName,
            isDestroyed = source.isDestroyed,
            isActivated = source.isActivated
        };

        _data.objects.Add(copy);
        _selectedIndex = _data.objects.Count - 1;
        _status = "World state duplicated locally. Click Save to persist.";
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