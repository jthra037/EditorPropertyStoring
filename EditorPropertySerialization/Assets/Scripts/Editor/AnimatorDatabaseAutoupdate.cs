using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AnimatorDatabaseAutoupdate : UnityEditor.AssetModificationProcessor
{
    private static AnimatorDB[] animatorDBs;
    private static string[] animatorPaths;

    static AnimatorDatabaseAutoupdate()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimatorDB", null);
        animatorDBs = new AnimatorDB[guids.Length];

        for(int i = 0; i < animatorDBs.Length; i++)
        {
            animatorDBs[i] = AssetDatabase.LoadAssetAtPath<AnimatorDB>(AssetDatabase.GUIDToAssetPath(guids[i]));
        }

        // if there is a collision here, there are two animator databases for the same animator
        animatorPaths = animatorDBs.Select(db => db.AnimatorAssetPath).ToArray();
    }
    static string[] OnWillSaveAssets(string[] paths)
    {
        if (AnimatorDatabaseWindow.Autosave)
        {
            HashSet<string> relevantPaths = new HashSet<string>(animatorPaths);
            relevantPaths.IntersectWith(paths);

            foreach(string path in relevantPaths)
            {
                // needs an extra search since AnimatorDB's aren't sorted by associated AnimatorController paths, which is what we actually have
                // could make it faster by caching things differently, but probably less space efficient
                AnimatorDB animatorDB = System.Array.Find(animatorDBs, db => db.AnimatorAssetPath == path);
                AnimatorDBEditor editor = (AnimatorDBEditor)Editor.CreateEditor(animatorDB);
                editor.Save(animatorDB);
            }
        }

        return paths;
    }
}