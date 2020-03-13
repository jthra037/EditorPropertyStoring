using UnityEditor;

public class AnimatorDatabaseWindow : EditorWindow
{
    public static bool Autosave { get; private set; } = true;

	// Add menu named "My Window" to the Window menu
	[MenuItem("Window/Animator Database Settings")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimatorDatabaseWindow window = (AnimatorDatabaseWindow)GetWindow(typeof(AnimatorDatabaseWindow));
        window.Show();
    }

    private void OnGUI()
    {
        Autosave = EditorGUILayout.Toggle("Auto Save AnimatorDB Assets", Autosave);
    }
}
