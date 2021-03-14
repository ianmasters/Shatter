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
        private Shatter shatter;
        
        private void OnEnable()
        {
            // Debug.Log("ShatterEditor.OnEnable");
            shatter = (Shatter)target;
            SetupStates();
        }

        private void SetupStates()
        {
            var r = shatter.testPlane.GetComponent<Renderer>();
            if (r) r.enabled = shatter.enableTestPlane;
        }

        public override void OnInspectorGUI()
        {
            // serializedObject.Update(); // TODO: is this required - surely not?

            shatter.objectToShatter = (GameObject)EditorGUILayout.ObjectField("Object to Shatter", shatter.objectToShatter, typeof(GameObject), true);

            if (!shatter.objectToShatter)
            {
                EditorGUILayout.LabelField("Add a GameObject to Shatter.");
                return;
            }

            if (!shatter.objectToShatter.activeInHierarchy)
            {
                EditorGUILayout.LabelField("Object to slice is Hidden. Cannot Slice.");
                return;
            }

            shatter.crossSectionMaterial = (Material)EditorGUILayout.ObjectField("Cross Section Material", shatter.crossSectionMaterial, typeof(Material), false);

            var destroyOnComplete = (Shatter.DestroyOnCompleteType)EditorGUILayout.EnumPopup("Destroy on Complete", shatter.destroyOnComplete);
            if (destroyOnComplete != shatter.destroyOnComplete)
            {
                Undo.RegisterFullObjectHierarchyUndo(shatter, "Destroy on Complete");
                shatter.destroyOnComplete = destroyOnComplete;
            }
            
            var en = EditorGUILayout.Toggle("Enable Gravity", shatter.enableGravity);
            if (en != shatter.enableGravity)
            {
                Undo.RegisterFullObjectHierarchyUndo(shatter, "Enable Gravity");
                shatter.enableGravity = en;
            }
            
            en = EditorGUILayout.Toggle("Enable Test Plane", shatter.enableTestPlane);
            if (en != shatter.enableTestPlane)
            {
                Undo.RegisterFullObjectHierarchyUndo(shatter, "Enable Test Plane");
                shatter.enableTestPlane = en;
            }

            if (shatter.enableTestPlane)
                shatter.testPlane = (GameObject)EditorGUILayout.ObjectField("Test Plane", shatter.testPlane, typeof(GameObject), true);
            else
                shatter.shatterCount = EditorGUILayout.IntSlider("Shatter Count", shatter.shatterCount, 1, 20);

            var colorSave = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button($"\n{(shatter.enableTestPlane ? "Slice" : "Shatter")} '{shatter.objectToShatter.name}'\n"))
            {
                const string undoName = "Shatter";

                // Don't think this is required as the mouse down or some other event should have incremented it.
                // This stuff is extremely unclear from any of the documentation as to when you should need to create an undo group.
                // Undo.IncrementCurrentGroup();
                
                Undo.SetCurrentGroupName(undoName);

                Undo.RegisterFullObjectHierarchyUndo(shatter.gameObject, undoName);
                Undo.RegisterFullObjectHierarchyUndo(shatter.objectToShatter, undoName);
                
#if UNITY_EDITOR
                SlicedHull.ResetDebug();
#endif

                // Perform the action
                if (shatter.enableTestPlane)
                    shatter.SlicePlane(shatter.testPlane);
                else
                    shatter.RandomShatter();

                var objects = Array.ConvertAll(shatter.shrapnels.ToArray(), shard => (Object)shard.gameObject);

                Selection.objects = objects;

                foreach (var shard in shatter.shrapnels)
                {
                    Undo.RegisterCreatedObjectUndo(shard.gameObject, undoName);
                }

                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            GUI.backgroundColor = colorSave;

            // if(shatter.objectToShatter.gameObject != shatter.gameObject)
            if(shatter)
                serializedObject.ApplyModifiedProperties();
            
            // once you have an editor shatter apparently you are responsible for doing this
            if (GUI.changed)
            {
                // shatter.OnValidate();
                SetupStates();
            }
        }
    }
}