using Core.Path;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityRO.Core.Database;
using UnityRO.Core.Database.Emotion;
using UnityRO.Core.GameEntity;
using UnityRO.Core.Sprite;

namespace Heimdallr.Core.Game.Sprite
{
    public partial class SpriteGameEntity
    {
        public override float GetActionDelay(EntityActionRequest actionRequest)
        {
            switch (actionRequest.action)
            {
                case ActionRequestType.ATTACK_MULTIPLE_NOMOTION:
                case ActionRequestType.ATTACK_MULTIPLE:
                case ActionRequestType.ATTACK_NOMOTION:
                case ActionRequestType.ATTACK_REPEAT:
                case ActionRequestType.ATTACK_CRITICAL:
                case ActionRequestType.ATTACK_LUCKY:
                case ActionRequestType.ATTACK:
                    return SpriteViewer.GetAttackDelay();
                default:
                    return SpriteViewer.GetDelay();
            }
        }

        public override void RequestOffsetMovement(Vector2 destination)
        {
            var position = transform.position;
            MovementController.RequestMovement((int)(position.x + destination.x), (int)(position.z + destination.y));
        }

        public override void RequestMovement(Vector2 destination)
        {
            MovementController.RequestMovement((int)destination.x, (int)destination.y);
        }

        public override void Vanish(VanishType vanishType)
        {
            _state = EntityState.Vanish;
            switch (vanishType)
            {
                case VanishType.DIED:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Dead });

                    if (Status.EntityType != EntityType.PC)
                    {
                        EntityManager.UnlinkEntity((uint)Status.AID);
                        SpriteViewer.FadeOut(2f, 5f);
                        StartCoroutine(DestroyAfterSeconds(5));
                    }

                    break;
                case VanishType.OUT_OF_SIGHT:
                    if (EffectRenderer != null)
                    {
                        EffectRenderer.Vanish();
                        EffectRenderer = null;
                    }

                    EntityManager.UnlinkEntity((uint)Status.AID);
                    StartCoroutine(HideAfterSeconds(2f));
                    break;
                case VanishType.LOGGED_OUT:
                case VanishType.TELEPORT:
                    EntityManager.DestroyEntity((uint)Status.AID);
                    break;
            }
        }

        public override void ShowEmotion(byte emotionType)
        {
            var emotionIndex = DatabaseManager.GetEmotionIndex((EmotionType)emotionType);
            EffectRenderer.SetEmotion(emotionIndex).Forget();
        }

        public override void ChangeLook(LookType lookType, short packetValue, short packetValue2)
        {
            switch (lookType)
            {
                case LookType.LOOK_BASE:
                    _status.Job = packetValue;
                    var job = DatabaseManager.GetJobById(packetValue) as SpriteJob;
                    SpriteViewer.Init((_status.EntityType != EntityType.PC || _status.IsMale) ? job.Male : job.Female,
                        ViewerType.Body, this);
                    break;
                case LookType.LOOK_HAIR:
                    _status.HairStyle = packetValue;
                    var head = DatabaseManager.GetHeadById(packetValue);
                    SpriteViewer.FindChild(ViewerType.Head)
                        ?.Init((_status.EntityType != EntityType.PC || _status.IsMale) ? head.Male : head.Female,
                            ViewerType.Head, this);
                    break;
                case LookType.LOOK_CLOTHES_COLOR:
                    _status.ClothesColor = packetValue;
                    SpriteViewer.UpdatePalette();
                    break;
                case LookType.LOOK_HAIR_COLOR:
                    _status.HairColor = packetValue;
                    SpriteViewer.FindChild(ViewerType.Head)?.UpdatePalette();
                    break;
                default:
                    break;
            }
        }

        public override void SetAction(EntityActionRequest actionRequest, bool isSource, long delay = 0)
        {
            switch (actionRequest.action)
            {
                case ActionRequestType.SIT:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Sit });
                    break;
                case ActionRequestType.ATTACK_MULTIPLE_NOMOTION:
                case ActionRequestType.ATTACK_MULTIPLE:
                case ActionRequestType.ATTACK_NOMOTION:
                case ActionRequestType.ATTACK_REPEAT:
                case ActionRequestType.ATTACK_CRITICAL:
                case ActionRequestType.ATTACK_LUCKY:
                case ActionRequestType.ATTACK:
                    ProcessAttack(actionRequest, isSource, delay);
                    break;
                case ActionRequestType.ITEMPICKUP:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.PickUp });
                    break;
                case ActionRequestType.STAND:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Idle });
                    break;
                case ActionRequestType.SKILL:
                    ChangeMotion(new MotionRequest { Motion = SpriteMotion.Casting });
                    break;
                case ActionRequestType.SPLASH:
                    break;
                case ActionRequestType.TOUCHSKILL:
                    break;
                default:
                    break;
            }
        }

        public override void ChangeMotion(MotionRequest request)
        {
            var state = request.Motion switch
            {
                SpriteMotion.Idle => EntityState.Idle,
                SpriteMotion.Standby => EntityState.Standby,
                SpriteMotion.Walk => EntityState.Walk,
                SpriteMotion.Attack => EntityState.Attack,
                SpriteMotion.Attack1 => EntityState.Attack,
                SpriteMotion.Attack2 => EntityState.Attack,
                SpriteMotion.Attack3 => EntityState.Attack,
                SpriteMotion.Dead => EntityState.Dead,
                SpriteMotion.Hit => EntityState.Hit,
                SpriteMotion.Casting => EntityState.Cast,
                SpriteMotion.PickUp => EntityState.PickUp,
                SpriteMotion.Freeze1 => EntityState.Freeze,
                SpriteMotion.Freeze2 => EntityState.Freeze,
                SpriteMotion.Sit => EntityState.Sit,
                _ => EntityState.Idle
            };

            if (state == State && !request.forced)
            {
                return;
            }

            _state = state;
            SpriteViewer.ChangeMotion(request);
            MovementController.DelayMovement(request.startTime);
        }

        public override void LookTo(Vector3 position)
        {
            var offset = new Vector2Int((int)position.x, (int)position.z) -
                         new Vector2Int((int)transform.position.x, (int)transform.position.z);
            Direction = PathFinder.GetDirectionForOffset(offset);
            EntityDirection = Direction;
        }

        public override void ChangeDirection(Direction direction)
        {
            Direction = direction;
            EntityDirection = Direction;
        }

        private void StartMoving(int x, int y, int x1, int y2)
        {
            MovementController.StartMoving(x, y, x1, y2, GameManager.Tick);
        }

        public override void RequestAction(CoreGameEntity target)
        {
            var actionPacket = new CZ.REQUEST_ACT2
            {
                TargetID = (uint)target.GetEntityAID(),
                action = EntityActionType.CONTINUOUS_ATTACK
            };
            actionPacket.Send();
        }

        public override void TalkToNpc(CoreGameEntity target)
        {
            new CZ.CONTACTNPC
            {
                NAID = (uint)target.GetEntityAID(),
                Type = 1
            }.Send();
        }
    }
}