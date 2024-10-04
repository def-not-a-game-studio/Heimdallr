using _3rdparty.unityro_sdk.Core.Effects;
using Core.Effects;
using Core.Effects.EffectParts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Heimdallr
{
    public class EffectSpawner : MonoBehaviour
    {

        private EffectCache _effectCache;
        private StrEffectRenderer[] _strEffectRenderers;
        
        private void Awake()
        {
            _effectCache = FindAnyObjectByType<EffectCache>();
            _strEffectRenderers = FindObjectsByType<StrEffectRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        }

        public void SpawnEffect()
        {
            SpawnEffectAsync().Forget();
        }

        private async UniTaskVoid SpawnEffectAsync()
        {
            var renderInfo = await _effectCache.GetRenderInfo((int)EffectId.EF_BLESSING);
            foreach (var renderer in _strEffectRenderers)
            {
                renderer.Initialize(renderInfo);
            }
        }
    }
}