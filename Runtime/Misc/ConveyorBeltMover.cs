using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using SplineMeshTools.Core;

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

        private Dictionary<Rigidbody, (Spline spline, float position, int collisionCounts)> objectPositions
            = new Dictionary<Rigidbody, (Spline, float position, int collisionCounts)>();

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
                Rigidbody rb = collision.rigidbody;
                if (rb != null)
                {
                    // Find the closest spline and its closest position on that spline
                    (Spline closestSpline, float closestPosition) = SplineMeshUtils.FindClosestSplineAndPosition(splineContainer, collision.transform.position);

                    if (closestSpline != null)
                    {
                        if (!objectsOnBelt.Contains(rb))
                        {
                            objectsOnBelt.Add(rb);
                            objectPositions[rb] = (closestSpline, closestPosition, 1);
                        }
                        else
                        {
                            objectPositions[rb] = (closestSpline, closestPosition, objectPositions[rb].collisionCounts + 1);
                        }
                    }
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null && objectsOnBelt.Contains(rb))
            {
                objectPositions[rb] = (objectPositions[rb].spline, objectPositions[rb].position, objectPositions[rb].collisionCounts - 1);
                if (objectPositions[rb].collisionCounts == 0)
                {
                    objectsOnBelt.Remove(rb);
                    objectPositions.Remove(rb);
                }
            }
        }

        private void FixedUpdate()
        {
            for (int i = objectsOnBelt.Count - 1; i >= 0; i--)
            {
                var rb = objectsOnBelt[i];
                (Spline spline, float position, int collisionCount) = objectPositions[rb];
                Vector3 direction = spline.EvaluateTangent(position / spline.GetLength());
                int dir = reverseDirection ? -1 : 1;
                direction = direction * dir;
                // Calculate the new position along the spline
                position += dir * conveyorSpeed * Time.fixedDeltaTime;

                bool outOfConveyor = (!reverseDirection && position > spline.GetLength()) || (reverseDirection && (position < 0f));
                if (outOfConveyor)
                {
                    if (preserveMomentum)
                    {
                        // Apply a force in the last known direction to preserve momentum
                        rb.AddForce(direction * conveyorSpeed, ForceMode.VelocityChange);
                    }
                    objectPositions.Remove(rb);
                    objectsOnBelt.RemoveAt(i);
                    continue;
                }

                // Get the position on the spline
                Vector3 splinePosition = spline.EvaluatePosition(position / spline.GetLength());
                // Calculate the final position including height offset
                Vector3 finalPosition = splinePosition + splineContainer.transform.position + Vector3.up * (conveyorHeightOffset);
                finalPosition.y = rb.position.y;
                // Move the object to the new position
                rb.MovePosition(finalPosition);

                if (snapRotation)
                {
                    // Rotate the object while maintaining its original orientation
                    rb.MoveRotation(Quaternion.LookRotation(direction));
                }

                // Update the position in the dictionary
                objectPositions[rb] = (spline, position, objectPositions[rb].collisionCounts);
            }
        }

    }
}
