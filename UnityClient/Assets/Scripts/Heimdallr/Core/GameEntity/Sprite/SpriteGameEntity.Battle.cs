using UnityRO.Core.Sprite;

namespace Heimdallr.Core.Game.Sprite
{
    public partial class SpriteGameEntity
    {
        public override void SetAttackSpeed(ushort actionRequestSourceSpeed) {
            Status.AttackSpeed = actionRequestSourceSpeed;
        }

        public override void SetAttackedSpeed(ushort actionRequestTargetSpeed) {
            Status.AttackedSpeed = actionRequestTargetSpeed;
        }
        
        private void ProcessAttack(EntityActionRequest actionRequest, bool isSource, long delay) {
            if (isSource) ProcessAttacker(actionRequest);
            else ProcessAttacked(actionRequest, delay);
        }

        private void ProcessAttacked(EntityActionRequest actionRequest, long delay) {
            if (actionRequest.damage > 0 &&
                actionRequest.action is not (ActionRequestType.ATTACK_MULTIPLE_NOMOTION or ActionRequestType.ATTACK_NOMOTION)) {
                ChangeMotion(
                    new MotionRequest {
                        Motion = SpriteMotion.Hit,
                        forced = true,
                        startTime = delay
                    }
                );
            }
        }
        
        private void ProcessAttacker(EntityActionRequest actionRequest) {
            ChangeMotion(
                new MotionRequest { Motion = SpriteMotion.Attack, forced = true }
            );
        }
    }
}