using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Marks a [SerializeReference] field (or array/List of one) so it gets the
    /// "Add Component"-style type search dropdown drawn by EditorDrawReference.
    /// </summary>

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class Reference : PropertyAttribute { }
}
