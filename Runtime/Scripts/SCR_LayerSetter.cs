using UnityEngine;

namespace Core
{
    using static CoreUtility;

    internal sealed class LayerSetter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private int layer = 0;
        [SerializeField] private bool includeChildren = false;

        private void Start() => gameObject.SetLayer(layer, includeChildren);
    }
}
