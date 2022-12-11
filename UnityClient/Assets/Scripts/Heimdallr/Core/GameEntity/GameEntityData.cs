using System;

namespace Heimdallr.Core.Game {

    public class GameEntityData {
        #region Style
        public int HairColor;
        public int ClothesColor;
        public int HairStyle;
        public int Job;
        public bool IsMale;
        public int Eye;
        public int EyeColor;
        #endregion

        public EntityType EntityType;
        public int MoveSpeed;
        public int AttackSpeed;
    }
}
