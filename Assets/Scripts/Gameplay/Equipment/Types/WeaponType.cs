namespace MightyHeroes.Gameplay.Equipment.Types
{
    /// <summary>
    /// Defines the category of a weapon.
    /// Drives which Animator Controller, animation group suffixes,
    /// socket configuration, and effect behaviour apply at runtime.
    /// Add new values here as new weapon packs are integrated.
    /// </summary>
    public enum WeaponType
    {
        /// <summary>Single-handed sword. Uses Synty Sword Combat animation pack.</summary>
        Sword         = 1,

        /// <summary>Single-handed mace or hammer.</summary>
        Mace          = 2,

        /// <summary>Small single-handed blade. Faster attack cadence, shorter reach.</summary>
        Dagger        = 3,

        /// <summary>Two-handed sword or greatsword. Slower, higher damage.</summary>
        TwoHandedSword = 4,

        /// <summary>Bow. Uses ranged attack flow; no melee slash effects.</summary>
        Bow           = 5,

        /// <summary>Magic focus, staff, or orb. Triggers spell effects instead of slash arcs.</summary>
        MagicFocus    = 6
    }
}