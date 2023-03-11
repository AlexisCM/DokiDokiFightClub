using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class UIManager : MonoBehaviour
    {
        //private NetworkManager _networkManager;
        private NetworkRoomManager _lobbyMgr;

        private void Start()
        {
            //_networkManager = FindObjectOfType<NetworkManager>();
            _lobbyMgr = FindObjectOfType<NetworkRoomManager>();
        }

        /// <summary>
        /// TESTING ONLY! Remove later.
        /// </summary>
        public void LaunchServer()
        {
            //_networkManager.StartServer();
            _lobbyMgr.StartServer();
        }

        public void OnPlayButton()
        {
            //_networkManager.StartClient();
            _lobbyMgr.StartClient();
        }

        public void Host()
        {
            NetworkPlayer.LocalPlayer.HostMatch();
        }

        public void Join()
        {
            NetworkPlayer.LocalPlayer.JoinMatch();
        }
    }
}
