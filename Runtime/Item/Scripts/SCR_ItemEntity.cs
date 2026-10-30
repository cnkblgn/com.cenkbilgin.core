using UnityEngine;

namespace Core.Item
{
    [DisallowMultipleComponent]
    public sealed class ItemEntity : MonoBehaviour
    {
        public ItemID ID => id; 

        [Header("_")]
        [SerializeField] private ItemID id;
        [Info("Toggle 'overrideData' only if you want to override data.")]
        [SerializeField] private bool overrideData;
        [Info("Toggle 'overrideStack' only if you want to override stack.")]
        [SerializeField, Min(0)] private int overrideStack;

        private ItemData thisData;
        private IItemHandler[] thisHandlers;

        private void Awake()
        {
            thisHandlers = GetComponents<IItemHandler>();
            thisData = id.CreateData();

            if (overrideStack != 0)
            {
                thisData.SetStack(overrideStack);
            }

            if (!overrideData)
            {
                for (int i = 0; i < thisHandlers.Length; i++)
                {
                    thisHandlers[i]?.HandleImport(id, thisData.Data);
                }
            }
        }

        public void ExportTo(out ItemData data)
        {
            for (int i = 0; i < thisHandlers.Length; i++)
            {
                thisHandlers[i]?.HandleExport(id, thisData.Data);
            }
            data = new(this.thisData);
        }
        public void ImportFrom(ItemData data)
        {
            this.thisData = new(data);
            for (int i = 0; i < thisHandlers.Length; i++)
            {
                thisHandlers[i]?.HandleImport(id, thisData.Data);
            }
        }
    }
}
