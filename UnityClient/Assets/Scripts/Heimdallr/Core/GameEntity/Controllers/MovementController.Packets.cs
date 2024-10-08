using Core.Path;
using UnityEngine;

namespace Heimdallr.Core.Game.Controllers
{
    public partial class GameEntityMovementController
    {
        private void HookPackets()
        {
            if (Entity.HasAuthority())
            {
                NetworkClient.HookPacket<ZC.NOTIFY_PLAYERMOVE>(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            }
            else
            {
                NetworkClient.HookPacket<ZC.NOTIFY_MOVE>(ZC.NOTIFY_MOVE.HEADER, OnEntityMovement);
            }

            NetworkClient.HookPacket<ZC.STOPMOVE>(ZC.STOPMOVE.HEADER, OnEntityStop);
            NetworkClient.HookPacket<ZC.NPCACK_MAPMOVE>(ZC.NPCACK_MAPMOVE.HEADER, OnEntityMoved);
        }

        private void UnhookPackets()
        {
            if (Entity.HasAuthority())
            {
                NetworkClient.UnhookPacket<ZC.NOTIFY_PLAYERMOVE>(ZC.NOTIFY_PLAYERMOVE.HEADER, OnPlayerMovement); //Our movement
            }
            else
            {
                NetworkClient.UnhookPacket<ZC.NOTIFY_MOVE>(ZC.NOTIFY_MOVE.HEADER, OnEntityMovement);
            }

            NetworkClient.UnhookPacket<ZC.STOPMOVE>(ZC.STOPMOVE.HEADER, OnEntityStop);
        }

        private void OnEntityMovement(ushort cmd, int size, ZC.NOTIFY_MOVE packet)
        {
            if (packet.AID != Entity.Status.AID) return;
            StartMoving(packet.StartPosition[0], packet.StartPosition[1], packet.EndPosition[0], packet.EndPosition[1], GameManager.Tick);
        }

        private void OnPlayerMovement(ushort cmd, int size, ZC.NOTIFY_PLAYERMOVE packet)
        {
            StartMoving(packet.StartPosition[0], packet.StartPosition[1], packet.EndPosition[0], packet.EndPosition[1], GameManager.Tick);
        }

        private void OnEntityStop(ushort cmd, int size, ZC.STOPMOVE packet)
        {
            if (packet.AID != Entity.Status.AID || packet.AID != Entity.Status.GID) return;
            StartMoving((int)transform.position.x, (int)transform.position.z, packet.PosX, packet.PosY, GameManager.Tick);
        }

        private void OnEntityMoved(ushort cmd, int size, ZC.NPCACK_MAPMOVE packet)
        {
            StopMoving();

            PathFinder ??= FindObjectOfType<PathFinder>();

            var position = new Vector3(packet.PosX, PathFinder.GetCellHeight(packet.PosX, packet.PosY), packet.PosY);
            transform.position = position;

            new CZ.NOTIFY_ACTORINIT().Send();
        }
    }
}