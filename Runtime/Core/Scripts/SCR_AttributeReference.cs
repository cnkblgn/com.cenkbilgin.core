using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Marks a [SerializeReference] field (or array/List of one) so it gets the
    /// "Add Component"-style type search dropdown drawn by EditorDrawReference.
    /// Works on both a single field and an array/List field of the same base type:
    ///
    ///   [SerializeReference, Reference] public ItemComponent component;
    ///   [SerializeReference, Reference] public ItemComponent[] components;
    /// </summary>

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class Reference : PropertyAttribute { }
}
