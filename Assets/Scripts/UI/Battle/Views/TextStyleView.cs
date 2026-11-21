using MightyHeroes.Core.Logging;
using TMPro;
using UnityEngine;

namespace MightyHeroes.UI.Battle.Views
{
    /// <summary>
    /// Applies a TextStyleConfig on Awake to the TMP text component on the same
    /// GameObject. The text component is found automatically.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TextStyleView : MonoBehaviour
    {
        // FIELDS

        [SerializeField] private TextStyleConfig _style;

        // UNITY CALLBACKS

        private void Awake()
        {
            if (_style == null)
            {
                GameLogger.Error(LogSourceType.UI, $"TextStyleView on '{name}': TextStyleConfig reference is missing.");
                return;
            }

            TextStyleApplier.Apply(GetComponent<TMP_Text>(), _style);
        }
    }
}
