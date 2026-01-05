using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;

    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    public ushort gameState;
    public List<Profile> players;

    public ushort seed;
    public ushort objectCount;
    public ushort mapRadius;

    public GameObject[] bulletPrefabs;
    public Ship[] ships;

    public List<Missile> missiles;

    private bool reset;

    private void Awake()
    {
        missiles = new List<Missile>();
        Singleton = this;
        players = new List<Profile>();
    }

    private void Update()
    {
        List<Missile> remove = new List<Missile>();

        foreach (Missile currentMissile in missiles)
        {
            if (currentMissile.reference == null)
            {
                remove.Add(currentMissile);
            }
        }

        foreach (Missile currentMissile in remove)
        {
            missiles.Remove(currentMissile);
        }

        if (gameState == 1)
        {
            ushort alivePlayerCount = 0;
            ushort alivePlayerId = 0;
            foreach (Profile currentPlayer in players)
            {
                if (currentPlayer.state == 1)
                {
                    alivePlayerCount++;
                    alivePlayerId = currentPlayer.id;
                }
            }

            if (alivePlayerCount < 2 && players.Count > 1)
            {
                GameOver(alivePlayerId);
            }
        }

        if (gameState == 0)
        {
            SendPlayerData();
        }

        if (players.Count < 1)
        {
            gameState = 0;
        }
    }

    private void GenerateMap()
    {
        seed = (ushort)Random.Range(1, 10000000);
        objectCount = 150;
        mapRadius = 400;
        GetComponent<MapGen>().Generate(seed, objectCount, mapRadius);
    }

    private void GameOver(ushort alivePlayer)
    {
        ushort aliveShip = 0;
        foreach (Profile currentPlayer in players)
        {
            if (currentPlayer.id == alivePlayer)
            {
                aliveShip = currentPlayer.ship;
            }
        }

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.gameOver);
        message.AddUShort(alivePlayer);
        message.AddUShort(aliveShip);
        NetworkManager.Singleton.Server.SendToAll(message);

        if (!reset)
        {
            StartCoroutine(ResetGame());
        }
    }

    public IEnumerator ResetGame()
    {
        reset = true;
        yield return new WaitForSeconds(5);

        gameState = 0;

        foreach (Profile currentPlayer in players)
        {
            currentPlayer.state = 0;
        }

        reset = false;
    }

    public void StartGame()
    {
        gameState = 1;

        foreach (Profile currentPlayer in players)
        {
            currentPlayer.state = 1;
        }

        GenerateMap();
        SendMapData();
    }

    public void SendMapData()
    {
        foreach (Profile currentPlayer in players)
        {
            NetworkManager.Singleton.Server.Send(AddMapData(Message.Create(MessageSendMode.reliable, ServerToClientId.mapData), currentPlayer.id), currentPlayer.id);
        }
    }

    public void SendMapData(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddMapData(Message.Create(MessageSendMode.reliable, ServerToClientId.mapData), toClientId), toClientId);
    }

    private Message AddMapData(Message message, ushort toClient)
    {
        message.AddUShort(seed);
        message.AddUShort(objectCount);
        message.AddUShort(mapRadius);
        message.AddUShort(toClient);
        return message;
    }

    public void SendModelData(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.modelData);

        message.AddUShort((ushort)players.Count);

        foreach (Profile currentPlayer in players)
        {
            message.AddUShort(currentPlayer.id);
        }

        foreach (Profile currentPlayer in players)
        {
            message.AddUShort(currentPlayer.ship);
        }

        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void SendPlayerData()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.lobbyInfo);

        ushort host = 0;

        message.AddUShort((ushort)players.Count);
        foreach (Profile currentPlayer in players)
        {
            message.AddString(currentPlayer.name);
            if (host == 0 || currentPlayer.id < host)
            {
                host = currentPlayer.id;
            }
        }

        foreach (Profile currentPlayer in players)
        {
            message.AddUShort(currentPlayer.id);
        }

        foreach (Profile currentPlayer in players)
        {
            message.AddUShort(currentPlayer.ship);
        }

        message.AddUShort(host);

        NetworkManager.Singleton.Server.SendToAll(message);
    }
}

[System.Serializable]
public class Profile
{
    public string name;
    public ushort id;
    public ushort state;
    public ushort ship;

    public Profile(string _name, ushort _id, ushort _state, ushort _ship)
    {
        name = _name;
        id = _id;
        state = _state;
        ship = _ship;
    }
}

[System.Serializable]
public class Missile
{
    public GameObject reference;
    public ushort number;
    public ushort type;

    public Missile(GameObject _ref, ushort _num, ushort _type)
    {
        reference = _ref;
        number = _num;
        type = _type;
    }
}

[System.Serializable]
public class Ship
{
    public GameObject model;
    public Vector3 offset;

    public float speed;
    public float agility;
    public float health;
    public float battery;

    public float batteryRegen;

    public ushort shipAttack;

    public Ship(GameObject _model, Vector3 _offset, float _speed, float _agility, float _health, float _battery, float _batteryRegen, ushort _shipAttack)
    {
        model = _model;
        offset = _offset;
        speed = _speed;
        agility = _agility;
        health = _health;
        battery = _battery;
        shipAttack = _shipAttack;
        batteryRegen = _batteryRegen;
    }
}
