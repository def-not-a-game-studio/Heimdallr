using UnityEngine;
using UnityEditor;

namespace SpriteCombiner.EditorTools
{
    [CustomEditor(typeof(SpriteRendererCombiner))]
    /// <summary>
    /// Simple editor to provide a button to combine sprites via the SpriteRendererCombiner via button click
    /// </summary>
    public sealed class SpriteRendererCombinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(15);

            if (GUILayout.Button("Combine Sprites") && target is SpriteRendererCombiner src)
            {
                Undo.SetCurrentGroupName("Combine Sprites");
                Undo.RegisterFullObjectHierarchyUndo(src.gameObject, "Sprite Renderer Combiner");

                src.CombineSpriteRenderers();
            }
        }
    }
}
