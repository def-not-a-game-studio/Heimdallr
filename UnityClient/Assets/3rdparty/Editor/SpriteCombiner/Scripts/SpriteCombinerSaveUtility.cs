using System;
using System.IO;
using UnityEngine;

namespace SpriteCombiner
{
    /// <summary>
    /// Simple class that decouples access to the file system when writing a texture
    /// </summary>
    public static class SpriteCombinerSaveUtility
    {
        /// <summary>
        /// Method to save a texture as a PNG file in the location of the user's choosing
        /// </summary>
        /// <param name="combinedTexture">The texture to save</param>
        /// <param name="path">The location to write the texture to</param>
        public static void SaveTextureAsset(Texture2D combinedTexture, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    byte[] bytes = combinedTexture.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                }
                catch (Exception e)
                {
                    Debug.LogError($"SpriteCombiner: Saving failed! {e}");
                }
            }
        }
    }
}
