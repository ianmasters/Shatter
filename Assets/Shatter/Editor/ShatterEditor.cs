using System;
using EzySlice;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shatter.Editor
{
    /**
 * This is a simple Editor helper script for rapid testing/prototyping! 
 */
    [CustomEditor(typeof(Shatter))]
    public class ShatterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // serializedObject.Update(); // TODO: is this required - surely not?

            var script = (Shatter)target;

            script.objectToShatter = (GameObject)EditorGUILayout.ObjectField("Object to Shatter", script.objectToShatter, typeof(GameObject), true);

            if (!script.objectToShatter)
            {
                EditorGUILayout.LabelField("Add a GameObject to Shatter.");
                return;
            }

            if (!script.objectToShatter.activeInHierarchy)
            {
                EditorGUILayout.LabelField("Object to slice is Hidden. Cannot Slice.");
                return;
            }

            script.crossSectionMaterial = (Material)EditorGUILayout.ObjectField("Cross Section Material", script.crossSectionMaterial, typeof(Material), false);

            var destroyOnComplete = (Shatter.DestroyOnCompleteType)EditorGUILayout.EnumPopup("Destroy on Complete", script.destroyOnComplete);
            if (destroyOnComplete != script.destroyOnComplete)
            {
                Undo.RegisterFullObjectHierarchyUndo(script, "Destroy on Complete");
                script.destroyOnComplete = destroyOnComplete;
            }

            var en = EditorGUILayout.Toggle("Enable Test Plane", script.enableTestPlane);
            if (en != script.enableTestPlane)
            {
                Undo.RegisterFullObjectHierarchyUndo(script, "Enable Test Plane");
                script.enableTestPlane = en;
            }

            if (script.enableTestPlane)
                script.testPlane = (GameObject)EditorGUILayout.ObjectField("Test Plane", script.testPlane, typeof(GameObject), true);
            else
                script.shatterCount = EditorGUILayout.IntSlider("Shatter Count", script.shatterCount, 1, 20);

            var colorSave = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button($"\n{(script.enableTestPlane ? "Slice" : "Shatter")} '{script.objectToShatter.name}'\n"))
            {
                const string undoName = "Shatter";

                // Don't think this is required as the mouse down or some other event should have incremented it.
                // This stuff is extremely unclear from any of the documentation as to when you should need to create an undo group.
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName(undoName);

                Undo.RegisterFullObjectHierarchyUndo(script.objectToShatter, undoName);
                // Undo.RegisterCompleteObjectUndo(script.shards, undoName);
                
#if UNITY_EDITOR
                SlicedHull.ResetDebug();
#endif

                // Perform the action
                if (script.enableTestPlane)
                    script.SlicePlane(script.testPlane);
                else
                    script.RandomShatter();

                var objects = Array.ConvertAll(script.shards.ToArray(), o => (Object)o);

                // if(script.enableTestPlane)
                Selection.objects = objects;

                foreach (var o in script.shards)
                {
                    Undo.RegisterCreatedObjectUndo(o, undoName);
                }

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            GUI.backgroundColor = colorSave;

            // once you have an editor script apparently you are responsible for doing this
            if (GUI.changed)
                script.OnValidate();

            // if(script.objectToShatter.gameObject != script.gameObject)
            if(script)
                serializedObject.ApplyModifiedProperties();
        }
    }
}