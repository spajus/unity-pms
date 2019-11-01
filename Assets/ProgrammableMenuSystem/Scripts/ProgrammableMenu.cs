using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace GameHero.PMS {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class ProgrammableMenu : MonoBehaviour {
        #region Static
        public static string DefaultLocale = "english";
        public static string CurrentLocale { get; private set; } = "english";
        #endregion


        #region Configuration
        [Header("Menu Configuration")]

        [Tooltip("Set the prefab that will be used to instantiate all buttons")]
        public ProgrammableMenuNode MenuNodePrefab;
        [Tooltip("Prefix that will be added before every button game object name. Blank will default to this menu game object name plus underscore")]
        public string ButtonNamePrefix = null;
        [Header("Menu Flags")]
        [Tooltip("Will display menu when GameObject Start is called")]
        public bool ShowOnStart = false;
        [Tooltip("Will try to find other ProgrammableMenu instances that are open and select some button. Useful for controller navigation.")]
        public bool SelectAnyAvailableMenuItemOnClose = false;
        [Tooltip("Will automatically position buttons according to Auto Positioning settings")]
        public bool RepositionButtons = true;

        [Header("Auto Positioning")]
        public Vector2 MenuOffset;
        public float MenuItemGapVertical = 10;
        public float MenuItemGapHorizontal = 10;

        #endregion

        public ProgrammableMenuNode FirstNode { get; set; }
        public bool IsShowing => menuStack.Count > 0;


        private CanvasGroup canvasGroup;
        private Stack<ProgrammableMenuNode> menuStack = new Stack<ProgrammableMenuNode>();
        private string actualButtonNamePrefix => 
            string.IsNullOrWhiteSpace(ButtonNamePrefix) 
                ? string.Format("{0}_", name) : ButtonNamePrefix;

        public static void SetLocale(string locale) {
            CurrentLocale = locale; 
            foreach (var menu in FindObjectsOfType<ProgrammableMenu>()) {
                if (menu.menuStack.Count > 0) {
                    menu.ReloadNode(menu.menuStack.Peek(), null);
                }
            }
        }

        #region Public 

        public ProgrammableMenuNode Create(string nodeName) {
            var node = Instantiate(MenuNodePrefab);
            node.Menu = this;
            node.transform.SetParent(transform);
            node.transform.localScale = Vector3.one;
            node.name = string.Format("{0}{1}", actualButtonNamePrefix, nodeName);
            node.gameObject.SetActive(false);
            return node;
        }

        public void Open(ProgrammableMenuNode firstNode = null) {
            if (IsShowing) {
                HideNode(menuStack.Peek(), null);
            } else {
                ShowMenuPanel();
            }
            if (firstNode == null) {
                firstNode = FirstNode;
            }
            if (firstNode == null) {
                Debug.LogErrorFormat(
                    @"Programmable Menu System: 
                    Please set ProgrammableMenuSystem.Instance.FirstNode 
                    or pass it into Show() method!");
            }
            menuStack.Push(firstNode);
            ShowNode(firstNode, null);
            SelectButton(firstNode);
        }

        public void Back() {
            if (menuStack.Count == 0) {
                DeselectButton();
                return;
            }
            HideNode(menuStack.Pop(), null);
            if (menuStack.Count > 0) {
                var node = menuStack.Peek();
                ShowNode(node, null);
                SelectButton(node);
            } else {
                Close();
            }
        }

        public void Close() {
            while (menuStack.Count > 0) {
                var node = menuStack.Pop();
                HideNode(node, null);
            }
            HideMenuPanel();
            if (!SelectAnyAvailableMenuItemOnClose) {
                DeselectButton();
                return;
            }

            var menus = FindObjectsOfType<ProgrammableMenu>();
            var firstAvailable = menus.FirstOrDefault(m => m != this && m.IsShowing);
            if (firstAvailable != null) {
                firstAvailable.SelectButton();
            } else {
                DeselectButton();
            }
        } 
        #endregion

        #region Unity MonoBehaviour
        private void Awake() {
            LoadCanvasGroup();
        }

        private void Start() {
            if (ShowOnStart) {
                Open();
            } else {
                HideMenuPanel();
            }
        }
        #endregion


        #region Private
        private void LoadCanvasGroup() {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }

        private void ShowMenuPanel() {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void HideMenuPanel() {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void DeselectButton() {
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void SelectButton(ProgrammableMenuNode node = null) {
            if (node == null && menuStack.Count > 0) {
                node = menuStack.Peek();
            }
            if (node != null) {
                EventSystem.current.SetSelectedGameObject(node.gameObject);
            }
        }

        private void RepositionNode(ProgrammableMenuNode node, ProgrammableMenuNode parentNode) {
            if (!RepositionButtons) {
                return; 
            }

            if (parentNode == null) {
                node.RectTransform.anchoredPosition = MenuOffset;
                return;
            }
            var parentPosition = parentNode.RectTransform.anchoredPosition;
            Vector2 offset;
            if (parentNode == node.Up) {
                offset = new Vector2(
                    0, 
                    -parentNode.RectTransform.sizeDelta.y 
                    - MenuItemGapVertical);
            } else if (parentNode == node.Down) {
                offset = new Vector2(
                    0, 
                    parentNode.RectTransform.sizeDelta.y 
                    + MenuItemGapVertical);
            } else if (parentNode == node.Left) {
                offset = new Vector2(
                    parentNode.RectTransform.sizeDelta.x 
                    + MenuItemGapHorizontal, 
                    0);
            } else if (parentNode == node.Right) {
                offset = new Vector2(
                    -parentNode.RectTransform.sizeDelta.x
                    - MenuItemGapHorizontal, 
                    0);
            } else {
                offset = new Vector2(0, 0);
            }
            node.RectTransform.anchoredPosition = parentPosition + offset;

        }

        private void ShowNode(ProgrammableMenuNode node, ProgrammableMenuNode parentNode) {
            if (node == null) { return; }
            node.gameObject.SetActive(true);
            RepositionNode(node, parentNode);
            if (node.Down != null && node.Down != parentNode) { 
                ShowNode(node.Down, node); 
            }
            if (node.Left != null && node.Left != parentNode) { 
                ShowNode(node.Left,  node); 
            }
            if (node.Right != null && node.Right != parentNode) { 
                ShowNode(node.Right, node); 
            }
            if (node.Up != null && node.Up != parentNode) { 
                ShowNode(node.Up, node);
            }
        }

        private void HideNode(ProgrammableMenuNode node, ProgrammableMenuNode parentNode) {
            if (node == null) { return; }
            node.gameObject.SetActive(false);
            if (node.Down != null && node.Down != parentNode) { 
                HideNode(node.Down, node); 
            }
            if (node.Left != null && node.Left != parentNode) { 
                HideNode(node.Left,  node); 
            }
            if (node.Right != null && node.Right != parentNode) { 
                HideNode(node.Right, node); 
            }
            if (node.Up != null && node.Up != parentNode) { 
                HideNode(node.Up, node);
            }
        }

        private void ReloadNode(ProgrammableMenuNode node, ProgrammableMenuNode parentNode) {
            if (node == null) { return; }
            node.LoadText();
            if (node.Down != null && node.Down != parentNode) { 
                ReloadNode(node.Down, node); 
            }
            if (node.Left != null && node.Left != parentNode) { 
                ReloadNode(node.Left,  node); 
            }
            if (node.Right != null && node.Right != parentNode) { 
                ReloadNode(node.Right, node); 
            }
            if (node.Up != null && node.Up != parentNode) { 
                ReloadNode(node.Up, node);
            }
        }
        #endregion
    }
}