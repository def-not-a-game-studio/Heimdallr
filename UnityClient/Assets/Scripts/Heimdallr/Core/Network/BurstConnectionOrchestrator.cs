using System;
using UnityEngine;

namespace Heimdallr.Core.Network {
    public class BurstConnectionOrchestrator : MonoBehaviour {

        private NetworkClient NetworkClient;
        private PathFinder PathFinder;

        private HC.NOTIFY_ZONESVR2 CurrentMapInfo;

        private int CharServerIndex;
        private int CharIndex;
        private string Username = "danilo";
        private string Password = "123456";
        private string Host;
        private string ForceMap;
        private CoreGameEntity PlayerEntity;

        private void Start() {
            NetworkClient = FindObjectOfType<NetworkClient>();
            PathFinder = FindObjectOfType<PathFinder>();

            NetworkClient.HookPacket(AC.ACCEPT_LOGIN3.HEADER, OnLoginResponse);
            NetworkClient.HookPacket(HC.ACCEPT_ENTER.HEADER, OnEnterResponse);
            NetworkClient.HookPacket(HC.NOTIFY_ZONESVR2.HEADER, OnCharacterSelectionAccepted);
            NetworkClient.HookPacket(HC.ACCEPT_MAKECHAR.HEADER, OnMakeCharAccepted);
            NetworkClient.HookPacket(ZC.ACCEPT_ENTER2.HEADER, OnMapServerLoginAccepted);
            NetworkClient.HookPacket(ZC.NPCACK_MAPMOVE.HEADER, OnEntityMoved);

            Connect();
        }

        public void Init(int charServerIndex, int charIndex, string username, string password, string host, string forceMap, CoreGameEntity playerEntity) {
            CharServerIndex = charServerIndex;
            CharIndex = charIndex;
            Username = username;
            Password = password;
            Host = host;
            ForceMap = forceMap;
            PlayerEntity = playerEntity;
        }

        private void Connect() {
            TryConnectAndLogin(Host, Username, Password);
        }

        private async void TryConnectAndLogin(string host, string username, string password) {
            Debug.Log("Logging in");
            await NetworkClient.ChangeServer(host, 6900);
            new CA.LOGIN(username, password, 10, 10).Send();
        }

        private async void ConnectToCharServer(AC.ACCEPT_LOGIN3 loginInfo, string charIp, CharServerInfo charServerInfo) {
            Debug.Log("Connecting to char server");
            await NetworkClient.ChangeServer(Host, charServerInfo.Port);
            NetworkClient.SkipBytes(4);

            new CH.ENTER(loginInfo.AccountID, loginInfo.LoginID1, loginInfo.LoginID2, loginInfo.Sex).Send();
        }

        private void SelectCharacter(int index) {
            Debug.Log("Selecting character");
            new CH.SELECT_CHAR(index).Send();
        }

        #region Packet Hooks
        private void OnLoginResponse(ushort cmd, int size, InPacket packet) {
            if (packet is not AC.ACCEPT_LOGIN3 pkt) return;
            Debug.Log("Login response received");
            NetworkClient.State.LoginInfo = pkt;
            NetworkClient.State.CharServer = pkt.Servers[CharServerIndex];
            
            // If using docker for rA, this should point to same host as the login server
            // Docker sends the ips internally as 172 whatever
            ConnectToCharServer(pkt, NetworkClient.State.CharServer.IP.ToString(), NetworkClient.State.CharServer);
        }

        private void OnEnterResponse(ushort cmd, int size, InPacket packet) {
            if (packet is not HC.ACCEPT_ENTER pkt) return;
            Debug.Log("Char server response received");
            NetworkClient.State.CurrentCharactersInfo = pkt;
                
            // if no character available, create one
            if (pkt.Chars.Count == 0) {
                new CH.MAKE_CHAR2 {
                                      Name = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8),
                                      CharNum = 0
                                  }.Send();
            } else {
                NetworkClient.State.SelectedCharacter = pkt.Chars[CharIndex];
                SelectCharacter(CharIndex);
            }
        }

        private void OnMapServerLoginAccepted(ushort cmd, int size, InPacket packet) {
            if (packet is not ZC.ACCEPT_ENTER2 pkt) return;
            Debug.Log("Map server response received");
            var mapLoginInfo = new MapLoginInfo {
                                                    mapname = CurrentMapInfo.Mapname.Split('.')[0],
                                                    PosX = pkt.PosX,
                                                    PosY = pkt.PosY,
                                                    Dir = pkt.Dir
                                                };
            NetworkClient.State.MapLoginInfo = mapLoginInfo;
            Session.CurrentSession.SetCurrentMap(mapLoginInfo.mapname);
                
            PlayerEntity.transform.SetPositionAndRotation(new Vector3(pkt.PosX, PathFinder.GetCellHeight(pkt.PosX, pkt.PosY), pkt.PosY), Quaternion.identity);

            if (mapLoginInfo.mapname != ForceMap) {
                new CZ.REQUEST_CHAT($"@warp {ForceMap} 150 150").Send();
            }
        }

        /**
         * The only situation we'll end up is when we don't have any character to begin with 
         * So we create a new one and as we're orchestrating, just connect with it.
         */
        private void OnMakeCharAccepted(ushort cmd, int size, InPacket packet) {
            if (packet is not HC.ACCEPT_MAKECHAR ACCEPT_MAKECHAR) return;
            Debug.Log("Char created");
            NetworkClient.State.SelectedCharacter = ACCEPT_MAKECHAR.characterData;

            SelectCharacter(0);
        }

        private async void OnCharacterSelectionAccepted(ushort cmd, int size, InPacket packet) {
            if (packet is not HC.NOTIFY_ZONESVR2 currentMapInfo) return;
            Debug.Log("Char selection accepted");
            CurrentMapInfo = currentMapInfo;

            await NetworkClient.ChangeServer(Host, currentMapInfo.Port);

            PlayerEntity.Init(new GameEntityBaseStatus {
                GID = NetworkClient.State.SelectedCharacter.GID,
                HairStyle = NetworkClient.State.SelectedCharacter.Head,
                IsMale = NetworkClient.State.SelectedCharacter.Sex == 0,
                HairColor = NetworkClient.State.SelectedCharacter.HeadPalette,
                Job = NetworkClient.State.SelectedCharacter.Job,
                ClothesColor = NetworkClient.State.SelectedCharacter.BodyPalette,
                MoveSpeed = NetworkClient.State.SelectedCharacter.Speed,
                EntityType = EntityType.PC,
                Name = NetworkClient.State.SelectedCharacter.Name,
            });

            Session.StartSession(new Session(new NetworkEntity(0, PlayerEntity.Status.GID, NetworkClient.State.SelectedCharacter.Name), NetworkClient.State.LoginInfo.AccountID));

            var loginInfo = NetworkClient.State.LoginInfo;
            new CZ.ENTER2(loginInfo.AccountID, NetworkClient.State.SelectedCharacter.GID, loginInfo.LoginID1, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(), loginInfo.Sex).Send();
        }

        private void OnEntityMoved(ushort cmd, int size, InPacket packet) {
            if (packet is not ZC.NPCACK_MAPMOVE pkt) return;
            PlayerEntity.transform.position = new Vector3(pkt.PosX, PathFinder.GetCellHeight(pkt.PosX, pkt.PosY), pkt.PosY);
            new CZ.NOTIFY_ACTORINIT().Send();
        }
        #endregion
    }
}
