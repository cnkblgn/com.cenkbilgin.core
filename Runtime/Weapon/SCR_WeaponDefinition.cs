using System;

namespace Core.Weapon
{
    public sealed class WeaponDefinition
    {
        public readonly WeaponID ID;
        public readonly ulong Tags;
        private readonly WeaponSettings settings;

        public WeaponDefinition(WeaponID id, ulong tags, WeaponSettings settings)
        {
            ID = id;
            Tags = tags;

            this.settings = settings ?? throw new ArgumentNullException(nameof(settings), "Weapon definiton ctor failed! settings missing!?");
        }
        public WeaponDefinition(WeaponEntry entry) : this(entry.ID, entry.Tags.CreateMask(), entry.Settings) { }

        public WeaponSettings ExportSettings() => settings.Clone();
    }
}
