using UnityEngine;

public class PersistentWorldObjectIdentity : MonoBehaviour
{
    [SerializeField] private string objectId;

    public string ObjectId => objectId;

#if UNITY_EDITOR
    [ContextMenu("Generate New ID")]
    private void GenerateNewId()
    {
        objectId = System.Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}