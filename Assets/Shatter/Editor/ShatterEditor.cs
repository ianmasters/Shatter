using System;
using EzySlice;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shatter.Editor
{
    [CustomEditor(typeof(Shatter))]
    public class ShatterEditor : UnityEditor.Editor
    {
        private Shatter script;
        
        private void OnEnable()
        {
            // Debug.Log("ShatterEditor.OnEnable");
            script = (Shatter)target;
            SetupStates();
        }

        private void SetupStates()
        {
            var r = script.testPlane.GetComponent<Renderer>();
            if (r) r.enabled = script.enableTestPlane;
        }

        public override void OnInspectorGUI()
        {
            // serializedObject.Update(); // TODO: is this required - surely not?

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
                // Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName(undoName);

                // Undo.RegisterFullObjectHierarchyUndo(script.gameObject, undoName);
                Undo.RegisterFullObjectHierarchyUndo(script.objectToShatter, undoName);
                
#if UNITY_EDITOR
                SlicedHull.ResetDebug();
#endif

                // Perform the action
                if (script.enableTestPlane)
                    script.SlicePlane(script.testPlane);
                else
                    script.RandomShatter();

                var objects = Array.ConvertAll(script.shards.ToArray(), shard => (Object)shard.gameObject);

                Selection.objects = objects;

                foreach (var shard in script.shards)
                {
                    Undo.RegisterCreatedObjectUndo(shard.gameObject, undoName);
                }

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            GUI.backgroundColor = colorSave;

            // if(script.objectToShatter.gameObject != script.gameObject)
            if(script)
                serializedObject.ApplyModifiedProperties();
            
            // once you have an editor script apparently you are responsible for doing this
            if (GUI.changed)
            {
                // script.OnValidate();
                SetupStates();
            }
        }
    }
}