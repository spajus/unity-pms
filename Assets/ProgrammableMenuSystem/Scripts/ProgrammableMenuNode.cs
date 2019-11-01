using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace GameHero.PMS {
    public class ProgrammableMenuNode : MonoBehaviour {
        [SerializeField]
        protected Image Icon;
        protected Button MenuButton;
        protected string CurrentLocale => ProgrammableMenu.CurrentLocale;
        protected string DefaultLocale => ProgrammableMenu.DefaultLocale;

        internal ProgrammableMenu Menu;
        internal RectTransform RectTransform { 
            get {
                if (rectTransform == null) {
                    rectTransform = GetComponent<RectTransform>();
                }
                return rectTransform;
            }
        }

        #region Linked Menu Nodes
        internal ProgrammableMenuNode Left { get; set; }
        internal ProgrammableMenuNode Right { get; set; }
        internal ProgrammableMenuNode Up { get; set; }
        internal ProgrammableMenuNode Down { get; set; }
        internal ProgrammableMenuNode In { get; set; }
        #endregion

        private Action clickAction;
        private Text text;
        private Dictionary<string, string> localizedText = new Dictionary<string, string>();


        private RectTransform rectTransform;

        #region Navigation hooks
        public ProgrammableMenuNode OnDown(ProgrammableMenuNode other) {
            Debug.LogFormat("On down {0}", name);
            Down = other;
            other.Up = this;
            return this;
        }

        public ProgrammableMenuNode OnUp(ProgrammableMenuNode other) {
            Up = other;
            other.Down = this;
            return this;
        }

        public ProgrammableMenuNode OnLeft(ProgrammableMenuNode other) {
            Left = other;
            other.Right = this;
            return this;
        }

        public ProgrammableMenuNode OnRight(ProgrammableMenuNode other) {
            Right = other;
            other.Left = this;
            return this;
        }

        public ProgrammableMenuNode OnIn(ProgrammableMenuNode other) {
            In = other;
            return this;
        }

        #endregion

        public ProgrammableMenuNode OnClick(System.Action action) {
            clickAction = action;
            return this;
        }

        public ProgrammableMenuNode WithIcon(Sprite icon) {
            if (Icon == null) {
                Debug.LogError(
                    @"Programmable Menu System:
                    Cannot set icon sprite, Icon is not set on prefab!");
            } else {
                Icon.sprite = icon;
            }
            return this;
        }

        public ProgrammableMenuNode WithText(string text) {
            SetLocalizedText(text, DefaultLocale);
            return this;
        }

        public ProgrammableMenuNode Localize(string locale, string text) {
            SetLocalizedText(text, locale);
            return this;
        }

        internal virtual void LoadText() {
            text = GetComponentInChildren<Text>();
            text.text = GetLocalizedText();
        }

        protected void MenuButtonClicked() {
            if (clickAction != null) {
                clickAction();
            } else {
                if (In == null) {
                    Debug.LogWarningFormat(
                        @"Programmable Menu System: 
                        No action bound to button: {0}.
                        No deeper menu levels to go in to.", 
                        name);
                }
            }
            if (In != null) {
                Menu.Open(In);
            }
        }

        protected void SetLocalizedText(string text, string locale) {
            Debug.LogFormat("Setting text: {0} with locale {1} on button {2}", text, locale, name);
            if (localizedText.ContainsKey(locale)) {
                localizedText[locale] = text;
            } else {
                localizedText.Add(locale, text);
            }
        }

        protected string GetLocalizedText() {
            var value = "Untitled";
            if (!localizedText.TryGetValue(CurrentLocale, out value)) {
                if (CurrentLocale != DefaultLocale) {
                    if (!localizedText.TryGetValue(DefaultLocale, out value)) {
                        Debug.LogWarningFormat(
                            @"Programmable Menu System: 
                            Could not find text for menu node: {0}. 
                            Current locale: {1}. 
                            Default locale: {2}", 
                            name, 
                            CurrentLocale, 
                            DefaultLocale);
                    }
                } else {
                    Debug.LogWarningFormat(
                        @"Programmable Menu System: 
                        Could not find text for menu node: {0}.",
                        name);
                }
            }
            return value;
        }

        protected void Start() {
            MenuButton = GetComponent<Button>();
            MenuButton.onClick.AddListener(MenuButtonClicked);
        }

        protected void OnEnable() {
            LoadText();
        }
    }
}