using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent, SelectionBase]
    public sealed class GameEntity : MonoBehaviour 
    {
        public Transform ThisTransform
        {
            get
            {
                if (thisTransform == null)
                {
                    thisTransform = transform;
                }

                return thisTransform;
            }
        } private Transform thisTransform = null;
    }
}