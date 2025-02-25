using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

namespace Heimdallr.Core.Game.Sprite
{
    public partial class SpriteGameEntity
    {
        
        private StatusWindowSourceScriptableObject _statusSO;

        private async void InitUi()
        {
            var canvas = GameObject.FindWithTag("WorldUI");
            var nameplatePrefab = await Resources.LoadAsync("UI/World/NamePlate") as GameObject;
            _namePlateAsset = Instantiate(nameplatePrefab, canvas.transform);
            _namePlateAsset.transform.position = transform.position;
            _namePlateAsset.transform.localScale = Vector3.one / 100f;

            if (HasAuthority())
            {
                _statusSO = await Resources.LoadAsync<StatusWindowSourceScriptableObject>("UI/Overlay/Bindings/StatusWindowSource") as StatusWindowSourceScriptableObject;
                
                var root = FindAnyObjectByType<UIDocument>().rootVisualElement;
                var statusWindow = root.Q<TemplateContainer>("StatusWindow").Q<VisualElement>("StatusWindowRoot");
                statusWindow.dataSource = _statusSO;
            }
        }
        
        private void UpdateStatusUI()
        {
            _statusSO.Str = $"{_status.Str} + {_status.NeedStr}";
            _statusSO.Agi = $"{_status.Agi} + {_status.NeedAgi}";
            _statusSO.Vit = $"{_status.Vit} + {_status.NeedVit}";
            _statusSO.Int = $"{_status.Int} + {_status.NeedInt}";
            _statusSO.Dex = $"{_status.Dex} + {_status.NeedDex}";
            _statusSO.Luk = $"{_status.Luk} + {_status.NeedLuk}";

            _statusSO.Atk = $"{_status.Atk} - {_status.Atk2}";
            _statusSO.Def = $"{_status.Def} - {_status.Def2}";
            _statusSO.Matk = $"{_status.MatkMin} - {_status.MatkMax}";
            _statusSO.Mdef = $"{_status.Mdef} - {_status.Mdef2}";
            _statusSO.Hit = $"{_status.Hit}";
            _statusSO.Flee = $"{_status.Flee} - {_status.Flee2}";
            _statusSO.Critical = $"{_status.Crit}";
            _statusSO.Aspd = $"{_status.Aspd} - {_status.Aspd2}";
            _statusSO.StatusPoints = _status.StatusPoints;
        }
    }
}