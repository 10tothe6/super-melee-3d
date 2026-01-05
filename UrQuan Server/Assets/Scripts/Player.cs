using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //THIS PLAYER BELONGS TO THE SERVER

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }

    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;
    [SerializeField] private string version;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Awake()
    {
        version = "1.0.1";
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
        {
            otherPlayer.SendSpawned(id);
        }
           
        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        if (GameLogic.Singleton.gameState == 0)
        {
            NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players.Add(new Profile(username, id, 0, 0));
            player.SendSpawned();
        }
        else if (GameLogic.Singleton.gameState == 1)
        {
            NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players.Add(new Profile(username, id, 2, 0));
            player.SendSpawned();
            GameLogic.Singleton.SendMapData(id);
        }

        list.Add(id, player);

        foreach (Player currentPlayer in list.Values)
        {
            if (GameLogic.Singleton.gameState == 1)
            {
                GameLogic.Singleton.SendModelData(id);
            }
        }
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    public void Leave(Player leavingPlayer)
    {
        //remove an alive player or a dead one depending on the above result
        int destroy = -1;
        foreach (Profile currentPlayer in NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players)
        {
            if (currentPlayer.id == leavingPlayer.Id)
            {
                destroy = NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players.IndexOf(currentPlayer);
                break;
            }
        }
        NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players.Remove(NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players[destroy]);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    private void SendDisconnect(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.clientDisconnect);
        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    [MessageHandler((ushort)ClientToServerId.name)]

    private static void Name(ushort fromCliendId, Message message)
    {
        string username = message.GetString();
        string playerVersion = null;
        if (message.UnreadLength > 0)
        {
            playerVersion = message.GetString();
        }

        Spawn(fromCliendId, username);

        if (!string.IsNullOrEmpty(playerVersion))
        {
            if (list.TryGetValue(fromCliendId, out Player player))
            {
                if (playerVersion != player.version)
                {
                    Debug.Log(username + " tried to join, but their client has the wrong version.");
                    player.SendDisconnect(fromCliendId);
                }
            }  
        }
        else if(list.TryGetValue(fromCliendId, out Player player))
        {
            Debug.Log(username + " tried to join, but their client has the wrong version.");
            player.SendDisconnect(fromCliendId);
        }
    }

    [MessageHandler((ushort)ClientToServerId.input)]

    private static void Input(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.Movement.SetInput(message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.buttons)]

    private static void Buttons(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.Movement.HandleButtons(message.GetBool(), message.GetBool());
    }

    [MessageHandler((ushort)ClientToServerId.startGame)]

    private static void HandleStart(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            GameLogic.Singleton.StartGame();
    }

    [MessageHandler((ushort)ClientToServerId.shipChoice)]

    private static void HandleShipSelect(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
        {
            foreach (Profile currentPlayer in GameLogic.Singleton.players)
            {
                if (currentPlayer.id == fromClientId)
                {
                    currentPlayer.ship = message.GetUShort();
                }
            }
        }
            
    }
}
