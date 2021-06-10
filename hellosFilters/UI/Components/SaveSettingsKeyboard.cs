using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using HMUI;
using HUI.UI.Components;
using HUI.Utilities;

namespace HUIFilters.UI.Components
{
    public class SaveSettingsKeyboard : MonoBehaviour
    {
        public event Action<char> KeyPressed;
        public event Action DeleteButtonPressed;
        public event Action ClearButtonPressed;

        private GameObject _alphaKeysContainer;
        private GameObject _symbolKeysContainer;

        private CustomButtonAnimations _shiftButtonAnimations;

        private bool _isShiftMode = false;

        private static readonly Color DeleteButtonSelectedColour = new Color(1f, 0.216f, 0.067f);
        private static readonly Color NonKeyButtonSelectedColour = new Color(0.4f, 1f, 0.133f);

        private const float KeySpacing = 0.3f;

        private void Awake()
        {
            var le = this.GetComponent<LayoutElement>();
            if (le != null)
            {
                le.preferredWidth = 60f;
                le.preferredHeight = 20f;
            }
        }

        [Inject]
        private void Initialize(UIKeyboardManager uiKeyboardManager)
        {
            // prepare prefab
            GameObject keyPrefab = Instantiate(uiKeyboardManager.keyboard.GetComponentsInChildren<UIKeyboardKey>().First().gameObject);
            DestroyImmediate(keyPrefab.GetComponent<UIKeyboardKey>());
            DestroyImmediate(keyPrefab.GetComponent<ButtonStaticAnimations>());

            keyPrefab.AddComponent<CustomButtonAnimations>();
            keyPrefab.AddComponent<CustomKeyboardKeyButton>();

            var le = keyPrefab.GetComponent<LayoutElement>();
            le.minWidth = 0f;
            le.minHeight = 0f;
            le.preferredWidth = 0f;
            le.preferredHeight = 0f;

            // create key layouts
            // assume the keyboard is going to be 60u wide by 20u tall
            _alphaKeysContainer = new GameObject("AlphaKeysContainer", typeof(VerticalLayoutGroup));

            var vlg = _alphaKeysContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = KeySpacing;
            vlg.padding = new RectOffset();
            vlg.childAlignment = TextAnchor.UpperLeft;

            var rt = vlg.transform as RectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.SetParent(this.transform, false);

            _symbolKeysContainer = Instantiate(_alphaKeysContainer);
            _symbolKeysContainer.name = "SymbolKeysContainer";
            _symbolKeysContainer.transform.SetParent(this.transform, false);
            _symbolKeysContainer.SetActive(false);

            (char character, float size)[][] AlphaKeyLayout = new (char, float)[][]
            {
                new (char, float)[] { ('q', 6f), ('w', 6f), ('e', 6f), ('r', 6f), ('t', 6f), ('y', 6f), ('u', 6f), ('i', 6f), ('o', 6f), ('p', 6f) },
                new (char, float)[] { ('\0', 3f), ('a', 6f), ('s', 6f), ('d', 6f), ('f', 6f), ('g', 6f), ('h', 6f), ('j', 6f), ('k', 6f), ('l', 6f), ('\0', 3f) },
                new (char, float)[] { ('\0', 6f), ('z', 6f), ('x', 6f), ('c', 6f), ('v', 6f), ('b', 6f), ('n', 6f), ('m', 6f) }
            };
            (char character, float size)[][] SymbolKeyLayout = new (char, float)[][]
            {
                new (char, float)[] { ('+', 6f), ('-', 6f), ('*', 6f), ('/', 6f), ('(', 6f), (')', 6f), ('\0', 6f), ('7', 6f), ('8', 6f), ('9', 6f) },
                new (char, float)[] { ('=', 6f), ('_', 6f), ('\'', 6f), ('"', 6f), ('<', 6f), ('>', 6f), ('\0', 6f), ('4', 6f), ('5', 6f), ('6', 6f) },
                new (char, float)[] { ('\0', 24f), ('!', 6f), ('?', 6f), ('\0', 6f), ('1', 6f), ('2', 6f), ('3', 6f) }
            };

            CreateRows(keyPrefab, AlphaKeyLayout, _alphaKeysContainer.transform);
            CreateRows(keyPrefab, SymbolKeyLayout, _symbolKeysContainer.transform);

            // create keys for the last rows, but set it later
            var spaceButton = Instantiate(keyPrefab).GetComponent<CustomKeyboardKeyButton>();
            spaceButton.name = "SpaceKeyboardButton";
            spaceButton.Key = ' ';
            spaceButton.KeyPressed += OnKeyPressed;

            le = spaceButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 30f;

            var commaButton = Instantiate(keyPrefab).GetComponent<CustomKeyboardKeyButton>();
            commaButton.name = "CommaKeyboardButton";
            commaButton.Key = ',';
            commaButton.KeyPressed += OnKeyPressed;

            le = commaButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 6f;

            var periodButton = Instantiate(keyPrefab).GetComponent<CustomKeyboardKeyButton>();
            periodButton.name = "PeriodKeyboardButton";
            periodButton.Key = '.';
            periodButton.KeyPressed += OnKeyPressed;

            le = periodButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 6f;

            var zeroButton = Instantiate(keyPrefab).GetComponent<CustomKeyboardKeyButton>();
            zeroButton.name = "0KeyboardButton";
            zeroButton.Key = '0';
            zeroButton.KeyPressed += OnKeyPressed;

            le = zeroButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 6f;

            // prepare prefab for action keys
            DestroyImmediate(keyPrefab.GetComponent<CustomKeyboardKeyButton>());
            keyPrefab.AddComponent<CustomKeyboardActionButton>();

            var lastRowTransform = _alphaKeysContainer.transform.GetChild(_alphaKeysContainer.transform.childCount - 1);
            var deleteButton = InstantiateKey(keyPrefab, lastRowTransform).GetComponent<CustomKeyboardActionButton>();
            deleteButton.name = "DeleteKeyboardButton";
            deleteButton.Text = "Del";
            deleteButton.ButtonPressed += () => DeleteButtonPressed?.Invoke();
            deleteButton.SelectedColour = DeleteButtonSelectedColour;

            le = deleteButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 12f;

            // alpha key bottom row
            var rowContainer = CreateRow();
            rowContainer.transform.SetParent(_alphaKeysContainer.transform, false);

            var symbolButton = InstantiateKey(keyPrefab, rowContainer.transform).GetComponent<CustomKeyboardActionButton>();
            symbolButton.name = "SymbolKeyboardButton";
            symbolButton.Text = "(!?)";
            symbolButton.ButtonPressed += () => SetSymbolMode(true);
            symbolButton.SelectedColour = NonKeyButtonSelectedColour;

            le = symbolButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 9f;

            var shiftButton = InstantiateKey(keyPrefab, rowContainer.transform).GetComponent<CustomKeyboardActionButton>();
            shiftButton.name = "ShiftKeyboardButton";
            shiftButton.Text = "Shift";
            shiftButton.ButtonPressed += () => SetShiftMode(!_isShiftMode);
            shiftButton.SelectedColour = NonKeyButtonSelectedColour;

            le = shiftButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 12f;

            _shiftButtonAnimations = shiftButton.GetComponent<CustomButtonAnimations>();

            spaceButton.transform.SetParent(rowContainer.transform, false);

            var clearButton = InstantiateKey(keyPrefab, rowContainer.transform).GetComponent<CustomKeyboardActionButton>();
            clearButton.name = "ClearKeyboardButton";
            clearButton.Text = "Clear";
            clearButton.ButtonPressed += () => ClearButtonPressed?.Invoke();
            clearButton.SelectedColour = DeleteButtonSelectedColour;

            le = clearButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 9f;

            // symbol key bottom row
            rowContainer = CreateRow();
            rowContainer.transform.SetParent(_symbolKeysContainer.transform, false);

            var alphaButton = InstantiateKey(keyPrefab, rowContainer.transform).GetComponent<CustomKeyboardActionButton>();
            alphaButton.name = "AlphaKeyboardButton";
            alphaButton.Text = "ABC";
            alphaButton.ButtonPressed += () => SetSymbolMode(false);
            alphaButton.SelectedColour = NonKeyButtonSelectedColour;

            le = alphaButton.GetComponent<LayoutElement>();
            le.flexibleWidth = 12f;

            CreateSpacer(12f, rowContainer.transform);

            commaButton.transform.SetParent(rowContainer.transform, false);
            periodButton.transform.SetParent(rowContainer.transform, false);

            CreateSpacer(12f, rowContainer.transform);

            zeroButton.transform.SetParent(rowContainer.transform, false);

            CreateSpacer(6f, rowContainer.transform);

            Destroy(keyPrefab);
        }

        private void CreateRows(GameObject keyPrefab, (char character, float size)[][] keyLayout, Transform parent)
        {
            foreach (var row in keyLayout)
            {
                var rowContainer = CreateRow();
                rowContainer.transform.SetParent(parent, false);

                foreach (var keyInfo in row)
                {
                    char character = keyInfo.character;
                    float size = keyInfo.size;

                    if (character == '\0')
                    {
                        CreateSpacer(size, rowContainer.transform);
                    }
                    else
                    {
                        var newKey = InstantiateKey(keyPrefab, rowContainer.transform).GetComponent<CustomKeyboardKeyButton>();

                        string name;
                        if (character == '/')
                            name = "Slash";
                        else
                            name = character.ToString().ToUpper();

                        newKey.name = $"{name}KeyboardButton";
                        newKey.Key = character;
                        newKey.KeyPressed += OnKeyPressed;

                        var le = newKey.GetComponent<LayoutElement>();
                        le.flexibleWidth = size;
                    }
                }
            }
        }

        private GameObject InstantiateKey(GameObject keyPrefab, Transform parent)
        {
            // instantiate without parent
            // this ensures Awake is called
            var go = Instantiate(keyPrefab);
            go.transform.SetParent(parent, false);
            return go;
        }

        private GameObject CreateRow()
        {
            var hlg = new GameObject("KeyRow").AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = KeySpacing;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            return hlg.gameObject;
        }

        private GameObject CreateSpacer(float size, Transform parent)
        {
            var spacer = new GameObject("Spacer", typeof(LayoutElement));
            spacer.transform.SetParent(parent, false);

            var le = spacer.GetComponent<LayoutElement>();
            le.flexibleWidth = size;

            return spacer;
        }

        public void SetShiftMode(bool enabled)
        {
            _isShiftMode = enabled;

            _shiftButtonAnimations.NormalBGColour = enabled ? _shiftButtonAnimations.HighlightedBGColour : Color.clear;
        }

        public void SetSymbolMode(bool enabled)
        {
            _alphaKeysContainer.SetActive(!enabled);
            _symbolKeysContainer.SetActive(enabled);
        }

        private void OnKeyPressed(char character)
        {
            if (_isShiftMode && char.IsLetter(character))
                character = char.ToUpper(character);

            // exception handling done in CustomKeyboardButton
            KeyPressed?.Invoke(character);
        }
    }
}
