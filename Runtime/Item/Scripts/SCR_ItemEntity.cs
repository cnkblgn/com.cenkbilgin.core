using UnityEngine;

namespace Core.Item
{
    [DisallowMultipleComponent]
    public sealed class ItemEntity : MonoBehaviour
    {
        public ItemID ID => id; [SerializeField, HideInInspector] internal ItemID id;

        [Header("_")]       
        [Info("Toggle 'overrideData' only if you want to override data.")]
        [SerializeField] private bool overrideData;
        [Info("Toggle 'overrideStack' only if you want to override stack.")]
        [SerializeField, Min(0)] private int overrideStack;

        private ItemData thisData;
        private IItemHandler thisHandler;

        private void Awake()
        {
            thisHandler = GetComponent<IItemHandler>();
            thisData = id.CreateData();

            if (overrideStack != 0)
            {
                thisData.SetStack(overrideStack);
            }

            if (!overrideData)
            {
                thisHandler?.HandleImport(id, thisData.Data);
            }
        }

        public void ExportTo(out ItemData data)
        {
            thisHandler?.HandleExport(id, this.thisData.Data);
            data = new(this.thisData);
        }
        public void ImportFrom(ItemData data)
        {
            this.thisData = new(data);
            thisHandler?.HandleImport(id, this.thisData.Data);
        }
    }
}
