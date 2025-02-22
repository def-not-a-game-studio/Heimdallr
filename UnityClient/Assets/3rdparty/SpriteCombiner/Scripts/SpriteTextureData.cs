using System;
using UnityEngine;

namespace SpriteCombiner
{
    [Serializable]
    /// <summary>
    /// Data class representing various properties about a sprite to be combined in code
    /// </summary>
    public sealed class SpriteTextureData
    {
        public Rect TextureRect
            => new Rect
            (
                textureData.texturePosition.x,
                textureData.texturePosition.y,
                textureData.texture.width * textureData.textureScale.x,
                textureData.texture.height * textureData.textureScale.y
            );
        [SerializeField] public TextureData textureData;
        [SerializeField] public int sortingOrder;

        public SpriteTextureData(Texture2D _tex, Vector2 _texPos, Vector2 _texScale, float _texRot, int _sortingOrder = 0)
        {
            textureData = new TextureData(_tex, _texPos, _texScale, _texRot);
            sortingOrder = _sortingOrder;
        }
    }
}
