using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace SplineMeshTools.Colliders
{

    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class SplineBoxColliderGenerator : MonoBehaviour
    {
        public float width = 1f;
        public float height = 1f;
        public Vector3 offset = Vector3.zero;
        public int resolution = 10;

        private MeshCollider meshCollider;

        private void OnValidate()
        {
            // Ensure resolution is never below 1
            resolution = Mathf.Max(1, resolution);

            // Regenerate the mesh whenever values are changed in the editor
            GenerateAndAssignMesh();
        }

        private void GenerateAndAssignMesh()
        {
            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();

                if (meshCollider == null)
                {
                    meshCollider = gameObject.AddComponent<MeshCollider>();
                }
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (!rb.isKinematic)
            {
                Debug.LogWarning("Rigidbody is changed to be Kinematic.");
                rb.isKinematic = true;
            }
            meshCollider.sharedMesh = GenerateBoxColliderMesh();
        }

        public Mesh GenerateBoxColliderMesh()
        {
            SplineContainer splineContainer = GetComponent<SplineContainer>();

            List<Vector3> combinedVertices = new List<Vector3>();
            List<int> combinedTriangles = new List<int>();

            foreach (Spline spline in splineContainer.Splines)
            {
                Mesh mesh = new Mesh();

                Vector3[] vertices = new Vector3[resolution * 8];
                int[] triangles = new int[(resolution) * 36];

                for (int i = 0; i < resolution; i++)
                {
                    float t = i / (float)(resolution - 1);
                    Vector3 splinePosition = (Vector3)spline.EvaluatePosition(t) + offset;

                    Vector3 tangent = spline.EvaluateTangent(t);
                    Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized * width / 2f;
                    Vector3 up = Vector3.up * height / 2f;

                    if (i == 0)
                    {
                        // Front face (first segment)
                        vertices[0] = splinePosition - right - up;      // Bottom left
                        vertices[1] = splinePosition + right - up;      // Bottom right
                        vertices[2] = splinePosition - right + up;      // Top left
                        vertices[3] = splinePosition + right + up;      // Top right
                    }

                    // Back face (offset by depth)
                    vertices[i * 4 + 4] = splinePosition - right - up;
                    vertices[i * 4 + 5] = splinePosition + right - up;
                    vertices[i * 4 + 6] = splinePosition - right + up;
                    vertices[i * 4 + 7] = splinePosition + right + up;

                    // Triangle assignment
                    if (i < resolution)
                    {
                        int vi = i * 4;
                        int ti = i * 36;

                        // Front face
                        triangles[ti] = vi;
                        triangles[ti + 1] = vi + 2;
                        triangles[ti + 2] = vi + 1;

                        triangles[ti + 3] = vi + 1;
                        triangles[ti + 4] = vi + 2;
                        triangles[ti + 5] = vi + 3;

                        // Back face
                        triangles[ti + 6] = vi + 4;
                        triangles[ti + 7] = vi + 5;
                        triangles[ti + 8] = vi + 6;

                        triangles[ti + 9] = vi + 6;
                        triangles[ti + 10] = vi + 5;
                        triangles[ti + 11] = vi + 7;

                        // Left face
                        triangles[ti + 12] = vi;
                        triangles[ti + 13] = vi + 4;
                        triangles[ti + 14] = vi + 2;

                        triangles[ti + 15] = vi + 4;
                        triangles[ti + 16] = vi + 6;
                        triangles[ti + 17] = vi + 2;

                        // Right face
                        triangles[ti + 18] = vi + 1;
                        triangles[ti + 19] = vi + 3;
                        triangles[ti + 20] = vi + 5;

                        triangles[ti + 21] = vi + 5;
                        triangles[ti + 22] = vi + 3;
                        triangles[ti + 23] = vi + 7;

                        // Top face
                        triangles[ti + 24] = vi + 2;
                        triangles[ti + 25] = vi + 6;
                        triangles[ti + 26] = vi + 3;

                        triangles[ti + 27] = vi + 3;
                        triangles[ti + 28] = vi + 6;
                        triangles[ti + 29] = vi + 7;

                        // Bottom face
                        triangles[ti + 30] = vi;
                        triangles[ti + 31] = vi + 1;
                        triangles[ti + 32] = vi + 4;

                        triangles[ti + 33] = vi + 4;
                        triangles[ti + 34] = vi + 1;
                        triangles[ti + 35] = vi + 5;
                    }
                }

                combinedVertices.AddRange(vertices);
                combinedTriangles.AddRange(triangles);
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.SetVertices(combinedVertices);
            combinedMesh.SetTriangles(combinedTriangles, 0);
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }


    }
}