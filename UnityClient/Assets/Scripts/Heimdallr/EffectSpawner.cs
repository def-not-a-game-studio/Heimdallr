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
            var renderInfo = await _effectCache.GetRenderInfo((int)EffectId.EF_ANGELUS);
            foreach (var renderer in _strEffectRenderers)
            {
                renderer.Initialize(renderInfo);
            }
        }
        
        private float count;
    
        private IEnumerator Start()
        {
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