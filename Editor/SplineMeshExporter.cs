#if UNITY_EDITOR
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditor;
using UnityEngine;

namespace SplineMeshTools.Editor
{
    public static class SplineMeshExporter
    {
        [MenuItem("SplineMesh/Export Selected to FBX")]
        public static void ExportToFBX()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Export FBX", "No GameObject selected.", "OK");
                return;
            }

            MeshFilter mf = selected.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                EditorUtility.DisplayDialog("Export FBX", "Selected object has no mesh.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel(
                "Export SplineMesh as FBX",
                Application.dataPath,
                selected.name,
                "fbx"
            );

            if (string.IsNullOrEmpty(path)) return;

            // ModelExporter preserves UVs, normals, and submeshes
            ModelExporter.ExportObject(path, selected);
            AssetDatabase.Refresh();

            Debug.Log($"[SplineMesh] Exported FBX to: {path}");
        }
    }
}
#endif