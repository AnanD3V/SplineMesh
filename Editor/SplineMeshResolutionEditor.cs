using UnityEditor;
using UnityEngine;
using SplineMeshTools.Core;

namespace SplineMeshTools.Editor
{
    [CustomEditor(typeof(SplineMeshResolution))]
    public class SplineMeshResolutionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawGenerateButton();
        }

        private void DrawGenerateButton()
        {
            GUILayout.Space(10f);

            SplineMesh splineMesh = (SplineMesh)target;

            // Generate Mesh Button with custom color
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fixedHeight = 30;
            buttonStyle.fixedWidth = 150;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            if (GUILayout.Button("Generate Mesh", buttonStyle))
            {
                splineMesh.GenerateMeshAlongSpline();
            }

            GUI.backgroundColor = originalColor;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
