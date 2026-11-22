using TMPro;
using UnityEngine;

namespace MightyHeroes.UI.TextStyle
{
    /// <summary>
    /// Applies a TextStyleConfig to a TMP_Text component.
    /// Ensures consistent styling and avoids manual inspector tweaking.
    /// </summary>
    public static class TextStyleApplier
    {
        /// <summary>
        /// Applies given style config to target TMP_Text.
        /// </summary>
        public static void Apply(TMP_Text text, TextStyleConfig config)
        {
            if (text == null || config == null)
                return;

            Material mat = text.fontMaterial;

            mat.SetColor(ShaderUtilities.ID_FaceColor, config.FaceColor);

            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, config.OutlineWidth);
            mat.SetColor(ShaderUtilities.ID_OutlineColor, config.OutlineColor);

            if (config.UseUnderlay)
            {
                mat.EnableKeyword("UNDERLAY_ON");
                mat.SetColor(ShaderUtilities.ID_UnderlayColor, config.UnderlayColor);
                mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, config.UnderlaySoftness);
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, config.UnderlayOffset.x);
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, config.UnderlayOffset.y);
            }
            else
            {
                // Pooled texts reuse one material instance, so a style without
                // underlay must switch off what a previous style switched on.
                mat.DisableKeyword("UNDERLAY_ON");
            }

            if (config.UseGlow)
            {
                mat.EnableKeyword("GLOW_ON");
                mat.SetColor(ShaderUtilities.ID_GlowColor, config.GlowColor);
                mat.SetFloat(ShaderUtilities.ID_GlowPower, config.GlowPower);
            }
            else
            {
                mat.DisableKeyword("GLOW_ON");
            }
        }
    }
}
