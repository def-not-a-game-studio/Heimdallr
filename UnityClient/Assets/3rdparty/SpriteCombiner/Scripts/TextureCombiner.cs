using System;
using UnityEngine;

namespace SpriteCombiner
{
    /// <summary>
    /// Main class for creating the combined texture that will be used by a combined sprite
    /// </summary>
    public static class TextureCombiner
    {
        /// <summary>
        /// Method to get a combined texture created from component textures
        /// </summary>
        /// <param name="textureData">The collection of texture data objects to use in texture creation</param>
        /// <param name="combinedWidth">The combined total width of the final combined texture</param>
        /// <param name="combinedHeight">The combined total height of the final combined texture</param>
        /// <param name="settings">The settings used to influence final texture creation</param>
        /// <returns>Texture2D The combined texture</returns>
        public static Texture2D CombineTextures(TextureData[] textureData, int combinedWidth, int combinedHeight, TextureCombinerSettings settings)
        {
            if (settings == null) { return null; }

            Texture2D combinedTexture = new Texture2D(combinedWidth, combinedHeight, settings.textureFormat, false)
            {
                alphaIsTransparency = settings.alphaIsTransparency,
                filterMode = settings.filterMode,
                wrapMode = settings.wrapMode,
            };

            Color32 baseCol = settings.alphaIsTransparency ? Color.clear : Color.black;

            Color32[] fillCols = new Color32[combinedTexture.width * combinedTexture.height];
            Array.Fill(fillCols, baseCol);
            combinedTexture.SetPixels32(fillCols);

            for (int i = 0; i < textureData.Length; i++)
            {
                TextureData td = textureData[i];
                if (td == null || td.texture == null) { continue; }

                Texture2D tex = td.texture;

                if (td.textureScale != Vector2.one) // Scale texture if applicable
                    tex = GetScaledTexture(tex, td.textureScale);

                if (td.textureRotation != 0) // Rotate texture if applicable
                    tex = GetRotatedTexture(tex, td.textureRotation);

                Vector2 texturePos = td.texturePosition;
                Color[] pixels = tex.GetPixels();

                for (int y = 0; y < tex.height; y++)
                {
                    for (int x = 0; x < tex.width; x++)
                    {
                        Color texCol = pixels[(y * tex.width) + x];

                        if (!settings.alphaIsTransparency && texCol.a > 0)
                            texCol.a = 1;

                        int
                            xIndex = Mathf.RoundToInt(x + texturePos.x),
                            yIndex = Mathf.RoundToInt(y + texturePos.y);

                        Color col = combinedTexture.GetPixel(xIndex, yIndex);

                        if (settings.alphaBlend && texCol.a < col.a) // Alpha colour blending
                        {
                            texCol = new Color
                            (
                                (col.r * (1 - texCol.a)) + (texCol.r * texCol.a),
                                (col.g * (1 - texCol.a)) + (texCol.g * texCol.a),
                                (col.b * (1 - texCol.a)) + (texCol.b * texCol.a),
                                col.a
                            );
                        }

                        if (texCol.a > settings.alphaClipThreshold && texCol.a >= col.a)
                            combinedTexture.SetPixel(xIndex, yIndex, texCol);
                    }
                }
            }
            combinedTexture.Apply();

            return combinedTexture;
        }

        /// <summary>
        /// Local function to rotate the given texture by the given angle
        /// </summary>
        /// <param name="tex">The texture to rotate</param>
        /// <param name="angle">The angle in degrees to rotate the texture</param>
        /// <returns>Texture2D The rotated texture</returns>
        static Texture2D GetRotatedTexture(Texture2D tex, float angle)
        {
            float diagonal = Mathf.Sqrt((tex.width * tex.width) + (tex.height * tex.height));
            int
                rotatedWidth = Mathf.CeilToInt(diagonal),
                rotatedHeight = Mathf.CeilToInt(diagonal);

            Texture2D rotatedTexture = new Texture2D(rotatedWidth, rotatedHeight, tex.format, false)
            {
                alphaIsTransparency = tex.alphaIsTransparency,
                filterMode = tex.filterMode,
                wrapMode = tex.wrapMode,
            };

            Vector2 center = new Vector2(tex.width / 2f, tex.height / 2f);
            for (int x = 0; x < rotatedWidth; x++)
            {
                for (int y = 0; y < rotatedHeight; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    pos -= center;
                    pos = Quaternion.Euler(0, 0, angle) * pos;
                    pos += center;

                    int
                        oldX = Mathf.RoundToInt(pos.x),
                        oldY = Mathf.RoundToInt(pos.y);

                    Color col;
                    if (oldX < 0 || oldX >= tex.width || oldY < 0 || oldY >= tex.height)
                        col = Color.clear;
                    else
                        col = tex.GetPixel(oldX, oldY);

                    rotatedTexture.SetPixel(x, y, col);
                }
            }
            rotatedTexture.Apply();

            return rotatedTexture;
        }

        /// <summary>
        /// Local function to scale the given texture by the given dimensions
        /// </summary>
        /// <param name="tex">The texture to scale</param>
        /// <param name="scale">The dimensions to scale the texture in</param>
        /// <returns>Texture2D The scaled texture</returns>
        static Texture2D GetScaledTexture(Texture2D tex, Vector2 scale)
        {
            int
                scaledWidth = Mathf.RoundToInt(tex.width * scale.x),
                scaledHeight = Mathf.RoundToInt(tex.height * scale.y);

            Texture2D scaledTex = new Texture2D(scaledWidth, scaledHeight, tex.format, false)
            {
                alphaIsTransparency = tex.alphaIsTransparency,
                filterMode = tex.filterMode,
                wrapMode = tex.wrapMode,
            };

            for (int x = 0; x < scaledWidth; x++)
            {
                for (int y = 0; y < scaledHeight; y++)
                {
                    int
                        xIndex = Mathf.RoundToInt(x / scale.x),
                        yIndex = Mathf.RoundToInt(y / scale.y);

                    Color col = tex.GetPixel(xIndex, yIndex);
                    scaledTex.SetPixel(x, y, col);
                }
            }
            scaledTex.Apply();

            return scaledTex;
        }
    }
}
