using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class PlayerLocationDebuggerWindow : EditorWindow
{
    private BinarySaveService _saveService;
    private PlayerLocationData _data;

    private Vector2 _detailsScroll;

    private string _status = "Idle";
    private bool _isBusy;

    [MenuItem("Tools/Save Debugger/Player Location")]
    public static void ShowWindow()
    {
        GetWindow<PlayerLocationDebuggerWindow>("Player Location Debugger");
    }

    private void OnEnable()
    {
        _saveService = new BinarySaveService();
        _data = new PlayerLocationData
        {
            hasSavedLocation = false,
            sceneName = "",
            position = Vector3.zero,
            rotation = Quaternion.identity
        };
    }

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.Space(8);

        DrawDetailsPanel();

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

        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            ClearData();
        }

        if (GUILayout.Button("Use Scene View Camera", EditorStyles.toolbarButton, GUILayout.Width(150)))
        {
            UseSceneViewCamera();
        }

        if (GUILayout.Button("Use Selected Transform", EditorStyles.toolbarButton, GUILayout.Width(160)))
        {
            UseSelectedTransform();
        }

        if (GUILayout.Button("Reveal File", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            RevealFile();
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDetailsPanel()
    {
        EditorGUILayout.LabelField("Player Location", EditorStyles.boldLabel);

        if (_data == null)
            CreateEmptyData();

        _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);

        _data.hasSavedLocation = EditorGUILayout.Toggle("Has Saved Location", _data.hasSavedLocation);

        EditorGUILayout.Space(6);

        _data.sceneName = EditorGUILayout.TextField("Scene Name", _data.sceneName);

        EditorGUILayout.Space(6);

        _data.position = EditorGUILayout.Vector3Field("Position", _data.position);

        Vector4 rotationVec = new Vector4(
            _data.rotation.x,
            _data.rotation.y,
            _data.rotation.z,
            _data.rotation.w
        );

        rotationVec = EditorGUILayout.Vector4Field("Rotation (x,y,z,w)", rotationVec);

        _data.rotation = new Quaternion(
            rotationVec.x,
            rotationVec.y,
            rotationVec.z,
            rotationVec.w
        );

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Rotation Helper", EditorStyles.boldLabel);

        Vector3 euler = _data.rotation.eulerAngles;
        Vector3 newEuler = EditorGUILayout.Vector3Field("Rotation Euler", euler);

        if (newEuler != euler)
            _data.rotation = Quaternion.Euler(newEuler);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Normalize Rotation"))
        {
            NormalizeRotation();
        }

        if (GUILayout.Button("Mark As Saved Location"))
        {
            _data.hasSavedLocation = true;
            _status = "Marked as saved location. Click Save to persist.";
        }

        if (GUILayout.Button("Mark As No Saved Location"))
        {
            _data.hasSavedLocation = false;
            _status = "Marked as no saved location. Click Save to persist.";
        }

        EditorGUILayout.EndScrollView();
    }

    private async UniTaskVoid LoadData()
    {
        try
        {
            SetBusy("Loading...");

            var loaded = await _saveService.LoadAsync<PlayerLocationData>(SaveType.PlayerLocation);
            _data = loaded ?? CreateEmptyData();

            _status = _data.hasSavedLocation
                ? $"Loaded player location from scene '{_data.sceneName}'."
                : "Loaded data, but it has no saved location.";

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

            await _saveService.SaveAsync(
                SaveType.PlayerLocation,
                _data ?? CreateEmptyData()
            );

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

    private void ClearData()
    {
        CreateEmptyData();
        _status = "Player location cleared locally. Click Save to persist.";
    }

    private PlayerLocationData CreateEmptyData()
    {
        _data = new PlayerLocationData
        {
            hasSavedLocation = false,
            sceneName = "",
            position = Vector3.zero,
            rotation = Quaternion.identity
        };

        return _data;
    }

    private void NormalizeRotation()
    {
        if (_data == null)
            CreateEmptyData();

        _data.rotation = NormalizeQuaternion(_data.rotation);
        _status = "Rotation normalized locally. Click Save to persist.";
    }

    private Quaternion NormalizeQuaternion(Quaternion q)
    {
        float magnitude = Mathf.Sqrt(
            q.x * q.x +
            q.y * q.y +
            q.z * q.z +
            q.w * q.w
        );

        if (magnitude <= Mathf.Epsilon)
            return Quaternion.identity;

        return new Quaternion(
            q.x / magnitude,
            q.y / magnitude,
            q.z / magnitude,
            q.w / magnitude
        );
    }

    private void UseSceneViewCamera()
    {
        if (SceneView.lastActiveSceneView == null ||
            SceneView.lastActiveSceneView.camera == null)
        {
            _status = "No active Scene View camera found.";
            return;
        }

        var camTransform = SceneView.lastActiveSceneView.camera.transform;

        if (_data == null)
            CreateEmptyData();

        _data.hasSavedLocation = true;
        _data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        _data.position = camTransform.position;
        _data.rotation = camTransform.rotation;

        _status = "Copied Scene View camera transform locally. Click Save to persist.";
    }

    private void UseSelectedTransform()
    {
        if (Selection.activeTransform == null)
        {
            _status = "No Transform selected.";
            return;
        }

        var selected = Selection.activeTransform;

        if (_data == null)
            CreateEmptyData();

        _data.hasSavedLocation = true;
        _data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        _data.position = selected.position;
        _data.rotation = selected.rotation;

        _status = $"Copied selected Transform '{selected.name}' locally. Click Save to persist.";
    }

    private void RevealFile()
    {
        string path = _saveService.GetDebugPath(SaveType.PlayerLocation);
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