using System.Collections.Generic;
using MightyHeroes.Gameplay.Common.Actions;
using UnityEngine;

namespace MightyHeroes.Gameplay.Level
{
    /// <summary>
    /// Content definition of one battle level: the puzzle board and the enemy formation.
    ///
    /// Board: row and column count, and which tile types (actions) can spawn.
    /// Colors and icons of the tile types come from the grid's element type list;
    /// the level only selects which of them are used.
    ///
    /// Location: the environment scene (loaded first; BattleScene is loaded
    /// additively on top) and the battle site inside it.
    ///
    /// Data only. Read by the battle setup through LevelSession.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData_New", menuName = "Mighty Heroes/Content/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        // FIELDS

        [Header("Info")]
        [Tooltip("Name shown in UI, e.g. 'Battle 1'.")]
        [SerializeField] private string _displayName = "Battle";

        [Header("Location")]
        [Tooltip("Environment scene name, e.g. 'Env_Dungeon'. Must be in Build Settings. Empty plays in BattleScene alone.")]
        [SerializeField] private string _environmentSceneName = "Env_Dungeon";

        [Tooltip("Battle site id inside the environment scene, e.g. 'Entrance'. Empty uses the first site.")]
        [SerializeField] private string _battleSiteId;

        [Header("Board")]
        [SerializeField, Range(3, 8)] private int _boardRows = 4;
        [SerializeField, Range(3, 8)] private int _boardColumns = 5;

        [Tooltip("Tile types that can spawn on the board. Each type needs an entry in the grid's element type list.")]
        [SerializeField] private List<ActionType> _tileTypes = new List<ActionType> { ActionType.Attack, ActionType.Shield, ActionType.Heal };

        [Header("Enemies")]
        [Tooltip("Enemy placements. Row 0 is the front row; slot order is left to right on screen.")]
        [SerializeField] private List<LevelEnemySlot> _enemies = new List<LevelEnemySlot>();

        // PROPERTIES

        /// <summary>Name shown in UI.</summary>
        public string DisplayName => _displayName;

        /// <summary>Environment scene name, or empty to play in BattleScene alone.</summary>
        public string EnvironmentSceneName => _environmentSceneName;

        /// <summary>Battle site id inside the environment scene; empty uses the first site.</summary>
        public string BattleSiteId => _battleSiteId;

        /// <summary>Puzzle board row count.</summary>
        public int BoardRows => _boardRows;

        /// <summary>Puzzle board column count.</summary>
        public int BoardColumns => _boardColumns;

        /// <summary>Tile types that can spawn on the board.</summary>
        public IReadOnlyList<ActionType> TileTypes => _tileTypes;

        /// <summary>Enemy placements of the level.</summary>
        public IReadOnlyList<LevelEnemySlot> Enemies => _enemies;
    }
}
