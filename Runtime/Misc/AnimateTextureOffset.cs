using UnityEngine;

namespace SplineMeshTools.Misc
{
    public class AnimateTextureOffset : MonoBehaviour
    {
        [Tooltip("Cached index of the material to animate")]
        [SerializeField] int materialIndex = 0;

        [Tooltip("Direction of the texture offset animation")]
        [SerializeField] Vector2 offsetDirection = Vector2.right;

        [Tooltip("Speed of the animation")]
        [SerializeField] float speed = 1.0f;

        private MeshRenderer meshRenderer;
        private Material material;
        private Vector2 originalOffset;

        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.materials.Length > materialIndex)
            {
                material = meshRenderer.materials[materialIndex];
                originalOffset = material.mainTextureOffset;
            }
            else
            {
                Debug.LogError("MeshRenderer or material index out of range.");
            }
        }

        void Update()
        {
            if (material != null)
            {
                // Calculate the new offset value for a continuous loop
                Vector2 newOffset = originalOffset + offsetDirection * speed * Time.time;

                // Apply the new texture offset to the material
                material.mainTextureOffset = newOffset;
            }
        }
    }
}
