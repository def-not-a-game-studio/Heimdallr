using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _3rdparty.unityro_sdk.Core.Effects;
using Core.Effects;
using Core.Effects.EffectParts;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityRO.Core.Database;
using UnityRO.Core.Database.Emotion;
using Random = UnityEngine.Random;

namespace Heimdallr
{
    public class EffectSpawner : MonoBehaviour
    {
        [SerializeField] private Effect effect;

        private EffectCache _effectCache;
        private List<EffectRenderer> _strEffectRenderers;
        private DatabaseManager _databaseManager;

        private void Awake()
        {
            _databaseManager = FindAnyObjectByType<DatabaseManager>();
            _effectCache = FindAnyObjectByType<EffectCache>();
            _strEffectRenderers =
                FindObjectsByType<EffectRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).ToList();
        }

        public void SpawnEffect()
        {
            _strEffectRenderers =
                FindObjectsByType<EffectRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).ToList();
            SpawnEffectAsync();
        }

        public void SpawnEmote()
        {
            _strEffectRenderers =
                FindObjectsByType<EffectRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).ToList();
            SpawnEmoteAsync();
        }

        private async void SpawnEffectAsync()
        {
            var effs = new Effect[]
            {
                _effectCache.Effects[(int)EffectId.EF_CONCENTRATION],
                _effectCache.Effects[(int)EffectId.EF_JOBLVUP],
                _effectCache.Effects[(int)EffectId.EF_MAGNIFICAT],
                _effectCache.Effects[(int)EffectId.EF_ANGELUS],
                _effectCache.Effects[(int)EffectId.EF_ANGEL],
            };
            var effIndex = 0;
            var count = 0;

            while (_strEffectRenderers.Count > 0)
            {
                var randomIndex = Random.Range(0, _strEffectRenderers.Count);
                var effectIndex = Random.Range(0, effs.Length);
                var renderer = _strEffectRenderers[randomIndex];
                var effect = effs[effectIndex];
                renderer.SetEffect(effect);
                await UniTask.Delay(Random.Range(0, 200));
                _strEffectRenderers.Remove(renderer);
            }

            return;
            foreach (var renderer in _strEffectRenderers)
            {
                if (effect == null)
                {
                    renderer.SetEffect(effs[effIndex]);
                    // renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_CONCENTRATION]);
                    // renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_JOBLVUP]);
                    // renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_MAGNIFICAT]);
                    // renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_ANGELUS]);
                    // renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_ANGEL]);
                }
                else
                {
                    renderer.SetEffect(effect);
                }
                
                count++;
                if (count % 15 == 0)
                {
                    effIndex = (effIndex + 1) % effs.Length;
                }
                await UniTask.Delay(Random.Range(0, 200));
            }
        }

        private async void SpawnEmoteAsync()
        {
            var emotions = Enum.GetValues(typeof(EmotionType)).Cast<EmotionType>().ToList();
            while (_strEffectRenderers.Count > 0)
            {
                var randomIndex = Random.Range(0, _strEffectRenderers.Count);
                var effectIndex = Random.Range(0, emotions.Count);
                
                var renderer = _strEffectRenderers[randomIndex];
                await renderer.SetEmotion(_databaseManager.GetEmotionIndex(emotions[effectIndex]));
                await UniTask.Delay(Random.Range(0, 200));
                _strEffectRenderers.Remove(renderer);
            }
        }

        private float count;
        private IEnumerator Start()
        {
            if (effect != null)
            {
                SpawnEffect();
            }

            GUI.depth = 2;
            while (true)
            {
                count = 1f / Time.unscaledDeltaTime;
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(5, 40, 100, 25), "FPS: " + Mathf.Round(count));
        }
    }
}