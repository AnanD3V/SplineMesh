using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace SplineMeshTools.Colliders
{

    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class SplineCylinderColliderGenerator : MonoBehaviour
    {
        public float radius = 1f;
        public Vector3 offset = Vector3.zero;
        public int resolution = 10;
        public int rings = 8;
        public bool generateEnds = true;

        private MeshCollider meshCollider;

        private void OnValidate()
        {
            // Ensure resolution is never below 1
            resolution = Mathf.Max(1, resolution);

            // Minimum 3 rings to form a cylinder
            rings = Mathf.Max(3, rings);

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

            meshCollider.sharedMesh = GenerateCylinderColliderMesh();
        }

        public Mesh GenerateCylinderColliderMesh()
        {
            SplineContainer splineContainer = GetComponent<SplineContainer>();

            List<Vector3> combinedVertices = new List<Vector3>();
            List<int> combinedTriangles = new List<int>();

            int segments = resolution;
            float ringStep = Mathf.PI * 2 / rings;

            foreach (Spline spline in splineContainer.Splines)
            {
                Mesh mesh = new Mesh();

                int vertexCount = (segments + 1) * (rings + 1);
                Vector3[] vertices = new Vector3[vertexCount];
                List<int> triangles = new List<int>();

                // Generate main cylinder body vertices
                for (int i = 0; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 splinePosition = (Vector3)spline.EvaluatePosition(t) + offset;

                    for (int j = 0; j <= rings; j++)
                    {
                        float angle = j * ringStep;
                        Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                        Vector3 offsetPosition = direction * radius;

                        // Add vertices for the cylinder
                        vertices[i * (rings + 1) + j] = splinePosition + offsetPosition;
                    }
                }

                // Create triangles for the main cylinder body
                for (int i = 0; i < segments; i++)
                {
                    for (int j = 0; j < rings; j++)
                    {
                        int current = i * (rings + 1) + j;
                        int next = (i + 1) * (rings + 1) + j;
                        int nextRing = (i + 1) * (rings + 1) + (j + 1);
                        int currentRing = i * (rings + 1) + (j + 1);

                        triangles.Add(current);
                        triangles.Add(next);
                        triangles.Add(currentRing);

                        triangles.Add(currentRing);
                        triangles.Add(next);
                        triangles.Add(nextRing);
                    }
                }

                combinedVertices.AddRange(vertices);
                combinedTriangles.AddRange(triangles);

                if (generateEnds)
                {
                    // Generate flat end caps
                    GenerateCylinderEnd(vertices, combinedVertices, combinedTriangles, spline, segments, true);
                    GenerateCylinderEnd(vertices, combinedVertices, combinedTriangles, spline, segments, false);
                }

                mesh.vertices = combinedVertices.ToArray();
                mesh.triangles = combinedTriangles.ToArray();
                mesh.RecalculateNormals();
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.SetVertices(combinedVertices);
            combinedMesh.SetTriangles(combinedTriangles, 0);
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }

        private void GenerateCylinderEnd(Vector3[] vertices, List<Vector3> combinedVertices, List<int> combinedTriangles, Spline spline, int segments, bool isStart)
        {
            int startIndex = isStart ? 0 : segments;
            Vector3 splinePosition = (Vector3)spline.EvaluatePosition(startIndex / (float)segments) + offset;

            int baseIndex = combinedVertices.Count;

            // Add the center point of the end cap
            combinedVertices.Add(splinePosition);

            // Generate vertices for the outer edge of the end cap
            for (int j = 0; j <= rings; j++)
            {
                float angle = j * Mathf.PI * 2 / rings;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                Vector3 offsetPosition = direction * radius;

                combinedVertices.Add(splinePosition + offsetPosition);
            }

            // Generate triangles for the end cap
            for (int j = 0; j < rings; j++)
            {
                combinedTriangles.Add(baseIndex); // Center point
                combinedTriangles.Add(baseIndex + j + 1);
                combinedTriangles.Add(baseIndex + j + 2);
            }
        }
    }
}
