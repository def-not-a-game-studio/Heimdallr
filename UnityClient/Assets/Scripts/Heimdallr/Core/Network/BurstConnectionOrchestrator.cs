﻿using System;
using Core.Path;
using UnityEngine;
using UnityRO.Core.GameEntity;
using UnityRO.Net;

namespace Core.Network
{
    public class BurstConnectionOrchestrator : MonoBehaviour
    {
        private NetworkClient NetworkClient;
        private PathFinder PathFinder;
        private SessionManager SessionManager;
        private GameManager GameManager;

        private HC.NOTIFY_ZONESVR2 CurrentMapInfo;

        private int CharServerIndex;
        private int CharIndex;
        private string Username = "danilo";
        private string Password = "123456";
        private string Host;
        private string ForceMap;
        private CoreGameEntity PlayerEntity;

        private void Start()
        {
            NetworkClient = FindObjectOfType<NetworkClient>();
            PathFinder = FindObjectOfType<PathFinder>();
            SessionManager = FindObjectOfType<SessionManager>();
            GameManager = FindObjectOfType<GameManager>();

            NetworkClient.HookPacket<AC.ACCEPT_LOGIN3>(AC.ACCEPT_LOGIN3.HEADER, OnLoginResponse);
            NetworkClient.HookPacket<HC.ACCEPT_ENTER>(HC.ACCEPT_ENTER.HEADER, OnEnterResponse);
            NetworkClient.HookPacket<HC.ACCEPT_ENTER2>(HC.ACCEPT_ENTER2.HEADER, OnEnter2Response);
            NetworkClient.HookPacket<HC.NOTIFY_CHARLIST>(HC.NOTIFY_CHARLIST.HEADER, OnCharListResponse);
            NetworkClient.HookPacket<HC.NOTIFY_ZONESVR2>(HC.NOTIFY_ZONESVR2.HEADER, OnCharacterSelectionAccepted);
            NetworkClient.HookPacket<HC.ACCEPT_MAKECHAR>(HC.ACCEPT_MAKECHAR.HEADER, OnMakeCharAccepted);
            NetworkClient.HookPacket<ZC.ACCEPT_ENTER2>(ZC.ACCEPT_ENTER2.HEADER, OnMapServerLoginAccepted);
            // NetworkClient.HookPacket<HC.SECOND_PASSWD_LOGIN>(HC.SECOND_PASSWD_LOGIN.HEADER, (cmd, size, packet) =>
            // {
            //     SelectCharacter(0);
            // });

            Connect();
        }

        public void Init(
            int charServerIndex,
            int charIndex,
            string username,
            string password,
            string host,
            string forceMap,
            CoreGameEntity playerEntity
        )
        {
            CharServerIndex = charServerIndex;
            CharIndex = charIndex;
            Username = username;
            Password = password;
            Host = host;
            ForceMap = forceMap;
            PlayerEntity = playerEntity;
        }

        public void Connect()
        {
            TryConnectAndLogin(Host, Username, Password);
        }

        private void TryConnectAndLogin(string host, string username, string password)
        {
            //Debug.Log("Logging in");
            NetworkClient.Connect(host, 6900, NetworkClient.ServerType.Login);
            new CA.LOGIN(username, password, 10, 10).Send();
        }

        private void OnLoginResponse(ushort cmd, int size, AC.ACCEPT_LOGIN3 packet)
        {
            Debug.Log("Login response received");
            NetworkClient.State.LoginInfo = packet;
            NetworkClient.State.CharServer = packet.Servers[CharServerIndex];
            NetworkClient.SetAccountId(packet.AccountID);

            // If using docker for rA, this should point to same host as the login server
            // Docker sends the ips internally as 172 whatever
            ConnectToCharServer(packet, NetworkClient.State.CharServer);
        }

        private void ConnectToCharServer(
            AC.ACCEPT_LOGIN3 loginInfo,
            CharServerInfo charServerInfo
        )
        {
            Debug.Log("Connecting to char server");
            NetworkClient.Connect(Host, charServerInfo.Port, NetworkClient.ServerType.Char);
            new CH.ENTER(loginInfo.AccountID, loginInfo.LoginID1, loginInfo.LoginID2, loginInfo.Sex).Send();
        }
        
        private void OnEnter2Response(ushort cmd, int size, HC.ACCEPT_ENTER2 packet) {
            Debug.Log($"Accept enter 2");
        }

        private void OnEnterResponse(ushort cmd, int size, HC.ACCEPT_ENTER pkt)
        {
            Debug.Log("Char server response received");
            NetworkClient.State.CurrentCharactersInfo = pkt;

            // if no character available, create one
            if (pkt.Chars.Count == 0)
            {
                new CH.MAKE_CHAR2
                {
                    Name = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..8],
                    CharNum = 0
                }.Send();
            }
            else
            {
                NetworkClient.State.SelectedCharacter = pkt.Chars[CharIndex];
                SelectCharacter(CharIndex);
            }
        }

        private void SelectCharacter(int index)
        {
            Debug.Log("Selecting character");
            new CH.SELECT_CHAR(index).Send();
        }

        /**
         * The only situation we'll end up is when we don't have any character to begin with
         * So we create a new one and as we're orchestrating, just connect with it.
         */
        private void OnMakeCharAccepted(ushort cmd, int size, HC.ACCEPT_MAKECHAR ACCEPT_MAKECHAR)
        {
            Debug.Log("Char created");
            NetworkClient.State.SelectedCharacter = ACCEPT_MAKECHAR.characterData;

            SelectCharacter(0);
        }

        private void OnCharacterSelectionAccepted(ushort cmd, int size, HC.NOTIFY_ZONESVR2 currentMapInfo)
        {
            Debug.Log("Char selection accepted");
            CurrentMapInfo = currentMapInfo;

            NetworkClient.Connect(Host, currentMapInfo.Port, NetworkClient.ServerType.Zone);

            var loginInfo = NetworkClient.State.LoginInfo;
            new CZ.ENTER2(loginInfo.AccountID, NetworkClient.State.SelectedCharacter.GID, loginInfo.LoginID1,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(), loginInfo.Sex).Send();
        }

        private async void OnMapServerLoginAccepted(ushort cmd, int size, ZC.ACCEPT_ENTER2 pkt)
        {
            Debug.Log("Map server response received");
            NetworkClient.PausePacketHandling();

            var mapLoginInfo = new MapLoginInfo
            {
                mapname = CurrentMapInfo.Mapname.Split('.')[0],
                PosX = pkt.PosX,
                PosY = pkt.PosY,
                Dir = (int)((NpcDirection)pkt.Dir).ToDirection()
            };
            NetworkClient.State.MapLoginInfo = mapLoginInfo;
            GameManager.SetServerTick(pkt.Tick);

            PlayerEntity.Init(new GameEntityBaseStatus()
            {
                GID = NetworkClient.State.SelectedCharacter.GID,
                HairStyle = NetworkClient.State.SelectedCharacter.Head,
                IsMale = NetworkClient.State.SelectedCharacter.Sex == 1,
                HairColor = NetworkClient.State.SelectedCharacter
                    .HeadPalette,
                Job = NetworkClient.State.SelectedCharacter.Job,
                ClothesColor = NetworkClient.State.SelectedCharacter
                    .BodyPalette,
                MoveSpeed = NetworkClient.State.SelectedCharacter.Speed,
                EntityType = EntityType.PC,
                Name = NetworkClient.State.SelectedCharacter.Name,
                Weapon = NetworkClient.State.SelectedCharacter.Weapon,
                Shield = NetworkClient.State.SelectedCharacter.Shield,
                BaseLevel = NetworkClient.State.SelectedCharacter.Level,
                JobLevel = NetworkClient.State.SelectedCharacter.JobLevel,
            });

            SessionManager
                .StartSession(PlayerEntity,
                    NetworkClient.State.LoginInfo.AccountID);

            await SessionManager.SetCurrentMap(mapLoginInfo.mapname);
            PathFinder = FindObjectOfType<PathFinder>();

            PlayerEntity.transform
                .SetPositionAndRotation(new Vector3(pkt.PosX, PathFinder.GetCellHeight(pkt.PosX, pkt.PosY), pkt.PosY),
                    Quaternion.identity);
            PlayerEntity.ChangeDirection(Direction.North);
            NetworkClient.ResumePacketHandling();

            if (mapLoginInfo.mapname != ForceMap)
            {
                //new CZ.REQUEST_CHAT($"@warp {ForceMap} 150 150").Send();
            }
        }

        private void OnCharListResponse(ushort cmd, int size, HC.NOTIFY_CHARLIST packet)
        {
            //Debug.Log("Char list received");
        }

        public void SendCommand(string command)
        {
            new CZ.REQUEST_CHAT(SessionManager.CurrentSession.Entity.GetEntityName(), command).Send();
        }
    }
}