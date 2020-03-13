using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;

[CustomEditor(typeof(AnimatorDB), true)]
public class AnimatorDBEditor : Editor
{
	public override void OnInspectorGUI()
	{
        AnimatorDB db = target as AnimatorDB;
        animatorPicker(db);
        buildStateMapFromButton(db);
    }

    public void Save(AnimatorDB db)
    {
        AnimatorController animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(db.AnimatorAssetPath);
        if (animator != null)
        {
            serializedObject.Update();

            Dictionary<int, AnimatorDB.AnimatorStateInfo> map = new Dictionary<int, AnimatorDB.AnimatorStateInfo>();
            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < animator.layers.Length; i++)
            {
                for (int j = 0; j < animator.layers[i].stateMachine.states.Length; j++)
                {
                    int fullPathHash = Animator.StringToHash($"{animator.layers[i].name}." +
                        $"{animator.layers[i].stateMachine.states[j].state.name}");

                    bool isCombat = animator.layers[i].stateMachine.states[j].state.behaviours
                                .Where(behaviour => behaviour is CombatAnimation)
                                .Select(behaviour => ((CombatAnimation)behaviour).IsFCombat)
                                .FirstOrDefault();

                    map.Add(fullPathHash,
                        new AnimatorDB.AnimatorStateInfo()
                        {
                            Name = animator.layers[i].stateMachine.states[j].state.name,
                            HasTransitions = animator.layers[i].stateMachine.states[j].state.transitions.Length > 0,
                            IsCombat = isCombat,
                        });
                }
            }
            watch.Stop();
            Debug.Log($"Built underlying dictionary in {watch.ElapsedMilliseconds} ms");

            watch = System.Diagnostics.Stopwatch.StartNew();
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] arrayRepresentation;
            try
            {
                formatter.Serialize(stream, map);
            }
            catch (SerializationException e)
            {
                Debug.Log("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                arrayRepresentation = stream.ToArray();
                stream.Close();
            }
            watch.Stop();
            Debug.Log($"Transferred dictionary to byte array in {watch.ElapsedMilliseconds} ms");

            watch = System.Diagnostics.Stopwatch.StartNew();
            string base64String = System.Convert.ToBase64String(arrayRepresentation);
            SerializedProperty serializedProperty = serializedObject.FindProperty("SerializedStateMap");
            serializedProperty.stringValue = base64String;
            serializedObject.ApplyModifiedProperties();
            watch.Stop();
            Debug.Log($"Wrote byte array to serialized asset in {watch.ElapsedMilliseconds} ms");
        }
    }

    private void buildStateMapFromButton(AnimatorDB db)
    {
        if (GUILayout.Button("Build State Map"))
        {
            Save(db);
        }
    }

    private void animatorPicker(AnimatorDB db)
    {
        AnimatorController oldAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(db.AnimatorAssetPath);
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        AnimatorController newAnimator = EditorGUILayout.ObjectField("Animator", oldAnimator, typeof(AnimatorController), false) as AnimatorController;

        if (EditorGUI.EndChangeCheck())
        {
            string newPath = AssetDatabase.GetAssetPath(newAnimator);
            SerializedProperty animatorPathProperty = serializedObject.FindProperty("AnimatorAssetPath");
            animatorPathProperty.stringValue = newPath;
        }
        serializedObject.ApplyModifiedProperties();
    }
}