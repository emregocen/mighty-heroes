using System;
using System.Collections.Generic;
using MightyHeroes.Core.Logging;
using UnityEngine;

namespace MightyHeroes.Gameplay.Battle.Environment
{
    /// <summary>
    /// Root component of an environment scene (for example Env_Dungeon). The
    /// environment scene holds only art, lighting and battle sites; all battle
    /// systems live in BattleScene, which is loaded additively on top of it.
    ///
    /// Battle sites are found automatically: every BattleSite in this object's
    /// children (active or not) is available, there is no list to maintain.
    /// Site ids must be unique; a duplicate is reported and ignored.
    ///
    /// Also lists the large sections that a site may switch on or off. When a
    /// site lists its own sections, all listed environment sections are
    /// disabled first and only the site's sections are enabled, so the device
    /// does not render the whole dungeon.
    /// </summary>
    public sealed class BattleEnvironment : MonoBehaviour
    {
        // FIELDS

        [Tooltip("All large sections of the environment that sites may switch on or off. Optional.")]
        [SerializeField] private GameObject[] _sections = Array.Empty<GameObject>();

        private readonly Dictionary<string, BattleSite> _sitesById = new Dictionary<string, BattleSite>();
        private BattleSite _firstSite;

        // UNITY CALLBACKS

        private void Awake()
        {
            CollectSites();
        }

        // PUBLIC METHODS

        /// <summary>Finds a site by id. An empty id returns the first site.</summary>
        /// <param name="siteId">Site id from LevelData.</param>
        /// <param name="site">The site when found.</param>
        public bool TryGetSite(string siteId, out BattleSite site)
        {
            if (_firstSite == null)
                CollectSites();

            if (string.IsNullOrEmpty(siteId))
            {
                site = _firstSite;
                return site != null;
            }

            return _sitesById.TryGetValue(siteId, out site);
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

        // PRIVATE METHODS

        /// <summary>Registers every BattleSite in the children by its id.</summary>
        private void CollectSites()
        {
            _sitesById.Clear();
            _firstSite = null;

            foreach (BattleSite site in GetComponentsInChildren<BattleSite>(true))
            {
                if (_firstSite == null)
                    _firstSite = site;

                if (_sitesById.ContainsKey(site.SiteId))
                {
                    GameLogger.Error(LogSourceType.BattleSystem, $"BattleEnvironment: duplicate site id '{site.SiteId}' on '{site.name}', ignored.");
                    continue;
                }

                _sitesById.Add(site.SiteId, site);
            }
        }
    }
}
