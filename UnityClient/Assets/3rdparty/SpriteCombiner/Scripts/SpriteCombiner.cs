using System.Collections.Generic;
using UnityEngine;

namespace SpriteCombiner
{
    /// <summary>
    /// Main entrypoint into sprite combination functionality, SpriteCombiner coordinates the creation of a combined sprite
    /// </summary>
    public static class SpriteCombiner
    {
        /// <summary>
        /// Creates a combined sprite from the given sprite renderers with the given settings
        /// </summary>
        /// <param name="spriteRenderersToCombine">The collection of sprite renderers to use as sources</param>
        /// <param name="settings">The sprite combiner settings influencing the created sprite</param>
        /// <param name="centerPos">Optional out parameter, representing the center of the combined sprite rect</param>
        /// <returns>Sprite The combined sprite result</returns>
        public static Sprite CombineSprites(SpriteRenderer[] spriteRenderersToCombine, SpriteCombinerSettings settings, out Vector3 centerPos)
        {
            // Calculating combined bounds of all sprites in pixel space
            Bounds combinedBounds = default;
            for (int i = 0; i < spriteRenderersToCombine.Length; i++)
            {
                SpriteRenderer sr = spriteRenderersToCombine[i];
                if (sr == null || sr.sprite == null) { continue; }

                Vector2 spriteCenterInPixels = sr.bounds.center * sr.sprite.pixelsPerUnit;
                Vector2 spriteSizeInPixels = sr.bounds.size * sr.sprite.pixelsPerUnit;

                if (sr.sprite.pixelsPerUnit == settings.pixelsPerUnit)
                {
                    if (combinedBounds == default)
                        combinedBounds = new Bounds(spriteCenterInPixels, spriteSizeInPixels);
                    else
                        combinedBounds.Encapsulate(new Bounds(spriteCenterInPixels, spriteSizeInPixels));
                }
            }

            centerPos = combinedBounds.center / settings.pixelsPerUnit;

            Rect combinedRect = new Rect
            (
                0,
                0,
                Mathf.CeilToInt(combinedBounds.size.x),
                Mathf.CeilToInt(combinedBounds.size.y)
            );

            // Creating sprite texture data from sprite renderer sources
            List<SpriteTextureData> spriteTextureData = new List<SpriteTextureData>(spriteRenderersToCombine.Length);
            for (int i = 0; i < spriteRenderersToCombine.Length; i++)
            {
                SpriteRenderer sr = spriteRenderersToCombine[i];
                if (sr == null || sr.sprite == null) { continue; }

                if (sr.sprite.pixelsPerUnit == settings.pixelsPerUnit)
                {
                    Vector2 texPos = new Vector2
                    (
                        (sr.bounds.min.x * sr.sprite.pixelsPerUnit) - combinedBounds.min.x,
                        (sr.bounds.min.y * sr.sprite.pixelsPerUnit) - combinedBounds.min.y
                    );

                    spriteTextureData.Add(new SpriteTextureData
                    (
                        sr.sprite.texture,
                        texPos,
                        sr.transform.localScale,
                        -sr.transform.localEulerAngles.z,
                        sr.sortingLayerID + sr.sortingOrder
                    ));
                }
                else // Don't add sprite if it has a mismatching resolution
                {
                    Debug.LogWarning($"SpriteCombiner: Sprite '{sr.sprite.name}''s pixels per unit does not match target. ({sr.sprite.pixelsPerUnit} to target {settings.pixelsPerUnit})");
                }
            }

            return GetCombinedSprite
            (
                spriteTextureData.ToArray(),
                combinedRect,
                settings
            );
        }

        /// <summary>
        /// Private method that handles final creation of the combined sprite from sprite texture data
        /// </summary>
        /// <param name="spriteTextureData">The collection of sprite texture data that will drive creation of the sprite</param>
        /// <param name="combinedRect">The final combined rect that the sprite will be written into</param>
        /// <param name="settings">Sprite combiner settings to influence the created sprite</param>
        /// <returns>Sprite The combined sprite result</returns>
        private static Sprite GetCombinedSprite(SpriteTextureData[] spriteTextureData, Rect combinedRect, SpriteCombinerSettings settings)
        {
            if (settings == null) { return null; }

            // Distilling texture data collection from sprite texture data collection
            List<TextureData> textureData = new List<TextureData>(spriteTextureData.Length);
            for (int i = 0; i < spriteTextureData.Length; i++)
            {
                TextureData td = spriteTextureData[i].textureData;
                if (td == null) { continue; }

                if (td.texture == null)
                {
                    Debug.LogWarning("SpriteCombiner: TextureData texture is null!");
                }
                else if (!td.texture.isReadable)
                {
                    Debug.LogWarning($"SpriteCombiner: Texture '{td.texture.name}' is not Read/Write enabled! Please enable this option in the texture import settings.");
                }
                else
                {
                    textureData.Add(td);
                }
            }

            // Getting the combined texture to use in the combined sprite from the texture combiner
            Texture2D combinedTexture = TextureCombiner.CombineTextures
            (
                textureData.ToArray(),
                (int)combinedRect.width,
                (int)combinedRect.height,
                settings.textureCombinerSettings
            );

            return Sprite.Create
            (
                combinedTexture,
                combinedRect,
                settings.pivotPoint,
                settings.pixelsPerUnit
            );
        }
    }
}
