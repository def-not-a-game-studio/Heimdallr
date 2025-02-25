using UnityEngine;
using UnityRO.Net;

namespace Heimdallr.Core.Game.Sprite
{
    public partial class SpriteGameEntity
    {
        private void OnLongParChange(ushort cmd, int size, ZC.LONGPAR_CHANGE packet)
        {
            HandleEntityStatus(packet.varID, packet.value);
        }

        private void OnLongParChange2(ushort cmd, int size, ZC.LONGPAR_CHANGE2 packet)
        {
            HandleEntityStatus(packet.varID, packet.value);
        }

        private void OnParChange(ushort cmd, int size, ZC.PAR_CHANGE packet)
        {
            HandleEntityStatus(packet.varID, packet.value);
        }

        private void OnStatus(ushort cmd, int size, ZC.STATUS packet)
        {
            _status.StatusPoints = packet.stpoint;
            _status.Str = packet.str;
            _status.Agi = packet.agi;
            _status.Vit = packet.vit;
            _status.Int = packet.inte;
            _status.Dex = packet.dex;
            _status.Luk = packet.luk;
            _status.NeedStr = packet.needStr;
            _status.NeedAgi = packet.needAgi;
            _status.NeedVit = packet.needVit;
            _status.NeedInt = packet.needInte;
            _status.NeedDex = packet.needDex;
            _status.NeedLuk = packet.needLuk;
            
            _status.Atk = packet.atk;
            _status.Atk2 = packet.atk2;
            _status.MatkMin = packet.matkMin;
            _status.MatkMax = packet.matkMax;
            _status.Def = packet.def;
            _status.Def2 = packet.def2;
            _status.Mdef = packet.mdef;
            _status.Mdef2 = packet.mdef2;
            _status.Hit = packet.hit;
            _status.Flee = packet.flee;
            _status.Flee2 = packet.flee2;
            _status.Crit = packet.crit;
            _status.Aspd = packet.aspd;
            _status.Aspd2 = packet.aspd2;
            
            UpdateStatusUI();
        }

        private void HandleEntityStatus(EntityStatus status, long value)
        {
         Debug.Log($"HandleEntityStatus {status}");   
        }
    }
}