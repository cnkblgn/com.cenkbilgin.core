using System.Collections.Generic;
using UnityEngine;
using Core.Input;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIKeyActionController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UIKeyActionElement actionTemplate = null;
        [SerializeField, Required] private RectTransform actionContainer = null;

        private Canvas thisCanvas = null;
        private readonly Dictionary<UIKeyActionType, List<UIKeyActionElement>> instanceTable = new();
        private readonly Dictionary<UIKeyActionType, KeyActionData[]> dataTable = new();
        private bool isEnabled = true;

        private void Awake() => thisCanvas = GetComponent<Canvas>();
        private void Start()
        {
            ManagerCoreInput input = ManagerCoreInput.Instance;

            Insert(UIKeyActionType.DEFAULT, new KeyActionData[]
            {
                new("Primary", input.GetIcon(input.KeyPrimary)),
                new("Secondary", input.GetIcon(input.KeySecondary)),
            });
        }

        public bool IsActive(UIKeyActionType id) => instanceTable.TryGetValue(id, out var _);
        public void Show(UIKeyActionType id)
        {
            if (!isEnabled)
            {
                return;
            }

            if (!dataTable.TryGetValue(id, out KeyActionData[] keys))
            {
                Debug.LogError($"UIKeyActionController.Show() undefined type [{id}]");
                return;
            }

            thisCanvas.Show();

            List<UIKeyActionElement> instances = new(keys.Length);

            for (int i = 0; i < keys.Length; i++)
            {
                UIKeyActionElement instance = Instantiate(actionTemplate, actionContainer);
                instance.Initialize(keys[i].Icon, keys[i].Description);
                instance.gameObject.SetActive(true);
                instances.Add(instance);
            }

            instanceTable[id] = instances;
        }
        public void Hide(UIKeyActionType id)
        {
            if (!isEnabled)
            {
                return;
            }

            if (!instanceTable.TryGetValue(id, out var keys))
            {
                Debug.LogError($"UIKeyActionController.Show() undefined type [{id}]");
                return;
            }

            foreach (var key in keys)
            {
                if (key != null)
                {
                    Destroy(key.gameObject);
                }
            }

            instanceTable.Remove(id);

            if (instanceTable.Count == 0)
            {
                thisCanvas.Hide();
            }
        }

        public void Insert(UIKeyActionType id, KeyActionData[] data)
        {
            if (data == null)
            {
                return;
            }

            if (data.Length <= 0)
            {
                Debug.LogError($"UIKeyActionController.Insert() data.Length <= 0 [{id}]");
                return;
            }

            if (dataTable.TryGetValue(id, out _))
            {
                Debug.LogError($"UIKeyActionController.Insert() type exists [{id}]");
                return;
            }

            dataTable.Add(id, data);
        }
        public void Remove(UIKeyActionType id)
        {
            if (!dataTable.TryGetValue(id, out _))
            {
                Debug.LogError($"UIKeyActionController.Remove() type not found [{id}]");
                return;
            }

            dataTable.Remove(id);
        }

        public void Clear()
        {
            thisCanvas.Hide();

            foreach (var i in instanceTable)
            {
                foreach (var j in i.Value)
                {
                    if (j != null)
                    {
                        Destroy(j.gameObject);
                    }
                }
            }

            instanceTable.Clear();
        }

        public void Enable()
        {
            isEnabled = true;
        }
        public void Disable()
        {
            isEnabled = false;

            Clear();
        }
    }
}