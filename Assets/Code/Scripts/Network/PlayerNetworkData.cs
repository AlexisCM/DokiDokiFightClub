using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DokiDokiFightClub
{
    // TODO: rename to PlayerNetworkConnection?
    public class PlayerNetworkData : NetworkBehaviour
    {
        [SyncVar]
        public int playerNumber;

        [SyncVar]
        public int scoreIndex;

        [SyncVar]
        public int matchIndex;

        [SyncVar]
        public uint score;

        public int clientMatchIndex = -1;

        void OnGUI()
        {
            if (!isServerOnly && !isLocalPlayer && clientMatchIndex < 0)
                clientMatchIndex = NetworkClient.connection.identity.GetComponent<PlayerNetworkData>().matchIndex;

            if (isLocalPlayer || matchIndex == clientMatchIndex)
            {
                GUI.Box(new Rect(10f + (scoreIndex * 110), 10f, 100f, 25f), $"P{playerNumber}: {score}");
                var canvases = FindObjectsOfType<Canvas>();

                foreach (var canvas in canvases)
                {
                    if (canvas.gameObject.CompareTag("LobbyUI"))
                    {
                        canvas.gameObject.SetActive(false);
                    }
                }
                //var canvasObj = GameObject.FindGameObjectWithTag("LobbyUI");
                //TargetDisableLobbyUI(canvasObj);
            }
        }

        [TargetRpc]
        void TargetDisableLobbyUI(GameObject canvasObj)
        {
            Debug.Log("attempting to disable lobby ui");
            Scene lobbyScene = SceneManager.GetSceneByName("RoomScene");
            var rootObjects = lobbyScene.GetRootGameObjects();
            
            foreach (var rootObj in rootObjects)
            {
                if (rootObj.CompareTag("LobbyUI"))
                    canvasObj.SetActive(false);
            }
        }
    }
}
