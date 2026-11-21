using System;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Environment
{
    /// <summary>
    /// One place in an environment scene where a battle can be fought, for
    /// example the dungeon entrance or the great hall. A level picks a site by
    /// its SiteId.
    ///
    /// The site's own transform is the hero battle position: place the object
    /// where the hero fights and turn its forward (blue) axis toward where the
    /// enemies should stand, on flat ground. The intro walk starts at
    /// HeroIntroOffset from there (in the site's local space; the default is
    /// 3 units behind the hero). Enemies are laid out ahead of the hero by the
    /// battle's FormationLayoutConfig.
    ///
    /// Optionally lists the environment sections that must stay active for
    /// this site; others can be disabled for performance.
    /// </summary>
    public sealed class BattleSite : MonoBehaviour
    {
        // CONSTANTS

        private const float GizmoFrontRowDistance = 5f;
        private const float GizmoRadius = 0.4f;

        // FIELDS

        [Tooltip("Id used by LevelData to pick this site, e.g. 'Site_01'.")]
        [SerializeField] private string _siteId = "Site";

        [Header("Hero")]
        [Tooltip("Intro walk start, relative to this site (local space). Z < 0 is behind the hero.")]
        [SerializeField] private Vector3 _heroIntroOffset = new Vector3(0f, 0f, -3f);

        [Header("Sections (optional)")]
        [Tooltip("Environment sections that must be active for this site. When empty, sections are not changed.")]
        [SerializeField] private GameObject[] _activeSections = Array.Empty<GameObject>();

        // PROPERTIES

        /// <summary>Id used by LevelData to pick this site.</summary>
        public string SiteId => _siteId;

        /// <summary>Where the hero fights (this site's position).</summary>
        public Vector3 HeroBattlePosition => transform.position;

        /// <summary>Hero facing in battle: this site's forward axis, kept level.</summary>
        public Quaternion HeroBattleRotation => Quaternion.LookRotation(Forward);

        /// <summary>Horizontal direction from the hero toward the enemies.</summary>
        public Vector3 Forward
        {
            get
            {
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            }
        }

        /// <summary>Where the intro walk starts.</summary>
        public Vector3 HeroIntroPosition => transform.TransformPoint(_heroIntroOffset);

        /// <summary>Sections that must be active for this site; empty means no change.</summary>
        public GameObject[] ActiveSections => _activeSections;

        // UNITY CALLBACKS

        /// <summary>Draws the hero battle position, the intro start and the direction to the enemies.</summary>
        private void OnDrawGizmos()
        {
            Vector3 battlePosition = HeroBattlePosition;
            Vector3 frontRow = battlePosition + Forward * GizmoFrontRowDistance;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(battlePosition, GizmoRadius);
            Gizmos.DrawLine(battlePosition, frontRow);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(HeroIntroPosition, GizmoRadius);
            Gizmos.DrawLine(HeroIntroPosition, battlePosition);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(frontRow, new Vector3(1f, 0.1f, 1f));
        }
    }
}
