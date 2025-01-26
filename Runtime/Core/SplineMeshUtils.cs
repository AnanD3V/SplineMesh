using UnityEngine;
using UnityEngine.Splines;

namespace SplineMeshTools.Core
{
	public static class SplineMeshUtils
    {
        public static Mesh NormalizeMesh(this Mesh mesh, Quaternion rotationAdjustment, Vector3 scaleAdjustment)
        {
            var normalizedMesh = Object.Instantiate(mesh);
            var vertices = normalizedMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Scale(vertices[i], scaleAdjustment);
                vertices[i] = rotationAdjustment * vertices[i];
            }

            normalizedMesh.vertices = vertices;

            var normals = normalizedMesh.normals;

            for (int i = 0; i < normals.Length; i++)
                normals[i] = rotationAdjustment * normals[i];

            normalizedMesh.normals = normals;

            normalizedMesh.RecalculateBounds();
            normalizedMesh.RecalculateTangents();

            return normalizedMesh;
        }

		public static float GetDistanceAlongSpline(this SplineContainer splineContainer, int index, Vector3 point, int samples = 100)
		{
			var spline = splineContainer.Splines[index];
			float closestDistance = float.MaxValue;
			float closestT = 0f;

			// Find the closest t value
			for (int i = 0; i <= samples; i++)
			{
				float t = i / (float)samples;
				var splinePoint = spline.EvaluatePosition(t);
				float distanceToSplinePoint = Vector3.Distance(point, splinePoint);

				if (distanceToSplinePoint < closestDistance)
				{
					closestDistance = distanceToSplinePoint;
					closestT = t;
				}
			}

			float distance = 0f;
			int segments = 1000;
			var previousPoint = spline.EvaluatePosition(0f);

			for (int i = 1; i <= segments; i++)
			{
				float t = i / (float)segments * closestT;
				var splinePoint = spline.EvaluatePosition(t);

				distance += Vector3.Distance(previousPoint, splinePoint);
				previousPoint = splinePoint;
			}

			return distance;
		}

		public static Vector2 MakeUVs(Vector2 uv, float point, int splineCount, VectorAxis uvAxis, float[] uvResolutions)
		{
			if (uvResolutions.Length == 0)
			{
				Debug.LogError("The UV resolution array is empty");
				return Vector2.zero;
			}

			switch (uvAxis)
			{
				case VectorAxis.X:
					return new Vector2(point * uvResolutions[splineCount], uv.y);
				default:
					return new Vector2(uv.x, point * uvResolutions[splineCount]);
			}
		}

		public static Vector3 GetRequiredOffset(Vector3 vector, VectorAxis axis)
		{
			switch (axis)
			{
				case VectorAxis.X:
					return new Vector3(vector.y, vector.z, 0f);

				default:
				case VectorAxis.Y:
					return new Vector3(vector.x, vector.z, 0f);
			}
		}

		public static float GetRequiredAxis(Vector3 vector, VectorAxis axis)
		{
			switch (axis)
			{
				case VectorAxis.X:
					return vector.x;

				default:
				case VectorAxis.Y:
					return vector.y;
			}
		}

        public static (Spline, float) FindClosestSplineAndPosition(SplineContainer splineContainer, Vector3 objectPosition)
        {
            Spline closestSpline = null;
            float closestPosition = 0f;
            float minDistance = float.MaxValue;

            foreach (Spline spline in splineContainer.Splines)
            {
                for (float t = 0; t <= 1; t += 0.01f)
                {
                    Vector3 pointOnSpline = (Vector3)spline.EvaluatePosition(t) + splineContainer.transform.position;
                    float distance = Vector3.Distance(objectPosition, pointOnSpline);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestSpline = spline;
                        closestPosition = t * spline.GetLength();
                    }
                }
            }
            return (closestSpline, closestPosition);
        }
    }
}
