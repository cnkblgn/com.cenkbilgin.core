using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Persistence
{
    [DisallowMultipleComponent]
    internal sealed class PersistentScene : MonoBehaviour
    {
        [Header("_")]
        [SerializeField, Required] private List<PersistentEntity> entities = new();

        private void Awake()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                PersistentEntity entity = entities[i];

                if (entity == null)
                {
                    throw new Exception($"Persistent entity is null at [{i}] index!");
                }

                PersistentDatabase.TryRegister(entity);
            }
        }

#if UNITY_EDITOR
        [Clickable("Populate")]
        public void Populate()
        {
            entities = GetComponentsInChildren<PersistentEntity>(true).ToList();
            var groups = entities.GroupBy(e => e.InstanceID).Where(g => g.Count() > 1).ToList();

            foreach (var entity in entities)
            {
                entity.GenerateID();

                UnityEditor.EditorUtility.SetDirty(entity);
            }

            foreach (var group in groups)
            {
                foreach (var entity in group.Skip(1))
                {
                    entity.GenerateID(true);

                    UnityEditor.EditorUtility.SetDirty(entity);

                    Debug.Log($"Duplicate GUID fixed: [{group.Key}] on object [{entity.name}], new GUID: [{entity.InstanceID}]");
                }
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.Log($"Collected {entities.Count} persistent entities!");
        }
#endif
    }
}