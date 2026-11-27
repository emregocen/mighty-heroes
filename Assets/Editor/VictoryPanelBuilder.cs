using System;
using System.Linq;
using MightyHeroes.UI.Battle;
using MightyHeroes.UI.Battle.Views;
using MightyHeroes.UI.MainMenu;
using MightyHeroes.UI.TextStyle;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MightyHeroes.EditorTools
{
    /// <summary>
    /// Builds the victory and reward panel hierarchy under the Victory UI Canvas
    /// and wires VictoryPanelView, RewardPanelView and BattleResultUIController.
    ///
    /// Non-destructive: an element that already exists (matched by name inside
    /// its panel) is never changed; only missing elements and missing components
    /// are created, and only references are assigned. New elements get the
    /// layout, sprites, fonts and styles of the approved design (27 Nov 2026),
    /// so a rebuilt panel looks like the current one. One Undo reverts a run.
    /// </summary>
    public static class VictoryPanelBuilder
    {
        // CONSTANTS

        private const string CanvasName = "Victory UI Canvas";
        private const int StarCount = 3;

        private const string SyntySprites = "Assets/ThirdParty/Synty/InterfaceFantasyWarriorHUD/Sprites/";
        private const string RibbonSpritePath = SyntySprites + "FantasyWarrior/SPR_FantasyWarrior_Banner01.png";
        private const string EmblemSpritePath = SyntySprites + "FantasyWarrior/SPR_FantasyWarrior_Frame_Top03.png";
        private const string ButtonBackgroundSpritePath = SyntySprites + "HUD/SPR_HUD_FantasyWarrior_Gradient_Vertical_Solid01.png";
        private const string ButtonFrameSpritePath = SyntySprites + "FantasyWarrior/SPR_FantasyWarrior_Frame_Box21_Variant01.png";
        private const string StarSpritePath = "Assets/Art/Icons/Icon_Star_Faceted.png";
        private const string StarGlowSpritePath = "Assets/Art/Icons/Icon_Star_Glow.png";
        private const string ButtonGlowSpritePath = "Assets/Art/Puzzle/Sprites/Tile_Glow_Square.png";
        private const string FontPath = "Assets/ThirdParty/Synty/InterfaceFantasyWarriorHUD/Fonts/Grenze/Grenze-SemiBold SDF.asset";
        private const string RatingStylePath = "Assets/Configs/Text Styles/TextStyle_Rating.asset";

        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        private static readonly Color EmblemColor = new Color(0.807f, 0.875f, 1f, 1f);
        private static readonly Color StarGlowColor = new Color(1f, 0.85f, 0.3f, 0.8f);
        private static readonly Color ButtonGlowColor = new Color(1f, 0.576f, 0f, 0.753f);
        private static readonly Color ButtonBackgroundColor = new Color(0f, 0.13f, 0.236f, 0.98f);
        private static readonly Color ButtonFrameColor = new Color(1f, 1f, 1f, 0.824f);

        // PUBLIC METHODS

        /// <summary>Builds or completes the panels on the selected (or the scene's) Victory UI Canvas.</summary>
        [MenuItem("Mighty Heroes/UI/Build Victory Panel")]
        public static void BuildVictoryPanel()
        {
            Transform canvas = FindCanvas();

            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Build Victory Panel", $"Select the '{CanvasName}' object in the BattleScene hierarchy and run the command again.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Build Victory Panel");
            int undoGroup = Undo.GetCurrentGroup();

            VictoryPanelView victoryPanel = BuildVictoryPanel(canvas);
            RewardPanelView rewardPanel = BuildRewardPanel(canvas);
            WireResultController(victoryPanel, rewardPanel);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = victoryPanel.gameObject;

            Debug.Log("Mighty Heroes: victory and reward panels are complete. Existing elements were not changed.");
        }

        // PRIVATE METHODS — Victory panel

        /// <summary>Creates the missing victory elements and wires VictoryPanelView.</summary>
        private static VictoryPanelView BuildVictoryPanel(Transform canvas)
        {
            RectTransform panel = FindOrCreate(canvas, canvas, "Victory Panel", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero, out _);

            RectTransform header = FindOrCreate(panel, panel, "Header", TopCenter, TopCenter, TopCenter, Vector2.zero, new Vector2(700f, 560f), out bool isNewHeader);

            if (isNewHeader)
                header.SetAsFirstSibling();

            RectTransform laurelLeft = FindOrCreateImage(panel, header, "Laurel Left", TopCenter, Center, new Vector2(-200f, -260f), new Vector2(170f, 340f), null, Color.white, Image.Type.Simple, out bool isNewLaurelLeft);
            RectTransform laurelRight = FindOrCreateImage(panel, header, "Laurel Right", TopCenter, Center, new Vector2(200f, -260f), new Vector2(170f, 340f), null, Color.white, Image.Type.Simple, out bool isNewLaurelRight);

            if (isNewLaurelRight)
                laurelRight.localEulerAngles = new Vector3(0f, 180f, 0f);

            // Laurels have no sprite yet; they start hidden like in the current design.
            SetInactiveIfNew(laurelLeft, isNewLaurelLeft);
            SetInactiveIfNew(laurelRight, isNewLaurelRight);

            RectTransform ribbon = FindOrCreateImage(panel, header, "Ribbon", TopCenter, TopCenter, Vector2.zero, new Vector2(300f, 500f), Load<Sprite>(RibbonSpritePath), Color.white, Image.Type.Sliced, out _);
            RectTransform emblem = FindOrCreateImage(panel, ribbon, "Emblem", Center, Center, new Vector2(0f, 110f), new Vector2(300f, 300f), Load<Sprite>(EmblemSpritePath), EmblemColor, Image.Type.Simple, out _);

            TMP_Text ratingTitle = FindOrCreateText(panel, panel, "Rating Title", BottomCenter, new Vector2(0f, 640f), new Vector2(500f, 60f), "Battle Rating", 40f, FontStyles.Normal, null, out bool isNewRatingTitle);
            CanvasGroup ratingTitleGroup = GetOrAdd<CanvasGroup>(ratingTitle.gameObject);
            SetInactiveIfNew(ratingTitle.transform, isNewRatingTitle);

            TMP_Text ratingText = FindOrCreateText(panel, panel, "Rating Text", BottomCenter, new Vector2(0f, 410f), new Vector2(700f, 120f), "PERFECT!", 80f, FontStyles.Bold, RatingStylePath, out _);

            RectTransform starRow = BuildStarRow(panel);

            TMP_Text turnsText = FindOrCreateText(panel, panel, "Turns Text", BottomCenter, new Vector2(0f, 200f), new Vector2(500f, 60f), "3 turns taken", 24f, FontStyles.Normal, RatingStylePath, out bool isNewTurnsText);
            SetInactiveIfNew(turnsText.transform, isNewTurnsText);

            Button collectButton = FindOrCreateButton(panel, panel, "Collect Rewards Button", new Vector2(0f, 120f), new Vector2(340f, 90f), "Collect Rewards", 28f);

            bool isNewView = panel.GetComponent<VictoryPanelView>() == null;
            VictoryPanelView view = GetOrAdd<VictoryPanelView>(panel.gameObject);

            var serialized = new SerializedObject(view);

            if (isNewView)
            {
                serialized.FindProperty("_starSize").floatValue = 120f;
                serialized.FindProperty("_starSpacing").floatValue = 0f;
                serialized.FindProperty("_glowSizeMultiplier").floatValue = 1.7f;
                serialized.FindProperty("_glowAlpha").floatValue = 0.6f;
            }

            AssignIfEmpty(serialized, "_ribbon", ribbon);
            AssignIfEmpty(serialized, "_emblem", emblem);
            AssignIfEmpty(serialized, "_laurelLeft", laurelLeft);
            AssignIfEmpty(serialized, "_laurelRight", laurelRight);
            AssignIfEmpty(serialized, "_ratingTitle", ratingTitleGroup);
            AssignIfEmpty(serialized, "_ratingText", ratingText);
            AssignIfEmpty(serialized, "_turnsText", turnsText);
            AssignIfEmpty(serialized, "_collectRewardsButton", collectButton);
            AssignStarSlots(serialized, starRow);

            serialized.ApplyModifiedProperties();
            return view;
        }

        /// <summary>
        /// Creates the star row: a centered horizontal layout (inactive slots are
        /// skipped, so 1 or 2 earned stars stay centered) with three slots, each a
        /// pulsing star glow (UIGlowPulse) behind the star image.
        /// </summary>
        private static RectTransform BuildStarRow(RectTransform panel)
        {
            RectTransform row = FindOrCreate(panel, panel, "Stars", BottomCenter, BottomCenter, Center, new Vector2(0f, 300f), new Vector2(560f, 170f), out _);

            if (row.GetComponent<HorizontalLayoutGroup>() == null)
            {
                HorizontalLayoutGroup layout = Undo.AddComponent<HorizontalLayoutGroup>(row.gameObject);
                layout.spacing = 0f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }

            for (int i = 0; i < StarCount; i++)
            {
                RectTransform slot = FindOrCreate(row, row, $"Star Slot {i + 1}", Vector2.zero, Vector2.zero, Center, Vector2.zero, new Vector2(120f, 120f), out _);

                RectTransform glow = FindOrCreateImage(slot, slot, "Glow", Center, Center, Vector2.zero, new Vector2(204f, 204f), Load<Sprite>(StarGlowSpritePath), StarGlowColor, Image.Type.Simple, out bool isNewGlow);

                if (isNewGlow)
                {
                    glow.SetAsFirstSibling();
                    AddGlowPulse(glow.gameObject, 0.9f, 1.05f, 3f, 0.45f, 0.9f);
                }

                RectTransform star = FindOrCreateImage(slot, slot, "Star", Center, Center, Vector2.zero, Vector2.zero, Load<Sprite>(StarSpritePath), Color.white, Image.Type.Simple, out bool isNewStar);

                if (isNewStar)
                {
                    // The star fills its slot, so Star Size on VictoryPanelView sizes it.
                    star.anchorMin = Vector2.zero;
                    star.anchorMax = Vector2.one;
                    star.sizeDelta = Vector2.zero;
                }
            }

            return row;
        }

        /// <summary>Fills the star slot array when it is empty or incomplete.</summary>
        private static void AssignStarSlots(SerializedObject serialized, RectTransform row)
        {
            SerializedProperty slots = serialized.FindProperty("_starSlots");

            if (slots.arraySize < StarCount)
                slots.arraySize = StarCount;

            for (int i = 0; i < StarCount; i++)
            {
                SerializedProperty element = slots.GetArrayElementAtIndex(i);
                Transform slot = row.Find($"Star Slot {i + 1}");

                if (slot == null)
                    continue;

                AssignIfEmpty(element.FindPropertyRelative("Root"), slot);
                AssignIfEmpty(element.FindPropertyRelative("Icon"), slot.Find("Star")?.GetComponent<Image>());
                AssignIfEmpty(element.FindPropertyRelative("Glow"), slot.Find("Glow")?.GetComponent<Image>());
            }
        }

        // PRIVATE METHODS — Reward panel

        /// <summary>Creates the missing reward popup elements and wires RewardPanelView. A new popup starts inactive.</summary>
        private static RewardPanelView BuildRewardPanel(Transform canvas)
        {
            RectTransform panel = FindOrCreate(canvas, canvas, "Reward Panel", Vector2.zero, Vector2.one, Center, Vector2.zero, Vector2.zero, out bool isNewPanel);

            if (isNewPanel)
            {
                Image dim = panel.gameObject.AddComponent<Image>();
                dim.color = new Color(0f, 0f, 0f, 0.75f);
                panel.gameObject.AddComponent<CanvasGroup>();
            }

            GetOrAdd<CanvasGroup>(panel.gameObject);

            RectTransform frame = FindOrCreateImage(panel, panel, "Frame", Center, Center, Vector2.zero, new Vector2(600f, 440f), null, new Color(0.16f, 0.16f, 0.24f, 1f), Image.Type.Simple, out _);
            FindOrCreateText(panel, frame, "Title", TopCenter, new Vector2(0f, -60f), new Vector2(500f, 70f), "REWARDS", 52f, FontStyles.Normal, null, out _);
            TMP_Text goldText = FindOrCreateText(panel, frame, "Gold Text", Center, new Vector2(0f, 40f), new Vector2(500f, 60f), "+0 Gold", 44f, FontStyles.Normal, null, out _);
            TMP_Text xpText = FindOrCreateText(panel, frame, "XP Text", Center, new Vector2(0f, -30f), new Vector2(500f, 60f), "+0 XP", 44f, FontStyles.Normal, null, out _);
            Button okButton = FindOrCreateSimpleButton(panel, frame, "OK Button", new Vector2(0f, 70f), new Vector2(240f, 90f), "OK");

            RewardPanelView view = GetOrAdd<RewardPanelView>(panel.gameObject);

            var serialized = new SerializedObject(view);
            AssignIfEmpty(serialized, "_frame", frame);
            AssignIfEmpty(serialized, "_goldText", goldText);
            AssignIfEmpty(serialized, "_xpText", xpText);
            AssignIfEmpty(serialized, "_okButton", okButton);
            serialized.ApplyModifiedProperties();

            if (isNewPanel)
            {
                panel.SetAsLastSibling();
                panel.gameObject.SetActive(false);
            }

            return view;
        }

        /// <summary>Assigns both panels to the scene's BattleResultUIController when its fields are empty.</summary>
        private static void WireResultController(VictoryPanelView victoryPanel, RewardPanelView rewardPanel)
        {
            BattleResultUIController controller = Object.FindFirstObjectByType<BattleResultUIController>(FindObjectsInactive.Include);

            if (controller == null)
            {
                Debug.LogWarning("Mighty Heroes: no BattleResultUIController in the scene; assign Victory Panel and Reward Panel by hand.");
                return;
            }

            var serialized = new SerializedObject(controller);
            AssignIfEmpty(serialized, "_victoryPanel", victoryPanel);
            AssignIfEmpty(serialized, "_rewardPanel", rewardPanel);
            serialized.ApplyModifiedProperties();
        }

        // PRIVATE METHODS — Elements

        /// <summary>Uses the selection when it is the canvas, otherwise finds it by name in the open scenes.</summary>
        private static Transform FindCanvas()
        {
            GameObject selected = Selection.activeGameObject;

            if (selected != null && selected.name == CanvasName)
                return selected.transform;

            Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(c => c.name == CanvasName);
            return canvas != null ? canvas.transform : null;
        }

        /// <summary>Finds a descendant of root by name, ignoring case.</summary>
        private static Transform FindByName(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t != root && string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Returns the element found under searchRoot, or creates it under parent with the given layout.</summary>
        private static RectTransform FindOrCreate(Transform searchRoot, Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, out bool isNew)
        {
            Transform existing = FindByName(searchRoot, name);

            if (existing != null)
            {
                isNew = false;
                return GetOrAdd<RectTransform>(existing.gameObject);
            }

            var gameObject = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);
            gameObject.layer = parent.gameObject.layer;

            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            isNew = true;
            return rect;
        }

        /// <summary>Finds or creates an Image element. Sprite, color and type are only set on new elements.</summary>
        private static RectTransform FindOrCreateImage(Transform searchRoot, Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Sprite sprite, Color color, Image.Type type, out bool isNew)
        {
            RectTransform rect = FindOrCreate(searchRoot, parent, name, anchor, anchor, pivot, position, size, out isNew);

            if (!isNew)
                return rect;

            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return rect;
        }

        /// <summary>Finds or creates a centered Grenze TMP text, with an optional TextStyleView. Settings apply to new elements only.</summary>
        private static TMP_Text FindOrCreateText(Transform searchRoot, Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, string text, float fontSize, FontStyles fontStyle, string stylePath, out bool isNew)
        {
            RectTransform rect = FindOrCreate(searchRoot, parent, name, anchor, anchor, Center, position, size, out isNew);
            TMP_Text label = rect.GetComponent<TMP_Text>();

            if (label == null)
                label = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);

            if (!isNew)
                return label;

            TMP_FontAsset font = Load<TMP_FontAsset>(FontPath);

            if (font != null)
                label.font = font;

            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            if (!string.IsNullOrEmpty(stylePath))
            {
                TextStyleView styleView = label.gameObject.AddComponent<TextStyleView>();
                var serialized = new SerializedObject(styleView);
                serialized.FindProperty("_style").objectReferenceValue = Load<TextStyleConfig>(stylePath);
                serialized.ApplyModifiedProperties();
            }

            return label;
        }

        /// <summary>
        /// Finds or creates a call-to-action button in the main menu Battle button
        /// style: pulsing glow, Synty gradient background and frame, Grenze label
        /// and press bounce (MenuButtonAnimator).
        /// </summary>
        private static Button FindOrCreateButton(Transform searchRoot, Transform parent, string name, Vector2 position, Vector2 size, string label, float fontSize)
        {
            RectTransform rect = FindOrCreate(searchRoot, parent, name, BottomCenter, BottomCenter, Center, position, size, out bool isNew);

            if (!isNew)
                return GetOrAdd<Button>(rect.gameObject);

            Image hitArea = rect.gameObject.AddComponent<Image>();
            hitArea.color = new Color(1f, 1f, 1f, 0f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = hitArea;
            rect.gameObject.AddComponent<MenuButtonAnimator>();

            RectTransform glow = CreateStretchedImage(rect, "Glow", Load<Sprite>(ButtonGlowSpritePath), ButtonGlowColor, Image.Type.Sliced, new Vector2(80f, 80f));
            AddGlowPulse(glow.gameObject, 0.95f, 1.05f, 3f, 0.6f, 1f);

            CreateStretchedImage(rect, "SPR_Background", Load<Sprite>(ButtonBackgroundSpritePath), ButtonBackgroundColor, Image.Type.Simple, Vector2.zero);
            CreateStretchedImage(rect, "SPR_Frame", Load<Sprite>(ButtonFrameSpritePath), ButtonFrameColor, Image.Type.Sliced, new Vector2(14f, 14f));

            var labelRect = new GameObject("Text (TMP)", typeof(RectTransform));
            labelRect.layer = rect.gameObject.layer;
            ((RectTransform)labelRect.transform).SetParent(rect, false);
            StretchToParent((RectTransform)labelRect.transform, Vector2.zero);

            TextMeshProUGUI text = labelRect.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = Load<TMP_FontAsset>(FontPath);

            if (font != null)
                text.font = font;

            text.text = label;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            return button;
        }

        /// <summary>Finds or creates a plain button with an Image background and a TMP label child.</summary>
        private static Button FindOrCreateSimpleButton(Transform searchRoot, Transform parent, string name, Vector2 position, Vector2 size, string label)
        {
            RectTransform rect = FindOrCreate(searchRoot, parent, name, BottomCenter, BottomCenter, Center, position, size, out bool isNew);

            if (!isNew)
                return GetOrAdd<Button>(rect.gameObject);

            Image background = rect.gameObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.55f, 0.95f, 1f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            TMP_Text text = FindOrCreateText(rect, rect, name + " Label", Center, Vector2.zero, size, label, 40f, FontStyles.Normal, null, out _);
            StretchToParent(text.rectTransform, Vector2.zero);

            return button;
        }

        /// <summary>Creates an image that fills its parent, grown by the given size delta (e.g. a glow larger than the button).</summary>
        private static RectTransform CreateStretchedImage(RectTransform parent, string name, Sprite sprite, Color color, Image.Type type, Vector2 sizeDelta)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = parent.gameObject.layer;

            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            StretchToParent(rect, sizeDelta);

            Image image = gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.raycastTarget = false;
            return rect;
        }

        private static void StretchToParent(RectTransform rect, Vector2 sizeDelta)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = sizeDelta;
        }

        /// <summary>Adds a UIGlowPulse with the given ranges.</summary>
        private static void AddGlowPulse(GameObject target, float minScale, float maxScale, float speed, float minAlpha, float maxAlpha)
        {
            UIGlowPulse pulse = target.AddComponent<UIGlowPulse>();

            var serialized = new SerializedObject(pulse);
            serialized.FindProperty("_minScale").floatValue = minScale;
            serialized.FindProperty("_maxScale").floatValue = maxScale;
            serialized.FindProperty("_pulseSpeed").floatValue = speed;
            serialized.FindProperty("_minAlpha").floatValue = minAlpha;
            serialized.FindProperty("_maxAlpha").floatValue = maxAlpha;
            serialized.ApplyModifiedProperties();
        }

        private static void SetInactiveIfNew(Transform target, bool isNew)
        {
            if (isNew)
                target.gameObject.SetActive(false);
        }

        private static void AssignIfEmpty(SerializedObject serialized, string propertyName, Object value)
        {
            AssignIfEmpty(serialized.FindProperty(propertyName), value);
        }

        private static void AssignIfEmpty(SerializedProperty property, Object value)
        {
            if (property != null && property.objectReferenceValue == null && value != null)
                property.objectReferenceValue = value;
        }

        private static T Load<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>Returns the component, adding it with Undo when missing.</summary>
        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(gameObject);
        }
    }
}
