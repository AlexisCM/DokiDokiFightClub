using Mirror;
using UnityEngine;

public class AutoHostClient : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkMgr;

    void Start()
    {
        if (!Application.isBatchMode) // Headless Build
        {
            _networkMgr.StartClient();
            Debug.Log("Client Build");
        }
        else
        {
            Debug.Log("Server Build");
        }
    }

    public void JoinLocal()
    {
        _networkMgr.networkAddress = "localhost";
        _networkMgr.StartClient();
    }
}
