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

            //Segment count for twisting



            foreach (var spline in splineContainer.Splines)
            {
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();

                var knots = new List<BezierKnot>(spline.Knots);
                var knotRotations = new List<Quaternion>();

                foreach (var knot in knots)
                    knotRotations.Add(knot.Rotation);

                var submeshTriangles = new List<int>[normalizedSegmentMesh.subMeshCount];

                for (int i = 0; i < normalizedSegmentMesh.subMeshCount; i++)
                    submeshTriangles[i] = new List<int>();

                int segmentCount = knots.Count - 1;

                if (spline.Closed)
                    segmentCount++;

                var segmentRatios = new List<float>();

                // Calculate Segment Ratios for the resolution
                for (int i = 0; i < segmentCount; i++)
                {
                    float splinePoint = splineContainer.GetDistanceAlongSpline(splineCounter, knots[i % knots.Count].Position);
                    float ratio = splinePoint / spline.GetLength();
                    segmentRatios.Add(ratio);
                }

                segmentRatios.Add(1f);  //Add the last ratio which will be 1f

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
                        point = Mathf.Clamp01(point); // Ensure it's within [0,1]

                        // Determine which segment the point is in
                        int segmentIndex = 0;
                        for (int s = 0; s < segmentRatios.Count - 1; s++)
                        {
                            if (point >= segmentRatios[s] && point <= segmentRatios[s + 1])
                            {
                                segmentIndex = s;
                                break;
                            }
                        }

                        // Clamp the index to avoid going out of bounds
                        int nextIndex = Mathf.Min(segmentIndex + 1, knotRotations.Count - 1);

                        float localRatio = Mathf.InverseLerp(segmentRatios[segmentIndex], segmentRatios[nextIndex], point);

                        
                        float twistWeight = localRatio;

                        Quaternion twistRotation = Quaternion.Slerp(knotRotations[segmentIndex], knotRotations[nextIndex], twistWeight);


                        var tangent = (Vector3)spline.EvaluateTangent(point);
                        var splinePosition = (Vector3) spline.EvaluatePosition(point);
                        var splineRotation = Quaternion.LookRotation(tangent.normalized, shouldTwistMesh ? (twistRotation * Vector3.up) : Vector3.up);
                        var transformedPosition = splinePosition + splineRotation * vertexOffsets[counter];

                        vertices.Add(transformedPosition + positionAdjustment);
                        counter++;
                    }

                    // Add transformed normals
                    for (int j = 0; j < normalizedSegmentMesh.normals.Length; j++)
                    {
                        var normal = normalizedSegmentMesh.normals[j];
                        float point = (i / (float)meshResolution[splineCounter]) + (vertexRatios[j] * (1 / (float)meshResolution[splineCounter]));
                        point = Mathf.Clamp01(point);

                        // Determine which segment the point is in
                        int segmentIndex = 0;
                        for (int s = 0; s < segmentRatios.Count - 1; s++)
                        {
                            if (point >= segmentRatios[s] && point <= segmentRatios[s + 1])
                            {
                                segmentIndex = s;
                                break;
                            }
                        }


                        // Clamp the index to avoid going out of bounds
                        int nextIndex = Mathf.Min(segmentIndex + 1, knotRotations.Count - 1);

                        float localRatio = Mathf.InverseLerp(segmentRatios[segmentIndex], segmentRatios[nextIndex], point);

                        
                        float twistWeight = localRatio;

                        Quaternion twistRotation = Quaternion.Slerp(knotRotations[segmentIndex], knotRotations[nextIndex], twistWeight);


                        var tangent = (Vector3)spline.EvaluateTangent(point);
                        var splineRotation = Quaternion.LookRotation(tangent.normalized, shouldTwistMesh ? (twistRotation * Vector3.up) : Vector3.up);
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

        private int FindSegmentIndexFromRatios(List<float> segmentRatios, float t)
        {
            for (int i = 0; i < segmentRatios.Count - 1; i++)
            {
                if (t >= segmentRatios[i] && t <= segmentRatios[i + 1])
                    return i;
            }
            return segmentRatios.Count - 2; // Fallback to last segment
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
