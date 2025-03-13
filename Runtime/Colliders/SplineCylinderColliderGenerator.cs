using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

namespace SplineMeshTools.Colliders
{
	[RequireComponent(typeof(Rigidbody), typeof(MeshCollider), typeof(SplineContainer))]
	public class SplineCylinderColliderGenerator : SplineColliderGenerator
	{
        public float radius = 1f;
        public int resolution = 10;
        public int rings = 8;
        public bool generateEnds = true;
        public Vector3 offset = Vector3.zero;

        protected override void OnValidate()
        {
            // Ensure resolution is never below 1
            resolution = Mathf.Max(1, resolution);

            // Minimum 3 rings to form a cylinder
            rings = Mathf.Max(3, rings);

            // Regenerate the mesh whenever values are changed in the editor
            GenerateAndAssignMesh();
        }

        public override Mesh GenerateColliderMesh()
        {
            var splineContainer = GetComponent<SplineContainer>();

            var combinedVertices = new List<Vector3>();
            var combinedTriangles = new List<int>();

            int segments = resolution;
            float ringStep = Mathf.PI * 2 / rings;

            foreach (var spline in splineContainer.Splines)
            {
                var mesh = new Mesh();

                int vertexCount = (segments + 1) * (rings + 1);
                var vertices = new Vector3[vertexCount];
                var triangles = new List<int>();

                // Generate main cylinder body vertices
                for (int i = 0; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    var splinePosition = (Vector3)spline.EvaluatePosition(t) + offset;

                    for (int j = 0; j <= rings; j++)
                    {
                        float angle = j * ringStep;
                        var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                        var offsetPosition = direction * radius;

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

                mesh.SetVertices(combinedVertices);
                mesh.SetTriangles(combinedTriangles, 0);
                mesh.RecalculateNormals();
            }

            var combinedMesh = new Mesh();

            combinedMesh.SetVertices(combinedVertices);
            combinedMesh.SetTriangles(combinedTriangles, 0);
            combinedMesh.RecalculateNormals();

            return combinedMesh;
        }

        private void GenerateCylinderEnd(Vector3[] vertices, List<Vector3> combinedVertices, List<int> combinedTriangles, Spline spline, int segments, bool isStart)
        {
            int startIndex = isStart ? 0 : segments;
            var splinePosition = (Vector3)spline.EvaluatePosition(startIndex / (float)segments) + offset;

            int baseIndex = combinedVertices.Count;

            // Add the center point of the end cap
            combinedVertices.Add(splinePosition);

            // Generate vertices for the outer edge of the end cap
            for (int j = 0; j <= rings; j++)
            {
                float angle = j * Mathf.PI * 2 / rings;
                var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                var offsetPosition = direction * radius;

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
