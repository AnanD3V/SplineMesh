using UnityEngine;

public abstract class SplineColliderGenerator : MonoBehaviour
{
	protected MeshCollider meshCollider;

	protected virtual void OnValidate() => GenerateAndAssignMesh();

	protected virtual void GenerateAndAssignMesh()
	{
		if (meshCollider == null)
		{
			meshCollider = GetComponent<MeshCollider>();

			if (meshCollider == null)
				meshCollider = gameObject.AddComponent<MeshCollider>();
		}

		var rigidbody = GetComponent<Rigidbody>();

		if (!rigidbody.isKinematic)
		{
			Debug.LogWarning("Rigidbody is changed to be Kinematic.");
			rigidbody.isKinematic = true;
		}

		meshCollider.sharedMesh = GenerateColliderMesh();
	}

	public abstract Mesh GenerateColliderMesh();
}
