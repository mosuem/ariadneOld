using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class SimpleEditorUtils
{
	// click command-0 to go to the prelaunch scene and then play

	[MenuItem ("Edit/Play-Unplay, But From Prelaunch Scene %0")]
	public static void PlayFromPrelaunchScene ()
	{
		if (!EditorApplication.isPlaying) {
			string currentSceneName = EditorSceneManager.GetActiveScene ().name;
			File.WriteAllText (".lastScene", currentSceneName);
			EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
			EditorSceneManager.OpenScene ("Assets/_scenes/mainMenu.unity");
			EditorApplication.isPlaying = true;
		}
		if (EditorApplication.isPlaying) {
			string lastScene = File.ReadAllText (".lastScene");
			EditorApplication.isPlaying = false;
			EditorSceneManager.LoadScene (lastScene);
		}
	}
}