using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

namespace SplineMeshTools.Core
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(SplineContainer)), ExecuteInEditMode, DisallowMultipleComponent]
    public class SplineMeshResolution : SplineMesh
    {
        [Space]
        [Header("Mesh Resolution Settings")]

        [Tooltip("Count must match the number of Splines in the Spline Container")]
        [SerializeField] private int[] meshResolution;

        public override void GenerateMeshAlongSpline()
        {
            if(CheckForErrors()) return;

            var combinedVertices = new List<Vector3>();
            var combinedNormals = new List<Vector3>();
            var combinedUVs = new List<Vector2>();
            var combinedSubmeshTriangles = new List<int>[segmentMesh.subMeshCount];

            for (int i = 0; i < segmentMesh.subMeshCount; i++)
                combinedSubmeshTriangles[i] = new List<int>();

            int combinedVertexOffset = 0;
            int splineCounter = 0;

            var normalizedSegmentMesh = segmentMesh.NormalizeMesh(rotationAdjustment, scaleAdjustment);

            foreach (var spline in splineContainer.Splines)
            {
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();

                var knots = new List<BezierKnot>(spline.Knots);
                var submeshTriangles = new List<int>[normalizedSegmentMesh.subMeshCount];

                for (int i = 0; i < normalizedSegmentMesh.subMeshCount; i++)
                    submeshTriangles[i] = new List<int>();

                int segmentCount = knots.Count - 1;

                if (meshResolution.Length == 0)
                {
                    Debug.LogError("The Mesh Resolution array is empty");
                    return;
                }

                // Loop through each resolution of the spline
                for (int i = 0; i < meshResolution[splineCounter]; i++)
                {
                    float meshBoundsDistance = Mathf.Abs(SplineMeshUtils.GetRequiredAxis(normalizedSegmentMesh.bounds.size, forwardAxis));

                    var vertexRatios = new List<float>();
                    var vertexOffsets = new List<Vector3>();

                    // Calculate vertex ratios and offsets
                    foreach (var vertex in normalizedSegmentMesh.vertices)
                    {
                        float ratio = Mathf.Abs(SplineMeshUtils.GetRequiredAxis(vertex, forwardAxis)) / meshBoundsDistance;
                        var offset = SplineMeshUtils.GetRequiredOffset(vertex, forwardAxis);
                        vertexRatios.Add(ratio);
                        vertexOffsets.Add(offset);

                    }

                    int counter = 0;

                    foreach (var vertex in normalizedSegmentMesh.vertices)
                    {
                        float point = (i / (float)meshResolution[splineCounter]) + (vertexRatios[counter] * (1 / (float)meshResolution[splineCounter]));
                        var tangent = spline.EvaluateTangent(point);
                        Vector3 splinePosition = spline.EvaluatePosition(point);

                        var splineRotation = Quaternion.LookRotation(tangent, Vector3.up);
                        var transformedPosition = splinePosition + splineRotation * vertexOffsets[counter];

                        vertices.Add(transformedPosition + positionAdjustment);
                        counter++;
                    }

                    // Add transformed normals
                    for (int j = 0; j < normalizedSegmentMesh.normals.Length; j++)
                    {
                        var normal = normalizedSegmentMesh.normals[j];
                        float point = (i / (float)meshResolution[splineCounter]) + (vertexRatios[j] * (1 / (float)meshResolution[splineCounter]));

                        var tangent = spline.EvaluateTangent(point);
                        var splineRotation = Quaternion.LookRotation(tangent, Vector3.up);
                        var transformedNormal = splineRotation * normal;

                        normals.Add(transformedNormal);
                    }

                    // Add triangles to each submesh
                    for (int submeshIndex = 0; submeshIndex < normalizedSegmentMesh.subMeshCount; submeshIndex++)
                    {
                        var submeshIndices = normalizedSegmentMesh.GetTriangles(submeshIndex);

                        for (int k = 0; k < submeshIndices.Length; k += 3)
                        {
                            combinedSubmeshTriangles[submeshIndex].Add(submeshIndices[k] + combinedVertexOffset);
                            combinedSubmeshTriangles[submeshIndex].Add(submeshIndices[k + 2] + combinedVertexOffset);
                            combinedSubmeshTriangles[submeshIndex].Add(submeshIndices[k + 1] + combinedVertexOffset);
                        }
                    }

                    // Add UVs with UV resolution
                    for (int j = 0; j < normalizedSegmentMesh.uv.Length; j++)
                    {
                        var uv = normalizedSegmentMesh.uv[j];
                        float point;

                        if (uniformUVs)
                        {
                            point = (i / (float)meshResolution[splineCounter]) + (vertexRatios[j] * (1 / (float)meshResolution[splineCounter]));
                        }
                        else
                        {
                            point = (i / (float)segmentCount) + (vertexRatios[j] * (1 / (float)segmentCount));
                        }

                        var splineUV = SplineMeshUtils.MakeUVs(uv, point, splineCounter, uvAxis, uvResolutions); // Apply UV resolution
                        uvs.Add(splineUV);
                    }

                    combinedVertexOffset += normalizedSegmentMesh.vertexCount;
                }

                combinedVertices.AddRange(vertices);
                combinedNormals.AddRange(normals);
                combinedUVs.AddRange(uvs);
                splineCounter++;
            }

            var generatedMesh = new Mesh();
            generatedMesh.name = "Spline Mesh";
            generatedMesh.vertices = combinedVertices.ToArray();
            generatedMesh.normals = combinedNormals.ToArray();
            generatedMesh.uv = combinedUVs.ToArray();
            generatedMesh.subMeshCount = segmentMesh.subMeshCount;

            for (int submeshIndex = 0; submeshIndex < segmentMesh.subMeshCount; submeshIndex++)
                generatedMesh.SetTriangles(combinedSubmeshTriangles[submeshIndex].ToArray(), submeshIndex);

            meshFilter.mesh = generatedMesh;

            generatedMesh.RecalculateBounds();
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateTangents();
        }

        protected override bool CheckForErrors()
        {
            if (base.CheckForErrors()) return true;

            if (meshResolution.Length != splineContainer.Splines.Count)
            {
                Debug.LogError("Mesh Resolution array count must match the number of Splines in the Spline Container");
                return true;
            }

            return false;
        }
    }
}
