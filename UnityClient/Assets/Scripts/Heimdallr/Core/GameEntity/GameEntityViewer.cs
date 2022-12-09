using Heimdallr.Core.Database.Eye;
using Heimdallr.Core.Database.Hair;
using Heimdallr.Core.Database.HeadFace;
using UnityEngine;

namespace Heimdallr.Core.Game {
    public class GameEntityViewer : MonoBehaviour {

        #region Components
        private GameEntity GameEntity;
        private Animator Animator;
        #endregion

        #region Bones
        [SerializeField]
        private Transform NeckBone;

        [SerializeField]
        private Transform EyeBone;

        [SerializeField]
        private Transform LeftHandBone;

        [SerializeField]
        private Transform RightHandBone;
        #endregion

        #region State
        public GameEntityData GameEntityData { get; private set; }
        public GameEntityCustomizableData GameEntityCustomizableData { get; private set; }
        private GameEntityState LastEntityState;

        private GameObject Eye;
        private GameObject Hair;
        private GameObject HeadFace;
        #endregion

        private void Awake() {
            GameEntity = GetComponentInParent<GameEntity>();
            Animator = GetComponent<Animator>();
        }

        private void Update() {
            if(LastEntityState != GameEntity.EntityState) {
                LastEntityState = GameEntity.EntityState;

                string animatorTrigger = "";
                string parameterName = null;
                float parameterValue = 1f;

                switch(LastEntityState) {
                    case GameEntityState.Wait:
                        animatorTrigger = "wait";
                        break;
                    case GameEntityState.Walk:
                        animatorTrigger = "walk";
                        parameterName = "walkSpeedMultiplier";
                        parameterValue = 0.75f;
                        break;
                    case GameEntityState.Attack:
                        animatorTrigger = "attack";
                        break;
                    case GameEntityState.Hit:
                        animatorTrigger = "hit";
                        break;
                };

                Animator.SetTrigger(animatorTrigger);
                if(parameterName != null) {
                    Animator.SetFloat(parameterName, parameterValue);
                }
            }
        }

        public void SetGameEntityData(GameEntityData data) {
            transform.localScale = new Vector3(2.3f, 2.3f, 2.3f);
            var oldData = GameEntityData;
            GameEntityData = data;

            if(oldData != GameEntityData) {
                UpdateCustomizableData(oldData ?? data);
            }
        }

        private void UpdateCustomizableData(GameEntityData data) {
            GameEntityCustomizableData ??= new GameEntityCustomizableData {
                Eye = DatabaseManager.GetEyeById(data.Eye),
                HeadFace = DatabaseManager.GetHeadFaceById(0),
                Hair = DatabaseManager.GetHairById(data.HairStyle)
            };

            if(data.HairStyle != GameEntityData.HairStyle || Hair == null) {
                SetHairStyle(data);
                SetHairColor(data);
            }

            if(data.HairColor != GameEntityData.HairColor && Hair != null) {
                SetHairColor(data);
            }

            if(data.Eye != GameEntityData.Eye || Eye == null) {
                if(Eye != null)
                    Destroy(Eye);

                Eye = Instantiate(data.IsMale ? GameEntityCustomizableData.Eye.EyeMale : GameEntityCustomizableData.Eye.EyeFemale, EyeBone);
            }

            if(HeadFace == null) {
                HeadFace = Instantiate(data.IsMale ? GameEntityCustomizableData.HeadFace.Male : GameEntityCustomizableData.HeadFace.Female, NeckBone);
            }
        }

        private void SetHairStyle(GameEntityData data) {
            if(Hair != null)
                Destroy(Hair);

            Hair = Instantiate(data.IsMale ? GameEntityCustomizableData.Hair.HairMale : GameEntityCustomizableData.Hair.HairFemale, NeckBone);
        }

        private void SetHairColor(GameEntityData data) {
            Renderer meshRenderer = Hair.gameObject.GetComponentInChildren<Renderer>();
            var colors = data.IsMale ? GameEntityCustomizableData.Hair.ColorsMale : GameEntityCustomizableData.Hair.ColorsFemale;
            if(data.HairColor < colors.Count) {
                meshRenderer.material = colors[data.HairColor];
            }
        }
    }

    public class GameEntityCustomizableData {
        public Eye Eye;
        public HeadFace HeadFace;
        public Hair Hair;
    }
}
