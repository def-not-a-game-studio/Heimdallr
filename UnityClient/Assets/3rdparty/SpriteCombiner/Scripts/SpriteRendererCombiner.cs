using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpriteCombiner
{
    [DisallowMultipleComponent]
    /// <summary>
    /// MonoBehaviour class to drive creation of a combined sprite from a collection of sprite renderer sources
    /// </summary>
    public sealed class SpriteRendererCombiner : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> spriteRenderersToCombine;
        [SerializeField] private SpriteCombinerSettings spriteCombinerSettings;

        public void CombineSpriteRenderers()
        {
            if (spriteRenderersToCombine?.Count > 0)
            {
                SpriteRenderer[] sortedSpriteRenderers = spriteRenderersToCombine.OrderBy(sr => sr.sortingOrder).ToArray();

                if (!TryGetComponent(out SpriteRenderer newSR))
                    newSR = gameObject.AddComponent<SpriteRenderer>();

                if (newSR != null)
                {
                    // Perform sprite combination
                    Sprite combinedSprite = SpriteCombiner.CombineSprites
                    (
                        sortedSpriteRenderers,
                        spriteCombinerSettings,
                        out Vector3 centerPos
                    );
                    if (combinedSprite != null)
                    {
                        newSR.sprite = combinedSprite;
                        newSR.transform.position = centerPos;
                    }
                }

                // Cleanup
                for (int i = 0; i < spriteRenderersToCombine.Count; i++)
                {
                    DestroyImmediate(spriteRenderersToCombine[i].gameObject);
                }

                DestroyImmediate(this);
            }
        }

        private void Start()
        {
            CombineSpriteRenderers();
        }
    }
}
