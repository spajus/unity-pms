using TMPro;
namespace GameHero.PMS {
    public class TMPProgrammableMenuNode : ProgrammableMenuNode {
        private TMP_Text textMesh;

        internal override void LoadText() {
            textMesh = GetComponentInChildren<TMP_Text>();
            textMesh.SetText(GetLocalizedText());
        }
    }
}