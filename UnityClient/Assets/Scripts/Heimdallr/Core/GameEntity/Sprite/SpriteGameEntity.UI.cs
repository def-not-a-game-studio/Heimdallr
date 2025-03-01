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
            _statusSO.Name = _status.Name;
            _statusSO.JobName = $"{_status.Job}";
            
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
            _statusSO.Aspd = $"{(2000 - _status.Aspd) / 10}";
            _statusSO.StatusPoints = $"{_status.StatusPoints}";

            _statusSO.Patk = $"{_status.Patk}";
            _statusSO.Res = $"{_status.Res}";
            _statusSO.Smatk = $"{_status.Smatk}";
            _statusSO.Mres = $"{_status.Mres}";
            _statusSO.Hplus = $"{_status.Hplus}";
            _statusSO.Crate = $"{_status.Crate}";
            _statusSO.TraitPoints = $"{_status.TraitPoints}";

            _statusSO.Weight = $"{_status.Weight} / {_status.MaxWeight}";

            _statusSO.Hp = _status.HP;
            _statusSO.MaxHp = _status.MaxHP;
            _statusSO.Sp = _status.SP;
            _statusSO.MaxSp = _status.MaxSP;
            _statusSO.Money = _status.Money;

            _statusSO.BaseExp = _status.BaseExp;
            _statusSO.NextBaseExp = _status.NextBaseExp;
            _statusSO.JobExp = _status.JobExp;
            _statusSO.NextJobExp = _status.NextJobExp;

            _statusSO.BaseLevel = (short)_status.BaseLevel;
            _statusSO.JobLevel = (short)_status.JobLevel;

            _statusSO.CurrentHpPercent = $"{(int)((float)_statusSO.Hp / _status.MaxHP * 100)}%";
            _statusSO.CurrentSpPercent = $"{(int)((float)_statusSO.Sp / _status.MaxSP * 100)}%";
        }
    }
}