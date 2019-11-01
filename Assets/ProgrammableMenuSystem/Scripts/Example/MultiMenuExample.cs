using UnityEngine;
namespace GameHero.PMS.Example {
    public class MultiMenuExample : MonoBehaviour {

        public Sprite TestIcon;

        public ProgrammableMenu MenuOne;
        public ProgrammableMenu MenuTwo;

        void Start() {
            BuildMenu(MenuOne);
            BuildMenu(MenuTwo);
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ToggleMenu(MenuOne);
            }
            if (Input.GetKeyDown(KeyCode.F1)) {
                ToggleMenu(MenuTwo);
            }
        }

        void BuildMenu(ProgrammableMenu menu) {
            var menuStart = menu
                .Create("menu_start")
                .WithText("Dummy Menu Item")
                .WithIcon(TestIcon)
                .Localize("lithuanian", "Nieko nedarau");

            var inMenuItemClose = menu
                .Create("menu_close")
                .WithText("Close menu")
                .Localize("lithuanian", "uzdaryti")
                .OnClick(() => menu.Close());

            var inMenuItemUp = menu
                .Create("menu_up")
                .WithText("Go up")
                .Localize("lithuanian", "atgal")
                .OnClick(() => menu.Back());

            var menuItemChooseLang = menu
                .Create("menu_a")
                .WithText("Choose Language")
                .Localize("lithuanian", "pasirinkti kalba");

            var menuItemDummy = menu
                .Create("menu_b")
                .WithText("Other Dummy")
                .Localize("lithuanian", "nieko nedarysiu");

            var menuItemEnglish = menu
                .Create("menu_in_a")
                .WithText("English")
                .OnClick(() => ProgrammableMenu.SetLocale("english"));

            var menuItemLithuanian = menu
                .Create("menu_in_b")
                .WithText("Lithuanian")
                .OnClick(() => ProgrammableMenu.SetLocale("lithuanian"));

            menuStart
                .OnDown(menuItemChooseLang);
            menuItemChooseLang
                .OnDown(menuItemDummy)
                .OnIn(menuItemEnglish);
            menuItemEnglish
                .OnDown(menuItemLithuanian);
            menuItemLithuanian
                .OnDown(inMenuItemUp);
            inMenuItemUp
                .OnDown(inMenuItemClose);

            menu.FirstNode = menuStart;
        }

        void ToggleMenu(ProgrammableMenu menu) {
            if (menu.IsShowing) {
                menu.Back();
            } else {
                menu.Open();
            }
        }
    }
}