using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Event
{
    public static class EventDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static EventID[] ids = Array.Empty<EventID>();
        private static List<Action<EventContext>>[] listeners = Array.Empty<List<Action<EventContext>>>();
        private static int invokeDepth;
        private static bool isDebugEnabled;

        internal static void Build(string[] _ids)
        {
            if (_ids == null)
            {
                return;
            }

            Debug.Log("Event Build test0!");

            invokeDepth = 0;
            idLookup.Clear();
            listeners = new List<Action<EventContext>>[_ids.Length];
            ids = new EventID[_ids.Length];

            for (int i = 0; i < _ids.Length; i++)
            {
                string key = _ids[i];

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Event database id key is invalid!?");
                    continue;
                }

                idLookup[key] = i;
                ids[i] = new(key, i);
                listeners[i] = new();
            }

            Debug.Log("Event Build test1!");
            Debug.Log($"Event database build successfull!");
        }
        public static void Clear()
        {
            if (invokeDepth > 0)
            {
                Debug.LogError("Event database clear failed! Cannot clear while invoking!");
                return;
            }

            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].Clear();
            }

#if UNITY_EDITOR
            if (isDebugEnabled)
            {
                Debug.Log("Event database cleared!");
            }
#endif
        }
        public static void Subscribe(EventID id, Action<EventContext> callback)
        {
            Debug.Log("Subscribe test0! " + id);

            if (invokeDepth > 0)
            {
                Debug.LogError($"Event database subscribe failed! Cannot subscribe to event [{id}] while invoking.");
                return;
            }

            Debug.Log("Subscribe test1! " + id);

            if (!IsValid(id))
            {
                return;
            }

            Debug.Log("Subscribe test2! " + id);

            if (callback == null)
            {
                Debug.LogError($"Event database subscribe failed! Cannot subscribe null event [{id}].");
                return;
            }

            Debug.Log("Subscribe test3! " + id);

            listeners[id.Index].Add(callback);

#if UNITY_EDITOR
            if (isDebugEnabled)
            {
                Debug.Log($"Event database event subscribed! [{id}]");
            }           
#endif
        }
        public static void Unsubscribe(EventID id, Action<EventContext> callback)
        {
            if (invokeDepth > 0)
            {
                Debug.LogError($"Event database unsubscribe failed! Cannot unsubscribe fromt event [{id}] while invoking.");
                return;
            }

            if (!IsValid(id))
            {
                return;
            }

            if (callback == null)
            {
                Debug.LogError($"Event database unsubscribe failed! Cannot unsubscribe null callback [{id}].");
                return;
            }

            listeners[id.Index].Remove(callback);

#if UNITY_EDITOR
            if (isDebugEnabled)
            {
                Debug.Log($"Event database event unsubscribed! [{id}]");
            }
#endif
        }
        public static void Invoke(EventID id, int actor, ulong tags, int amount = 1) => Invoke(new EventContext(id, actor, tags, amount));
        public static void Invoke(EventID id, int actor, int amount = 1) => Invoke(new EventContext(id, actor, amount));
        public static void Invoke(EventID id, int amount = 1) => Invoke(new EventContext(id, amount));
        public static void Invoke(EventContext @event)
        {
            if (!IsValid(@event.ID))
            {
                return;
            }

            List<Action<EventContext>> list = listeners[@event.ID.Index];

            if (list.Count == 0)
            {
                return;
            }

            invokeDepth++;

#if UNITY_EDITOR
            if (isDebugEnabled)
            {
                Debug.Log
                (
                    $"Event: {@event.ID.Key} " +
                    $"Actor: {@event.Actor} " +
                    $"Tags: {@event.Tags} " +
                    $"Amount: {@event.Amount} " +
                    $"Listeners: {list.Count}"
                );
            }
#endif

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Invoke(@event);
                }
            }
            finally
            {
                invokeDepth--;
            }
        }

        public static bool EnableDebug() => isDebugEnabled = true;
        public static bool DisableDebug() => isDebugEnabled = false;

        public static bool HasListener(EventID id) => IsValid(id) && listeners[id.Index].Count > 0;
        private static bool IsValid(EventID id) => id.IsValid && (uint)id.Index < (uint)listeners.Length;

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static EventID GetID(int index)
        {
            if (index >= ids.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Event id not found index out of range");
            }

            return ids[index];
        }
        public static IReadOnlyList<EventID> GetIDs() => ids;
    }
}
