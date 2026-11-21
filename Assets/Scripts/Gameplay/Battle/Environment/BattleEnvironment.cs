using System;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Environment
{
    /// <summary>
    /// Root component of an environment scene (for example Env_Dungeon). The
    /// environment scene holds only art, lighting and battle sites; all battle
    /// systems live in BattleScene, which is loaded additively on top of it.
    ///
    /// Lists the battle sites of the scene and the large sections that a site
    /// may switch on or off. When a site lists its own sections, all listed
    /// environment sections are disabled first and only the site's sections
    /// are enabled, so the device does not render the whole dungeon.
    /// </summary>
    public sealed class BattleEnvironment : MonoBehaviour
    {
        // FIELDS

        [Tooltip("Battle sites in this environment.")]
        [SerializeField] private BattleSite[] _sites = Array.Empty<BattleSite>();

        [Tooltip("All large sections of the environment that sites may switch on or off. Optional.")]
        [SerializeField] private GameObject[] _sections = Array.Empty<GameObject>();

        // PUBLIC METHODS

        /// <summary>Finds a site by id. An empty id returns the first site.</summary>
        /// <param name="siteId">Site id from LevelData.</param>
        /// <param name="site">The site when found.</param>
        public bool TryGetSite(string siteId, out BattleSite site)
        {
            site = null;

            foreach (BattleSite candidate in _sites)
            {
                if (candidate == null)
                    continue;

                if (string.IsNullOrEmpty(siteId) || candidate.SiteId == siteId)
                {
                    site = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Activates only the sections the site needs. Does nothing when the
        /// site lists no sections.
        /// </summary>
        /// <param name="site">Site the battle is fought at.</param>
        public void ActivateSite(BattleSite site)
        {
            if (site == null || site.ActiveSections.Length == 0)
                return;

            foreach (GameObject section in _sections)
            {
                if (section != null)
                    section.SetActive(false);
            }

            foreach (GameObject section in site.ActiveSections)
            {
                if (section != null)
                    section.SetActive(true);
            }
        }
    }
}
