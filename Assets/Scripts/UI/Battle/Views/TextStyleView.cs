using TMPro;
using UnityEngine;

/// <summary>
/// MonoBehaviour bridge that applies a TextStyleConfig on Awake.
/// </summary>
public sealed class TextStyleView : MonoBehaviour
{
    // 1) FIELDS

    [SerializeField] private TMP_Text _text;
    [SerializeField] private TextStyleConfig _style;

    // 2) UNITY

        /// <summary>

        /// Applies style when the object becomes active.

        /// </summary>

        private void Awake()

        {

            if (_text == null)

            {

                Debug.LogError("TextStyleView: TMP_Text reference is null.", this);

                return;

            }

            if (_style == null)

            {

                Debug.LogError("TextStyleView: TextStyleConfig reference is null.", this);

                return;

            }

            Debug.Log($"TextStyleView: Applying style to {_text.name}.", this);

            TextStyleApplier.Apply(_text, _style);

        }
}