using UnityEditor;
using SplineMeshTools.Core;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace SplineMeshTools.Editor
{
    [CustomEditor(typeof(SplineMesh), true)]
    public class SplineMeshEditor : UnityEditor.Editor
    {
		public override VisualElement CreateInspectorGUI()
		{
			var splineMesh = (SplineMesh)target;

			var root = new VisualElement();
			var defaultInspector = DrawDefaultInspector();

			var generateButton = new Button(() => splineMesh.GenerateMeshAlongSpline())
			{
				text = "Generate Mesh",
				style = { marginTop = 10f }
			};

			root.Add(defaultInspector);
			root.Add(generateButton);

			return root;
		}

		private new VisualElement DrawDefaultInspector() // Draws the default unity inspector with UI Toolkit
		{
			var container = new VisualElement();

			var iterator = serializedObject.GetIterator();

			if (iterator.NextVisible(true))
			{
				do
				{
					var propertyField = new PropertyField(iterator.Copy());

					if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
						propertyField.SetEnabled(false);

					container.Add(propertyField);
				}
				while (iterator.NextVisible(false));
			}

			return container;
		}
	}
}
