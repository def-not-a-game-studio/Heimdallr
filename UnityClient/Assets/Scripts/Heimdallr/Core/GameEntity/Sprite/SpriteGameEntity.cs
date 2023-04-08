namespace Heimdallr.Core.Game.Sprite {
    public class SpriteGameEntity : CoreSpriteGameEntity {

        public override int Direction { get; }
        public override int HeadDirection { get; }
        public override bool IsMonster { get; }

        private GameEntityBaseStatus _Status;

        public override GameEntityBaseStatus Status => _Status;

        public override void ChangeMotion(MotionRequest request) {
            throw new System.NotImplementedException();
        }

        public override void Init(GameEntityBaseStatus gameEntityBaseStatus) {
            _Status = gameEntityBaseStatus;
        }
    }
}