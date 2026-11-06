using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Row of status icons shown on an entity's HUD (attack buff today; debuffs later).
    ///
    /// Pure presentation: values are pushed by StatusIconPresenter. Icon art is
    /// whatever GameObject/Image is assigned in the Inspector, so placeholder
    /// visuals can be swapped for final art without code changes.
    ///
    /// Awake only applies the default hidden state when no value has been pushed
    /// yet, because Awake order is undefined relative to BattleBootstrapper.
    /// </summary>
    public sealed class StatusIconBarView : MonoBehaviour
    {
        // FIELDS

        [Header("Attack Buff")]
        [Tooltip("Root object of the attack buff icon. Activated while a buff is pending.")]
        [SerializeField] private GameObject _attackBuffIcon;

        [Tooltip("Optional label that shows the pending buff percent, e.g. +25%.")]
        [SerializeField] private TMP_Text _attackBuffValueText;

        [Header("Feedback")]
        [Tooltip("Scale punch strength applied when an icon appears or its value increases.")]
        [SerializeField] private float _punchScale = 0.25f;

        [Tooltip("Duration of the scale punch in seconds.")]
        [SerializeField] private float _punchDuration = 0.25f;

        private Tween _attackBuffTween;
        private int _attackBuffPercent;
        private bool _hasOwnerState;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (!_hasOwnerState)
                ResetView();
        }

        private void OnDisable()
        {
            KillTweens();
        }

        // PUBLIC METHODS

        /// <summary>
        /// Shows or hides the attack buff icon for the given pending percent.
        /// Zero hides the icon. An increase plays a short punch for feedback.
        /// </summary>
        /// <param name="percent">Pending attack bonus percent.</param>
        public void SetAttackBuff(int percent)
        {
            _hasOwnerState = true;

            bool isIncrease = percent > _attackBuffPercent;
            _attackBuffPercent = percent;

            if (_attackBuffIcon != null)
                _attackBuffIcon.SetActive(percent > 0);

            if (_attackBuffValueText != null)
                _attackBuffValueText.text = percent > 0 ? $"+{percent}%" : string.Empty;

            if (isIncrease)
                PlayPunch(ref _attackBuffTween, _attackBuffIcon);
        }

        /// <summary>
        /// Hides every status icon. Used on entity death and as the default state.
        /// </summary>
        public void ClearAll()
        {
            _hasOwnerState = true;
            ApplyCleared();
        }

        // PRIVATE METHODS

        /// <summary>Applies the default hidden state without marking owner state.</summary>
        private void ResetView()
        {
            ApplyCleared();
        }

        private void ApplyCleared()
        {
            KillTweens();
            _attackBuffPercent = 0;

            if (_attackBuffIcon != null)
                _attackBuffIcon.SetActive(false);

            if (_attackBuffValueText != null)
                _attackBuffValueText.text = string.Empty;
        }

        /// <summary>Plays a scale punch on the given icon, replacing any running punch.</summary>
        private void PlayPunch(ref Tween tween, GameObject icon)
        {
            if (icon == null || !icon.activeInHierarchy)
                return;

            tween?.Kill(complete: true);
            icon.transform.localScale = Vector3.one;
            tween = icon.transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 6, 0.5f).SetLink(icon);
        }

        private void KillTweens()
        {
            _attackBuffTween?.Kill(complete: true);
            _attackBuffTween = null;
        }
    }
}
