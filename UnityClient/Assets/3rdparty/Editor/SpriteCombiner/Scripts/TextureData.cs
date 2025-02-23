using System;
using UnityEngine;

namespace SpriteCombiner
{
    [Serializable]
    /// <summary>
    /// Data class representing various properties about a texture to be combined in code
    /// </summary>
    public sealed class TextureData
    {
        [SerializeField] public Texture2D texture;
        [SerializeField] public Vector2 texturePosition;
        [SerializeField] public Vector2 textureScale;
        [SerializeField] public float textureRotation;

        public TextureData(Texture2D _tex, Vector2 _texPos, Vector2 _texScale, float _texRot)
        {
            texture = _tex;
            texturePosition = _texPos;
            textureScale = _texScale;
            textureRotation = _texRot;
        }
    }
}
