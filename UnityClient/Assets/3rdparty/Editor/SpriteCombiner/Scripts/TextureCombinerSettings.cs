using System;
using UnityEngine;

namespace SpriteCombiner
{
    [Serializable]
    /// <summary>
    /// Small serializable class representing settings to influence final texture creation
    /// </summary>
    public class TextureCombinerSettings
    {
        [SerializeField] public FilterMode filterMode = FilterMode.Point;
        [SerializeField] public TextureFormat textureFormat = TextureFormat.RGBA32;
        [SerializeField] public TextureWrapMode wrapMode  = TextureWrapMode.Clamp;
        [Range(0, 1f)][SerializeField] public float alphaClipThreshold;
        [SerializeField] public bool alphaBlend = true;
        [SerializeField] public bool alphaIsTransparency = true;
    }
}
