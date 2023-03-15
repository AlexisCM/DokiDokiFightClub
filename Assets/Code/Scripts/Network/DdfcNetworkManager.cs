using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DokiDokiFightClub
{
    [AddComponentMenu("")]
    public class DdfcNetworkManager : NetworkManager
    {
        [Header("DDFC Settings")]
        public GameObject InGamePlayerPrefab; // Prefab to load when player spawns in game scene

        [Header("MultiScene Setup")]
        public int MatchInstances = 2; // Number of simultaneous match instances allowed

        [Scene]
        public string GameScene; // Name of game scene

        [Scene]
        public string UiScene; // Name of scene which holds UI

        // This is set true after server loads all subscene instances
        bool _subscenesLoaded;

        // subscenes are added to this list as they're loaded
        readonly List<Scene> _subScenes = new();

        // Sequential index used in round-robin deployment of players into instances and score positioning
        int _clientIndex;

        /// <summary>
        /// Called by the MatchMaker when two players are ready to be put into a match.
        /// </summary>
        /// <param name="matchPlayers"></param>
        public void AddPlayersToMatch(List<PlayerQueueIdentity> matchPlayers)
        {
            foreach (var player in matchPlayers)
            {
                StartCoroutine(OnAddPlayersToMatch(player.connectionToClient));
            }
        }

        /// <summary>
        /// Replace prefab corresponding to the player's connection.
        /// </summary>
        /// <param name="conn"></param>
        public void ReplacePlayerPrefab(NetworkConnectionToClient conn)
        {
            // Cache a reference to the current player object
            GameObject oldPlayer = conn.identity.gameObject;

            // Instantiate the new player object and broadcast to clients
            // Include true for keepAuthority paramater to prevent ownership change
            NetworkServer.ReplacePlayerForConnection(conn, Instantiate(InGamePlayerPrefab), true);

            // Remove the previous player object that's now been replaced
            // Delay is required to allow replacement to complete.
            Destroy(oldPlayer, 0.1f);
        }

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            StartCoroutine(OnServerAddPlayerDelayed(conn));
        }

        /// <summary>
        /// Delay adding the player by a frame until the additive UI scene is loaded.
        /// Begin matchmaking once player finishes connecting.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
        {
            // wait for server to async load all subscenes for game instances
            while (!_subscenesLoaded)
                yield return null;

            // Send Scene message to client to additively load the game scene
            conn.Send(new SceneMessage { sceneName = UiScene, sceneOperation = SceneOperation.LoadAdditive });

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            base.OnServerAddPlayer(conn);
            MatchMaker.Instance.AddPlayerToQueue(conn);

        }

        /// <summary>
        /// Additively load the game scene and move the player's prefab to spawn within it.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerator OnAddPlayersToMatch(NetworkConnectionToClient conn)
        {
            // wait for server to async load all subscenes for game instances
            while (!_subscenesLoaded)
                yield return null;

            ReplacePlayerPrefab(conn);

            // Send Scene message to client to additively load the game scene
            conn.Send(new SceneMessage { sceneName = GameScene, sceneOperation = SceneOperation.LoadAdditive });

            // Wait for end of frame before adding the player to ensure Scene Message goes first
            yield return new WaitForEndOfFrame();

            conn.Send(new SceneMessage { sceneName = UiScene, sceneOperation = SceneOperation.UnloadAdditive });

            PlayerNetworkData playerNetData = conn.identity.GetComponent<PlayerNetworkData>();
            playerNetData.playerNumber = _clientIndex;
            playerNetData.scoreIndex = _clientIndex / _subScenes.Count;
            playerNetData.matchIndex = _clientIndex % _subScenes.Count;

            // Do this only on server, not on clients
            // This is what allows the NetworkSceneChecker on player and scene objects
            // to isolate matches per scene instance on server.
            if (_subScenes.Count > 0)
                SceneManager.MoveGameObjectToScene(conn.identity.gameObject, _subScenes[_clientIndex % _subScenes.Count]);

            _clientIndex++;
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            MatchMaker.Instance.RemovePlayerFromQueue(conn.identity.GetComponent<PlayerQueueIdentity>());
            base.OnServerDisconnect(conn);
        }

        #endregion

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer()
        {
            StartCoroutine(ServerLoadSubScenes());
        }

        // We're additively loading scenes, so GetSceneAt(0) will return the main "container" scene,
        // therefore we start the index at one and loop through instances value inclusively.
        // If instances is zero, the loop is bypassed entirely.
        IEnumerator ServerLoadSubScenes()
        {
            // Wait for scene to properly transition from OfflineScene to RoomScene
            yield return new WaitForEndOfFrame();

            for (int index = 1; index <= MatchInstances; index++)
            {
                yield return SceneManager.LoadSceneAsync(GameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });

                Scene newScene = SceneManager.GetSceneAt(index);
                _subScenes.Add(newScene);
                // Spawn interactable objects here; ie., doors, powerups, etc.
            }

            _subscenesLoaded = true;
        }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
            NetworkServer.SendToAll(new SceneMessage { sceneName = GameScene, sceneOperation = SceneOperation.UnloadAdditive });
            StartCoroutine(ServerUnloadSubScenes());
            _clientIndex = 0;
        }

        // Unload the subScenes and unused assets and clear the subScenes list.
        IEnumerator ServerUnloadSubScenes()
        {
            for (int index = 0; index < _subScenes.Count; index++)
                yield return SceneManager.UnloadSceneAsync(_subScenes[index]);

            _subScenes.Clear();
            _subscenesLoaded = false;

            yield return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            // make sure we're not in host mode
            if (mode == NetworkManagerMode.ClientOnly)
            {
                StartCoroutine(ClientUnloadSubScenes());
            }
        }

        // Unload all but the active scene, which is the "container" scene
        IEnumerator ClientUnloadSubScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
            }
        }

        #endregion
    }
}