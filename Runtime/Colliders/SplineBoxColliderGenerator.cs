using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

namespace SplineMeshTools.Colliders
{
    [RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(SplineContainer))]
    public class SplineBoxColliderGenerator : SplineColliderGenerator
    {
        public float width = 1f;
        public float height = 1f;
        public int resolution = 10;
        public Vector3 offset = Vector3.zero;

		protected override void OnValidate()
        {
            // Ensure resolution is never below 1
            resolution = Mathf.Max(1, resolution);

            // Regenerate the mesh whenever values are changed in the editor
            GenerateAndAssignMesh();
        }

        public override Mesh GenerateColliderMesh()
        {
            var splineContainer = GetComponent<SplineContainer>();

            var combinedVertices = new List<Vector3>();
            var combinedTriangles = new List<int>();

            foreach (var spline in splineContainer.Splines)
            {
                var mesh = new Mesh();

                var vertices = new Vector3[resolution * 8];
                int[] triangles = new int[(resolution) * 36];

                for (int i = 0; i < resolution; i++)
                {
                    float t = i / (float)(resolution - 1);
                    var splinePosition = (Vector3)spline.EvaluatePosition(t) + offset;

                    var tangent = spline.EvaluateTangent(t);
                    var right = Vector3.Cross(Vector3.up, tangent).normalized * width / 2f;
                    var up = Vector3.up * height / 2f;

                    if (i == 0)
                    {
                        // Front face (first segment)
                        vertices[0] = splinePosition - right - up; // Bottom left
                        vertices[1] = splinePosition + right - up; // Bottom right
                        vertices[2] = splinePosition - right + up; // Top left
                        vertices[3] = splinePosition + right + up; // Top right
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

                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
            }

            var combinedMesh = new Mesh();

            combinedMesh.SetVertices(combinedVertices);
            combinedMesh.SetTriangles(combinedTriangles, 0);
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }
    }
}
