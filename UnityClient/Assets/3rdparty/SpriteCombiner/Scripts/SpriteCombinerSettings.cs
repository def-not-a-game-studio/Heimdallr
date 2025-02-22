using System;
using UnityEngine;

namespace SpriteCombiner
{
    [Serializable]
    /// <summary>
    /// Small serializable class representing settings to influence final sprite creation
    /// </summary>
    public class SpriteCombinerSettings
    {
        [SerializeField] public TextureCombinerSettings textureCombinerSettings;
        [SerializeField] public Vector2 pivotPoint = new Vector2(0.5f, 0.5f);
        [SerializeField] public int pixelsPerUnit = 16;
    }
}
