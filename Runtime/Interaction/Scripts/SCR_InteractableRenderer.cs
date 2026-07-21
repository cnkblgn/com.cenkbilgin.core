using UnityEngine;

namespace Core.Interaction
{
    [DisallowMultipleComponent]
    public class InteractableRenderer : MonoBehaviour 
    {
        [Header("_")]
        [SerializeField] private MeshRenderer overridedMesh = null;

        public MeshRenderer GetOverride() => overridedMesh;
        public void SetOverride(MeshRenderer value) => overridedMesh = value;
    }
}