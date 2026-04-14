#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SplineMeshTools.Editor
{
    public static class SplineMeshPrefabSaver
    {
        [MenuItem("SplineMesh/Save Selected as Prefab")]
        public static void SaveAsPrefab()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Save Prefab", "No GameObject selected.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save SplineMesh as Prefab",
                selected.name,
                "prefab",
                "Choose where to save the prefab",
                "Assets"
            );

            if (string.IsNullOrEmpty(path)) return;

            // Save the mesh as a sub-asset of the prefab so it doesn't get lost
            MeshFilter mf = selected.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh meshCopy = Object.Instantiate(mf.sharedMesh);
                meshCopy.name = selected.name + "_Mesh";

                // Create prefab first, then add mesh as sub-asset
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(selected, path);
                AssetDatabase.AddObjectToAsset(meshCopy, path);

                // Re-link the mesh inside the prefab
                MeshFilter prefabMF = prefab.GetComponent<MeshFilter>();
                prefabMF.sharedMesh = meshCopy;
                PrefabUtility.SavePrefabAsset(prefab);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(selected, path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SplineMesh] Prefab saved to: {path}");
        }
    }
}
#endif
    