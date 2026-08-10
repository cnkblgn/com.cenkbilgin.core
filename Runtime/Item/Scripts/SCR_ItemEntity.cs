using UnityEngine;

namespace Core.Item
{
    [DisallowMultipleComponent]
    public sealed class ItemEntity : MonoBehaviour
    {
        public ItemID ID => id;

        [Header("_")]
        [Info("Toggle 'overrideData' only if you want to keep overrides.")]
        [SerializeField] private ItemID id;
        [SerializeField] private bool overrideData;

        private ItemData thisData;
        private IItemHandler thisHandler;

        private void Awake()
        {
            thisHandler = GetComponent<IItemHandler>();
            thisData = id.CreateData();

            if (!overrideData)
            {
                thisHandler?.HandleImport(thisData.Data);
            }
        }

        public void ExportTo(out ItemData data)
        {
            thisHandler?.HandleExport(this.thisData.Data);
            data = new(this.thisData);
        }
        public void ImportFrom(ItemData data)
        {
            this.thisData = new(data);
            thisHandler?.HandleImport(this.thisData.Data);
        }

#if UNITY_EDITOR
        public void Override(ItemID id) => this.id = id;
#endif
    }
}
