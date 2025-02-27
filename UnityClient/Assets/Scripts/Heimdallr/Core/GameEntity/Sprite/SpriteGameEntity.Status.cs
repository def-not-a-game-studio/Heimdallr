using System;
using TMPro;
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
            switch (status)
            {
                case EntityStatus.SP_WEIGHT:
                    _status.Weight = value;
                    break;
                case EntityStatus.SP_MAXWEIGHT:
                    _status.MaxWeight = value;
                    break;
                case EntityStatus.SP_HIT:
                    _status.Hit = (short)value;
                    break;
                case EntityStatus.SP_FLEE1:
                    _status.Flee = (short)value;
                    break;
                case EntityStatus.SP_FLEE2:
                    _status.Flee2 = (short)value;
                    break;
                case EntityStatus.SP_ASPD:
                    _status.Aspd = (short)value;
                    break;
                case EntityStatus.SP_ATK1:
                    _status.Atk = (short)value;
                    break;
                case EntityStatus.SP_ATK2:
                    _status.Atk2 = (short)value;
                    break;
                case EntityStatus.SP_DEF1:
                    _status.Def = (short)value;
                    break;
                case EntityStatus.SP_DEF2:
                    _status.Def2 = (short)value;
                    break;
                case EntityStatus.SP_CRITICAL:
                    _status.Crit = (short)value;
                    break;
                case EntityStatus.SP_MATK1:
                    _status.MatkMin = (short)value;
                    break;
                case EntityStatus.SP_MATK2:
                    _status.MatkMax = (short)value;
                    break;
                case EntityStatus.SP_MDEF1:
                    _status.Mdef = (short)value;
                    break;
                case EntityStatus.SP_MDEF2:
                    _status.Mdef2 = (short)value;
                    break;
                case EntityStatus.SP_HP:
                    _status.HP = value;
                    break;
                case EntityStatus.SP_MAXHP:
                    _status.MaxHP = value;
                    break;
                case EntityStatus.SP_SP:
                    _status.SP = value;
                    break;
                case EntityStatus.SP_MAXSP:
                    _status.MaxSP = value;
                    break;
                case EntityStatus.SP_BASEEXP:
                    _status.BaseExp = value;
                    break;
                case EntityStatus.SP_NEXTBASEEXP:
                    _status.NextBaseExp = value;
                    break;
                case EntityStatus.SP_JOBEXP:
                    _status.JobExp = value;
                    break;
                case EntityStatus.SP_NEXTJOBEXP:
                    _status.NextJobExp = value;
                    break;
                case EntityStatus.SP_SKILLPOINT:
                    _status.SkillPoints = (short)value;
                    break;
                case EntityStatus.SP_PATK:
                    _status.Patk = (short)value;
                    break;
                case EntityStatus.SP_SMATK:
                    _status.Smatk = (short)value;
                    break;
                case EntityStatus.SP_RES:
                    _status.Res = (short)value;
                    break;
                case EntityStatus.SP_MRES:
                    _status.Mres = (short)value;
                    break;
                case EntityStatus.SP_HPLUS:
                    _status.Hplus = (short)value;
                    break;
                case EntityStatus.SP_CRATE:
                    _status.Crate = (short)value;
                    break;
                case EntityStatus.SP_TRAITPOINT:
                    _status.TraitPoints = (short)value;
                    break;
                case EntityStatus.SP_AP:
                    _status.Ap = (short)value;
                    break;
                case EntityStatus.SP_MAXAP:
                    _status.MaxAp = (short)value;
                    break;
                default:
                    Debug.Log($"HandleEntityStatus unknown status {status}");
                    break;
            }
            
            UpdateStatusUI();
        }
    }
}