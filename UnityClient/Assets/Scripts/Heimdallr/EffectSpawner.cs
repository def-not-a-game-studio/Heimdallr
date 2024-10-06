using System.Collections;
using _3rdparty.unityro_sdk.Core.Effects;
using Core.Effects;
using Core.Effects.EffectParts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Heimdallr
{
    public class EffectSpawner : MonoBehaviour
    {
        [SerializeField] private Effect effect;

        private EffectCache _effectCache;
        private EffectRenderer[] _strEffectRenderers;

        private void Awake()
        {
            _effectCache = FindAnyObjectByType<EffectCache>();
            _strEffectRenderers =
                FindObjectsByType<EffectRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        }

        public void SpawnEffect()
        {
            SpawnEffectAsync();
        }

        private void SpawnEffectAsync()
        {
            foreach (var renderer in _strEffectRenderers)
            {
                if (effect == null)
                {
                    renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_CONCENTRATION]);
                    renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_JOBLVUP]);
                    renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_MAGNIFICAT]);
                    renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_ANGELUS]);
                    renderer.SetEffect(_effectCache.Effects[(int)EffectId.EF_ANGEL]);
                }
                else
                {
                    renderer.SetEffect(effect);
                }
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