using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using System.Linq;

namespace SplineMeshTools.Core
{
    public enum VectorAxis { X, Y }

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(SplineContainer)), DisallowMultipleComponent, ExecuteInEditMode]
    public class SplineMesh : MonoBehaviour
    {
        [Tooltip("Generates mesh automatically when spline is modified." +
            " Set to 'false' to save in-editor performance and generate mesh manually")]      
        [SerializeField] protected bool autoGenerateMesh;

        [Space]
        [Header("Spline Mesh Settings")]
        [SerializeField] protected Mesh segmentMesh;

        [Tooltip("The name of the mesh to be generated")]
        [SerializeField] protected string meshName;

        [Tooltip("The local axis along which the mesh extends")]
        [SerializeField] protected VectorAxis forwardAxis;

        [Tooltip("Axis for the UV to be stretched")]
        [SerializeField] protected VectorAxis uvAxis;

        [Tooltip("Whether UVs are uniformly spread out, or based on the spline points")]
        [SerializeField] protected bool uniformUVs;

        [Tooltip("The UV Resolutions along spline(s). Count must match the same number of splines in the container.")]
        [SerializeField] protected float[] uvResolutions;

        [Space]
        [Header("Offsets")]
        [SerializeField] protected Vector3 positionAdjustment;
        [SerializeField] protected Quaternion rotationAdjustment;
        [SerializeField] protected Vector3 scaleAdjustment = Vector3.one;

        protected SplineContainer splineContainer;
        protected MeshFilter meshFilter;

        private bool autoGenFlag;

        void Awake() => meshFilter = GetComponent<MeshFilter>();

        void OnEnable()
        {
            splineContainer = GetComponent<SplineContainer>();
            autoGenFlag = autoGenerateMesh;
            if(autoGenerateMesh)
                Spline.Changed += OnSplineModified;
        }

        void OnDisable()
        {
            if(!autoGenerateMesh)
                Spline.Changed -= OnSplineModified;
        }

        private void OnValidate()
        {
            if(autoGenerateMesh && !autoGenFlag)
            {
                Spline.Changed += OnSplineModified;
            }
            else if(!autoGenerateMesh && autoGenFlag)
            {
                Spline.Changed -= OnSplineModified;
            }
            autoGenFlag = autoGenerateMesh;

        }

        public virtual void GenerateMeshAlongSpline()
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

                float meshBoundsDistance = Mathf.Abs(SplineMeshUtils.GetRequiredAxis(normalizedSegmentMesh.bounds.size, forwardAxis));
                var vertexRatios = new List<float>();
                var vertexOffsets = new List<Vector3>();

                // Calculate vertex ratios and offsets
                foreach (var vertex in normalizedSegmentMesh.vertices)
                {
                    var offset = SplineMeshUtils.GetRequiredOffset(vertex, forwardAxis);
                    float ratio = Mathf.Abs(SplineMeshUtils.GetRequiredAxis(vertex, forwardAxis)) / meshBoundsDistance;

                    vertexRatios.Add(ratio);
                    vertexOffsets.Add(offset);
                }

                // Loop through each segment of the spline
                for (int i = 0; i < segmentCount; i++)
                {
                    int counter = 0;

                    foreach (var vector in normalizedSegmentMesh.vertices)
                    {
                        float point = segmentRatios[i] + (vertexRatios[counter] * (segmentRatios[(i + 1) % segmentRatios.Count] - segmentRatios[i]));
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
                        float point = segmentRatios[i] + (vertexRatios[j] * (segmentRatios[(i + 1) % segmentRatios.Count] - segmentRatios[i]));
                        var tangent = spline.EvaluateTangent(point);

                        var splineRotation = Quaternion.LookRotation(tangent, Vector3.up);
                        var transformedNormal = splineRotation * normal;

                        normals.Add(transformedNormal);
                    }

                    // Add triangles to each submesh
                    for (int submeshIndex = 0; submeshIndex < normalizedSegmentMesh.subMeshCount; submeshIndex++)
                    {
                        int[] submeshIndices = normalizedSegmentMesh.GetTriangles(submeshIndex);

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
                        float point;
                        var uv = normalizedSegmentMesh.uv[j];

                        if (uniformUVs)
                        {
                            point = segmentRatios[i] + (vertexRatios[j] * (segmentRatios[(i + 1) % segmentRatios.Count] - segmentRatios[i]));
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

                // Combine current spline mesh data into the combined lists
                combinedVertices.AddRange(vertices);
                combinedNormals.AddRange(normals);
                combinedUVs.AddRange(uvs);

                splineCounter++;
            }

            var generatedMesh = new Mesh();
            generatedMesh.name = meshName;
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

        protected virtual bool CheckForErrors()
        {
            if (segmentMesh == null)
            {
                Debug.LogError("No Segment Mesh Assigned");
                return true;
            }

            if (uvResolutions.Length != splineContainer.Splines.Count)
            {
                Debug.LogError("UV Resolutions array count must match the number of Splines in the Spline Container");
                return true;
            }

            return false;
        }

        public void OnSplineModified(Spline spline, int knotIndex, SplineModification modification)
        {

            if (spline == null || segmentMesh == null)
                return;

            if (splineContainer.Splines.Contains(spline))
                GenerateMeshAlongSpline();
        }

    }
}
