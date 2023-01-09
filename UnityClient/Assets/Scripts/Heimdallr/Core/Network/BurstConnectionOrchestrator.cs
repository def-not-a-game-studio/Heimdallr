using System;
using UnityEngine;

namespace Heimdallr.Core.Network {
    public class BurstConnectionOrchestrator : MonoBehaviour {

        private NetworkClient NetworkClient;

        private HC.NOTIFY_ZONESVR2 CurrentMapInfo;

        private int CharServerIndex = 0;
        private int CharIndex = 0;
        private string Username = "danilo";
        private string Password = "123456";
        private string Host;

        private void Start() {
            NetworkClient = FindObjectOfType<NetworkClient>();
        }

        public void Init(int charServerIndex, int charIndex, string username, string password, string host) {
            CharServerIndex = charServerIndex;
            CharIndex = charIndex;
            Username = username;
            Password = password;
            Host = host;

            NetworkClient.HookPacket(AC.ACCEPT_LOGIN3.HEADER, OnLoginResponse);
            NetworkClient.HookPacket(HC.ACCEPT_ENTER.HEADER, OnEnterResponse);
            NetworkClient.HookPacket(HC.NOTIFY_ZONESVR2.HEADER, OnCharacterSelectionAccepted);
            NetworkClient.HookPacket(HC.ACCEPT_MAKECHAR.HEADER, OnMakeCharAccepted);
            NetworkClient.HookPacket(ZC.ACCEPT_ENTER2.HEADER, OnMapServerLoginAccepted);

            Connect();
        }

        private void Connect() {
            TryConnectAndLogin(Host, Username, Password);
        }

        private async void TryConnectAndLogin(string host, string username, string password) {
            await NetworkClient.ChangeServer(host, 6900);
            new CA.LOGIN(username, password, 10, 10).Send();
        }

        private async void ConnectToCharServer(AC.ACCEPT_LOGIN3 loginInfo, string charIp, CharServerInfo charServerInfo) {
            await NetworkClient.ChangeServer(charIp, charServerInfo.Port);
            NetworkClient.SkipBytes(4);

            new CH.ENTER(loginInfo.AccountID, loginInfo.LoginID1, loginInfo.LoginID2, loginInfo.Sex).Send();
        }

        #region Packet Hooks
        private void OnLoginResponse(ushort cmd, int size, InPacket packet) {
            if(packet is AC.ACCEPT_LOGIN3 pkt) {
                NetworkClient.State.LoginInfo = pkt;
                NetworkClient.State.CharServer = pkt.Servers[CharServerIndex];
                /**
                 * If using docker for rA, this should point to same host as the login server
                 * Docker sends the ips internally as 172 whatever
                 */
                ConnectToCharServer(pkt, NetworkClient.State.CharServer.IP.ToString(), NetworkClient.State.CharServer);
            }
        }

        private void OnEnterResponse(ushort cmd, int size, InPacket packet) {
            if(packet is HC.ACCEPT_ENTER pkt) {
                NetworkClient.State.CurrentCharactersInfo = pkt;
                
                // if no character available, create one
                if (pkt.Chars.Count == 0) {
                    new CH.MAKE_CHAR2() {
                        Name = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8),
                        CharNum = 0
                    }.Send();
                } else {
                    new CH.SELECT_CHAR(CharIndex).Send();
                }
            }
        }

        private void OnMapServerLoginAccepted(ushort cmd, int size, InPacket packet) {
            if(packet is ZC.ACCEPT_ENTER2) {
                // Pausing because we need to change scenes and
                // have everything ready on the next scene
                NetworkClient.PausePacketHandling();

                var pkt = packet as ZC.ACCEPT_ENTER2;
                var mapLoginInfo = new MapLoginInfo() {
                    mapname = CurrentMapInfo.Mapname.Split('.')[0],
                    PosX = pkt.PosX,
                    PosY = pkt.PosY,
                    Dir = pkt.Dir
                };
                NetworkClient.State.MapLoginInfo = mapLoginInfo;
                Session.CurrentSession.SetCurrentMap(mapLoginInfo.mapname);
            }
        }

        /**
         * The only situation we'll end up is when we don't have any character to begin with 
         * So we create a new one and as we're orchestrating, just connect with it.
         */
        private void OnMakeCharAccepted(ushort cmd, int size, InPacket packet) {
            if(packet is HC.ACCEPT_MAKECHAR ACCEPT_MAKECHAR) {
                NetworkClient.State.SelectedCharacter = ACCEPT_MAKECHAR.characterData;

                new CH.SELECT_CHAR(0).Send();
            }
        }

        private async void OnCharacterSelectionAccepted(ushort cmd, int size, InPacket packet) {
            if(packet is HC.NOTIFY_ZONESVR2 currentMapInfo) {
                CurrentMapInfo = currentMapInfo;
                NetworkClient.Disconnect();

                await NetworkClient.ChangeServer(currentMapInfo.IP.ToString(), currentMapInfo.Port);
                NetworkClient.CurrentConnection.Start();

                // TODO implement this part
                //var entity = EntityManager.SpawnPlayer(NetworkClient.State.SelectedCharacter);
                //Session.StartSession(new Session(entity, NetworkClient.State.LoginInfo.AccountID));
                //DontDestroyOnLoad(entity.gameObject);

                var loginInfo = NetworkClient.State.LoginInfo;
                new CZ.ENTER2(loginInfo.AccountID, NetworkClient.State.SelectedCharacter.GID, loginInfo.LoginID1, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(), loginInfo.Sex).Send();
            }
        }
        #endregion
    }
}
