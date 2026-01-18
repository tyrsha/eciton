using UnityEditor;

namespace Tyrsha.Eciton
{
    public class ExampleEditorScript : Editor
    {
        [MenuItem("Eciton/Example")]
        public static void ShowExample()
        {
            EditorUtility.DisplayDialog("Example", "This is an example editor script.", "OK");
        }
    }
}