using UnityEngine;
using UnityEngine.Splines;
using SplineMeshTools.Core;
using System.Collections.Generic;

namespace SplineMeshTools.Misc
{
    public class ConveyorBeltMover : MonoBehaviour
    {
        [Tooltip("Assign the Spline Container")]
        [SerializeField] SplineContainer splineContainer;

        [Tooltip("Speed at which objects move along the spline")]
        [SerializeField] float conveyorSpeed = 1.0f;

        [Tooltip("Height Offset for the conveyor. Useful")]
        [SerializeField] float conveyorHeightOffset = 0.0f;

        [Tooltip("Should the objects in the belt snap it's rotation to the tangents of the spline?")]
        [SerializeField] bool snapRotation = false;

        [Tooltip("Should the objects move in the reverse direction of the spline?")]
        [SerializeField] bool reverseDirection = false;

        [Tooltip("Should moving objects preserve momentum once out of the spline?")]
        [SerializeField] bool preserveMomentum = true;

        private List<Rigidbody> objectsOnBelt = new List<Rigidbody>();
        private Dictionary<Rigidbody, (Spline spline, float position, int collisionCounts)> objectPositions = new();

        private void Start()
        {
            if (splineContainer == null)
            {
                splineContainer = GetComponent<SplineContainer>();

                if (splineContainer == null)
                    Debug.LogError("Spline Container must be assigned");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.contacts[0].point.y > (transform.position.y + conveyorHeightOffset))
            {
                var rigidbody = collision.rigidbody;

                if (rigidbody != null)
                {
                    // Find the closest spline and its closest position on that spline
                    (Spline closestSpline, float closestPosition) = SplineMeshUtils.FindClosestSplineAndPosition(splineContainer, collision.transform.position);

                    if (closestSpline != null)
                    {
                        if (!objectsOnBelt.Contains(rigidbody))
                        {
                            objectsOnBelt.Add(rigidbody);
                            objectPositions[rigidbody] = (closestSpline, closestPosition, 1);
                        }
                        else
                        {
                            objectPositions[rigidbody] = (closestSpline, closestPosition, objectPositions[rigidbody].collisionCounts + 1);
                        }
                    }
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            var rigodbody = collision.rigidbody;

            if (rigodbody != null && objectsOnBelt.Contains(rigodbody))
            {
                objectPositions[rigodbody] = (objectPositions[rigodbody].spline, objectPositions[rigodbody].position, objectPositions[rigodbody].collisionCounts - 1);

                if (objectPositions[rigodbody].collisionCounts == 0)
                {
                    objectsOnBelt.Remove(rigodbody);
                    objectPositions.Remove(rigodbody);
                }
            }
        }

        private void FixedUpdate()
        {
            for (int i = objectsOnBelt.Count - 1; i >= 0; i--)
            {
                var rigidbody = objectsOnBelt[i];
                (Spline spline, float position, int collisionCount) = objectPositions[rigidbody];
                var direction = spline.EvaluateTangent(position / spline.GetLength());
                int dir = reverseDirection ? -1 : 1;

                direction = direction * dir;
                // Calculate the new position along the spline
                position += dir * conveyorSpeed * Time.fixedDeltaTime;

                bool outOfConveyor = (!reverseDirection && position > spline.GetLength()) || (reverseDirection && (position < 0f));

                if (outOfConveyor)
                {
                    if (preserveMomentum) // Apply a force in the last known direction to preserve momentum
                        rigidbody.AddForce(direction * conveyorSpeed, ForceMode.VelocityChange);

                    objectPositions.Remove(rigidbody);
                    objectsOnBelt.RemoveAt(i);
                    continue;
                }

                // Get the position on the spline
                Vector3 splinePosition = spline.EvaluatePosition(position / spline.GetLength());
                // Calculate the final position including height offset
                var finalPosition = splinePosition + splineContainer.transform.position + Vector3.up * (conveyorHeightOffset);
                finalPosition.y = rigidbody.position.y;
                // Move the object to the new position
                rigidbody.MovePosition(finalPosition);

                if (snapRotation) // Rotate the object while maintaining its original orientation                    
					rigidbody.MoveRotation(Quaternion.LookRotation(direction));

                // Update the position in the dictionary
                objectPositions[rigidbody] = (spline, position, objectPositions[rigidbody].collisionCounts);
            }
        }
    }
}
