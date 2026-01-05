using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    mapData,
    playerMovement,
    spawnBullet,
    playerHit,
    changeState,
    gameOver,
    lobbyInfo,
    shipData,
    modelData,
    sendMissile,
    killMissile,
    clientDisconnect,
}

public enum ClientToServerId : ushort
{
    name = 1,
    input,
    buttons,
    startGame,
    shipChoice,
}

public class NetworkManager : MonoBehaviour
{
    //THIS NETWORKMANAGER BELONGS TO THE SERVER

    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Id, out Player player))
            player.Leave(player);
            Destroy(player.gameObject);
    }
}
