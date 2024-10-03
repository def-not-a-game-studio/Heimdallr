using System;
using System.Collections.Generic;
using _3rdparty.unityro_sdk.Core.Effects;
using Core.Effects;
using ROIO.Models.FileTypes;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Heimdallr
{
    public class EffectSpawner : MonoBehaviour
    {
        [SerializeField] private StrEffectRenderer renderer;

        private EffectCache _effectCache;

        private void Awake()
        {
            _effectCache = FindAnyObjectByType<EffectCache>();
        }

        public void SpawnEffect()
        {
            SpawnEffectAsync().Forget();
        }

        private async UniTaskVoid SpawnEffectAsync()
        {
            var effect = await Resources.LoadAsync<STR>("Effects/STR/magnificat") as STR;
            var renderInfo = _effectCache.GetRenderInfo(0);
            if (renderInfo != null)
            {
                renderer.Initialize(effect, 0, renderInfo);
            }
            else
            {
                renderer.Initialize(effect, 0);
            }
        }
    }
}