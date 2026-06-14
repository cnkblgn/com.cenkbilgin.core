using System.Collections.Generic;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIKeyActionController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private UITextBox viewTemplate = null;
        [SerializeField, Required] private RectTransform viewContainer = null;

        private Canvas thisCanvas = null;
        private readonly Dictionary<UIKeyActionType, List<UITextBox>> instanceTable = new();
        private readonly Dictionary<UIKeyActionType, KeyActionData[]> dataTable = new();
        private bool isEnabled = true;

        private void Awake() => thisCanvas = GetComponent<Canvas>();
        public bool IsActive(UIKeyActionType id) => instanceTable.TryGetValue(id, out var _);
        public void Show(UIKeyActionType id)
        {
            if (!isEnabled)
            {
                return;
            }

            if (!dataTable.TryGetValue(id, out KeyActionData[] keys))
            {
                Debug.LogError($"Undefined type [{id}]");
                return;
            }

            if (instanceTable.ContainsKey(id))
            {
                return;
            }

            thisCanvas.Show();

            List<UITextBox> instances = new(keys.Length);

            for (int i = 0; i < keys.Length; i++)
            {
                UITextBox instance = Instantiate(viewTemplate, viewContainer);
                instance.Set($"{GetSprite(keys[i].Icon)}: {keys[i].Description}");
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
                Debug.LogError($"Undefined type [{id}]");
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
                Debug.LogError($"data.Length <= 0 [{id}]");
                return;
            }

            if (dataTable.TryGetValue(id, out _))
            {
                Debug.LogError($"Type exists for [{id}]");
                return;
            }

            dataTable.Add(id, data);
        }
        public void Remove(UIKeyActionType id)
        {
            if (!dataTable.TryGetValue(id, out _))
            {
                Debug.LogError($"Type not found for [{id}]");
                return;
            }

            dataTable.Remove(id);
        }
        public void Clear()
        {
            thisCanvas.Hide();

            foreach (var table in instanceTable)
            {
                foreach (var element in table.Value)
                {
                    if (element != null)
                    {
                        Destroy(element.gameObject);
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