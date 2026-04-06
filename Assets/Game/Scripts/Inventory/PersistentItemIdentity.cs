using UnityEngine;
//IMPORTANT: Generate the ID once in the inspector and keep it forever.
public class PersistentItemIdentity : MonoBehaviour
{
    [SerializeField] private string itemId;
    [SerializeField] private PersistentItemType itemType;

    public string ItemId => itemId;
    public PersistentItemType ItemType => itemType;

#if UNITY_EDITOR
    [ContextMenu("Generate New ID")]
    private void GenerateNewId()
    {
        itemId = System.Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}