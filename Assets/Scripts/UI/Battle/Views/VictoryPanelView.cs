using System;
using DG.Tweening;
using MightyHeroes.Gameplay.Level;
using MightyHeroes.UI.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Victory screen content: plays the reveal animation and shows the star
    /// rating of the win. Only the earned stars are shown, centered by the star
    /// row's layout group; all of them and their glows use the tier color
    /// (3 gold, 2 silver, 1 bronze). Every reference is optional, so
    /// the layout can change without code changes.
    ///
    /// Sequence: ribbon drops in, emblem pops, laurels open, rating text punches
    /// in, stars pop one by one, then the turns text and the Collect Rewards
    /// button appear. Runs on unscaled time.
    /// </summary>
    public sealed class VictoryPanelView : MonoBehaviour
    {
        // TYPES

        /// <summary>
        /// One star of the row: a slot the layout group positions, the star icon
        /// and an optional glow image behind it (pulsed by UIGlowPulse if present).
        /// </summary>
        [Serializable]
        public struct StarSlot
        {
            /// <summary>Slot root; hidden when the star is not earned, scaled by the pop animation.</summary>
            public RectTransform Root;

            /// <summary>Star image. Use a light sprite so the tier color shows.</summary>
            public Image Icon;

            /// <summary>Optional soft glow behind the star, tinted with the tier color.</summary>
            public Image Glow;
        }

        // FIELDS

        [Header("Header")]
        [Tooltip("Ribbon or banner that drops in from above.")]
        [SerializeField] private RectTransform _ribbon;

        [Tooltip("Emblem on the ribbon, popped in after the drop.")]
        [SerializeField] private RectTransform _emblem;

        [SerializeField] private RectTransform _laurelLeft;
        [SerializeField] private RectTransform _laurelRight;

        [Header("Rating")]
        [Tooltip("Small title above the rating, e.g. 'Battle Rating'. Faded in.")]
        [SerializeField] private CanvasGroup _ratingTitle;

        [Tooltip("Rating word, e.g. PERFECT!")]
        [SerializeField] private TMP_Text _ratingText;

        [SerializeField] private string _threeStarRating = "PERFECT!";
        [SerializeField] private string _twoStarRating = "GREAT!";
        [SerializeField] private string _oneStarRating = "GOOD!";

        [Header("Stars")]
        [Tooltip("Star slots in display order. Unearned slots are hidden, so the layout group centers the rest.")]
        [SerializeField] private StarSlot[] _starSlots = Array.Empty<StarSlot>();

        [Tooltip("Width and height of each star.")]
        [SerializeField] private float _starSize = 200f;

        [Tooltip("Glow size as a multiple of the star size.")]
        [SerializeField] private float _glowSizeMultiplier = 1.7f;

        [Tooltip("Glow alpha; the color comes from the tier.")]
        [Range(0f, 1f)]
        [SerializeField] private float _glowAlpha = 0.8f;

        [Tooltip("Gap between stars. Negative values overlap them slightly.")]
        [SerializeField] private float _starSpacing = -10f;

        [SerializeField] private Color _goldColor = new Color(1f, 0.82f, 0.2f, 1f);
        [SerializeField] private Color _silverColor = new Color(0.82f, 0.86f, 0.9f, 1f);
        [SerializeField] private Color _bronzeColor = new Color(0.8f, 0.5f, 0.25f, 1f);

        [Header("Footer")]
        [Tooltip("Turns text. {0} is the number of turns.")]
        [SerializeField] private TMP_Text _turnsText;

        [SerializeField] private string _turnsFormat = "{0} turns taken";

        [SerializeField] private Button _collectRewardsButton;

        [Header("Timing")]
        [Tooltip("Pause between two star pops in seconds.")]
        [SerializeField] private float _starInterval = 0.1f;

        [Tooltip("Ribbon drop distance in UI units.")]
        [SerializeField] private float _ribbonDropDistance = 600f;

        private Sequence _sequence;

        // PROPERTIES

        /// <summary>Raised when the Collect Rewards button is pressed.</summary>
        public event Action CollectRewardsClicked;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_collectRewardsButton != null)
                _collectRewardsButton.onClick.AddListener(OnCollectRewardsButtonClicked);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Layout changes are not allowed inside OnValidate; apply on the next editor tick.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                    ApplyStarLayout();
            };
#endif
        }

        private void OnDestroy()
        {
            _sequence?.Kill();

            if (_collectRewardsButton != null)
                _collectRewardsButton.onClick.RemoveListener(OnCollectRewardsButtonClicked);
        }

        // PUBLIC METHODS

        /// <summary>
        /// Fills the panel with the result, hides the animated parts at once and
        /// plays the reveal animation after the given delay, so it starts when
        /// the result canvas has become visible.
        /// </summary>
        /// <param name="result">Turns taken and stars earned.</param>
        /// <param name="startDelay">Seconds to wait before the first animation.</param>
        public void Play(LevelResult result, float startDelay)
        {
            int stars = Mathf.Clamp(result.Stars, 1, BattleStarRating.MaxStars);

            ApplyContent(result.TurnsTaken, stars);

            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetUpdate(true);
            _sequence.AppendInterval(startDelay);

            AppendHeader();
            AppendRating();
            AppendStars(stars);
            AppendFooter();
        }

        // PRIVATE METHODS

        /// <summary>Sets texts, star visibility and star color for the result.</summary>
        private void ApplyContent(int turnsTaken, int stars)
        {
            if (_ratingText != null)
                _ratingText.text = stars >= 3 ? _threeStarRating : stars == 2 ? _twoStarRating : _oneStarRating;

            if (_turnsText != null)
                _turnsText.text = string.Format(_turnsFormat, turnsTaken);

            ApplyStarLayout();

            Color tierColor = stars >= 3 ? _goldColor : stars == 2 ? _silverColor : _bronzeColor;

            Color glowColor = new Color(tierColor.r, tierColor.g, tierColor.b, _glowAlpha);

            for (int i = 0; i < _starSlots.Length; i++)
            {
                StarSlot slot = _starSlots[i];

                if (slot.Root == null)
                    continue;

                slot.Root.gameObject.SetActive(i < stars);
                slot.Root.localScale = Vector3.zero;

                if (slot.Icon != null)
                    slot.Icon.color = tierColor;

                if (slot.Glow != null)
                    ApplyGlowColor(slot.Glow, glowColor);
            }
        }

        /// <summary>Applies the star size and the gap of the star row's layout group.</summary>
        private void ApplyStarLayout()
        {
            HorizontalLayoutGroup row = null;

            foreach (StarSlot slot in _starSlots)
            {
                if (slot.Root == null)
                    continue;

                slot.Root.sizeDelta = new Vector2(_starSize, _starSize);

                if (slot.Glow != null)
                    slot.Glow.rectTransform.sizeDelta = Vector2.one * (_starSize * _glowSizeMultiplier);

                if (row == null && slot.Root.parent != null)
                    row = slot.Root.parent.GetComponent<HorizontalLayoutGroup>();
            }

            if (row != null)
                row.spacing = _starSpacing;
        }

        /// <summary>Tints a star glow, through its UIGlowPulse when it has one so the pulse keeps the color.</summary>
        private static void ApplyGlowColor(Image glow, Color color)
        {
            UIGlowPulse pulse = glow.GetComponent<UIGlowPulse>();

            if (pulse != null)
                pulse.SetGlowColor(color);
            else
                glow.color = color;
        }

        /// <summary>Ribbon drop, emblem pop and laurels opening.</summary>
        private void AppendHeader()
        {
            if (_ribbon != null)
            {
                Vector2 target = _ribbon.anchoredPosition;
                _ribbon.anchoredPosition = target + Vector2.up * _ribbonDropDistance;
                _sequence.Append(_ribbon.DOAnchorPos(target, 0.45f).SetEase(Ease.OutBack));
            }

            if (_emblem != null)
            {
                _emblem.localScale = Vector3.zero;
                _sequence.Append(_emblem.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }

            AppendLaurel(_laurelLeft, 35f);
            AppendLaurel(_laurelRight, -35f);
        }

        /// <summary>Opens a laurel from a tilted, hidden pose, together with the other one.</summary>
        private void AppendLaurel(RectTransform laurel, float startAngle)
        {
            if (laurel == null)
                return;

            Vector3 targetRotation = laurel.localEulerAngles;
            laurel.localScale = Vector3.zero;
            laurel.localEulerAngles = targetRotation + new Vector3(0f, 0f, startAngle);

            _sequence.Join(laurel.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            _sequence.Join(laurel.DOLocalRotate(targetRotation, 0.35f).SetEase(Ease.OutQuad));
        }

        /// <summary>Title fade and rating word punch.</summary>
        private void AppendRating()
        {
            if (_ratingTitle != null)
            {
                _ratingTitle.alpha = 0f;
                _sequence.Append(_ratingTitle.DOFade(1f, 0.2f));
            }

            if (_ratingText != null)
            {
                RectTransform rect = _ratingText.rectTransform;
                rect.localScale = Vector3.one * 2f;
                _ratingText.alpha = 0f;

                _sequence.Append(rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                _sequence.Join(FadeText(_ratingText, 0.15f));
            }
        }

        /// <summary>Pops the earned stars one after another.</summary>
        private void AppendStars(int stars)
        {
            for (int i = 0; i < _starSlots.Length && i < stars; i++)
            {
                if (_starSlots[i].Root == null)
                    continue;

                _sequence.Append(_starSlots[i].Root.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

                if (i < stars - 1)
                    _sequence.AppendInterval(_starInterval);
            }
        }

        /// <summary>Turns text fade and Collect Rewards button pop.</summary>
        private void AppendFooter()
        {
            if (_turnsText != null)
            {
                _turnsText.alpha = 0f;
                _sequence.Append(FadeText(_turnsText, 0.2f));
            }

            if (_collectRewardsButton != null)
            {
                Transform button = _collectRewardsButton.transform;
                button.localScale = Vector3.zero;
                _collectRewardsButton.interactable = false;

                _sequence.Append(button.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                _sequence.AppendCallback(() => _collectRewardsButton.interactable = true);
            }
        }

        /// <summary>Fades a TMP text in from its current alpha without the DOTween TMP module.</summary>
        private static Tween FadeText(TMP_Text text, float duration)
        {
            return DOVirtual.Float(text.alpha, 1f, duration, value => text.alpha = value);
        }

        // EVENT HANDLERS

        private void OnCollectRewardsButtonClicked()
        {
            _collectRewardsButton.interactable = false;
            CollectRewardsClicked?.Invoke();
        }
    }
}
