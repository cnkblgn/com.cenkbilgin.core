using System;
using System.Reflection;

namespace Core.Weapon
{
    public sealed class WeaponDefinition
    {
        public readonly WeaponID ID;
        public readonly ulong Tags;
        private readonly WeaponSettings settings;
        private readonly WeaponModule[] modules;

        public WeaponDefinition(WeaponID id, ulong tags, WeaponSettings settings, WeaponModule[] modules)
        {
            ID = id;
            Tags = tags;
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings), "Weapon definiton ctor failed! settings missing!?");
            this.modules = modules ?? throw new ArgumentNullException(nameof(modules), "Weapon definiton ctor failed! modules missing!?");
        }
        public WeaponDefinition(WeaponEntry entry) : this(entry.ID, entry.Tags.CreateMask(), entry.Settings, entry.Modules) { }

        public WeaponModule[] ExportModules()
        {
            WeaponModule[] export = new WeaponModule[modules.Length];

            for (int i = 0; i < modules.Length; i++)
            {
                export[i] = modules[i].Clone();
            }

            return export;
        }
        public WeaponSettings ExportSettings() => settings.Clone();
    }
}
