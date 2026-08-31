using System;

namespace Core.Weapon
{
    public sealed class WeaponDefinition
    {
        public readonly WeaponID ID;
        public readonly ulong Tags;
        private readonly WeaponSettings Settings;
        internal readonly WeaponModule[] Modules;

        public WeaponDefinition(WeaponID id, ulong tags, WeaponSettings settings, WeaponModule[] modules)
        {
            ID = id;
            Tags = tags;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings), "Weapon definiton ctor failed! settings missing!?");
            Modules = modules ?? throw new ArgumentNullException(nameof(settings), "Weapon definiton ctor failed! modules missing!?");
        }
        public WeaponDefinition(WeaponEntry entry) : this(entry.ID, entry.Tags.CreateMask(), entry.Settings, entry.Modules) { }

        public WeaponSettings ExportSettings() => Settings.Clone();
    }
}
