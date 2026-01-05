using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //THIS PLAYER BELONGS TO THE CLIENT

    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    public string username;
    private int modelId;
    [SerializeField] private GameObject bulletPrefab;

    private Audio audioScript;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Awake()
    {
        audioScript = GameObject.Find("AudioManager").GetComponent<Audio>();
    }

    private void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;

        transform.forward = forward;
    }

    private void Update()
    {
        if (Id != NetworkManager.Singleton.Client.Id)
        {
            UIManager.Singleton.SetTracker(Id, transform.position);
        }
    }

    private void FireBullet(Vector3 startPos, Vector3 _velocity, ushort bulletType, ushort playerId)
    {
        if (bulletType == 2)
        {
            audioScript.PlaySingleSound(4);

            GameObject newBullet = Instantiate(UIManager.Singleton.bulletPrefabs[bulletType], startPos, Quaternion.Euler(_velocity.normalized));
            newBullet.GetComponent<Fire>().velocity = _velocity;
        }
        else if (bulletType == 3)
        {
            audioScript.PlaySound(5);

            GameObject newBomb = Instantiate(UIManager.Singleton.bulletPrefabs[bulletType], startPos, Quaternion.Euler(_velocity.normalized));
        }
        else if (bulletType == 7)
        {
            GameObject newLaser = Instantiate(UIManager.Singleton.bulletPrefabs[bulletType], startPos, Quaternion.identity);
            newLaser.transform.up = _velocity;

            if (GameObject.Find("Laser" + playerId) != null)
            {
                newLaser.transform.SetParent(GameObject.Find("Laser" + playerId).transform);
            }
            else
            {
                audioScript.PlaySound(10);

                GameObject laserHolder = Instantiate(new GameObject(), transform.position, Quaternion.identity);
                laserHolder.AddComponent<Laser>();
                laserHolder.GetComponent<Laser>().source = this.gameObject;
                laserHolder.name = "Laser" + playerId;

                newLaser.transform.SetParent(GameObject.Find("Laser" + playerId).transform);
            }
        }
        else if (bulletType == 8)
        {
            GameObject newLaser = Instantiate(UIManager.Singleton.bulletPrefabs[bulletType], startPos, Quaternion.identity);
            newLaser.transform.up = _velocity;

            if (GameObject.Find("Laser" + playerId) != null)
            {
                newLaser.transform.SetParent(GameObject.Find("Laser" + playerId).transform);
            }
            else
            {
                audioScript.PlaySound(11);

                GameObject laserHolder = Instantiate(new GameObject(), transform.position, Quaternion.identity);
                laserHolder.AddComponent<Laser>();
                laserHolder.GetComponent<Laser>().source = this.gameObject;
                laserHolder.name = "Laser" + playerId;

                newLaser.transform.SetParent(GameObject.Find("Laser" + playerId).transform);
            }
        }
        else if (bulletType == 11)
        {
            GameObject newLaser = Instantiate(UIManager.Singleton.bulletPrefabs[bulletType], startPos, Quaternion.identity);
            newLaser.transform.up = _velocity;

            if (GameObject.Find("Laser" + playerId) != null)
            {
                newLaser.transform.SetParent(GameObject.Find("Laser" + playerId).transform);
            }
            else
            {
                audioScript.PlaySound(15);

                GameObject laserHolder = Instantiate(new GameObject(), transform.position, Quaternion.identity);
                laserHolder.AddComponent<Laser>();
                laserHolder.GetComponent<Laser>().source = this.gameObject;
                laserHolder.name = "Laser" + playerId;

                newLaser.transform.SetParent(GameObject.Find("Laser" + playerId).transform);
            }
        }
        else
        {
            if (bulletType == 4)
            {
                audioScript.PlaySound(3);
            }
            else if (bulletType == 9)
            {
                audioScript.PlaySound(12);
            }
            else
            {
                audioScript.PlaySound(2);
            }

            GameObject newBullet = Instantiate(UIManager.Singleton.bulletPrefabs[bulletType], startPos, Quaternion.Euler(_velocity.normalized));
            newBullet.GetComponent<Bullet>().velocity = _velocity;
            newBullet.transform.forward = _velocity.normalized;
        }
    }

    private static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.username = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})"; 
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]

    private static void SpawnPlayer(Message message)
    {
        ushort playerId = message.GetUShort();
        string username = message.GetString();
        Vector3 position = message.GetVector3();
        Spawn(playerId, username, position);
    }

    [MessageHandler((ushort)ServerToClientId.clientDisconnect)] 

    private static void DisconnectClient(Message message)
    {
        Debug.Log("THE SERVER HAS THE WRONG VERSION!");
        NetworkManager.Singleton.Client.Disconnect();
    }

    [MessageHandler((ushort)ServerToClientId.killMissile)]

    private static void HandleMissileKill(Message message)
    {
        ushort lookFor = message.GetUShort();

        Missile remove = null;
        foreach (Missile currentMissile in GameLogic.Singleton.missiles)
        {
            if (currentMissile.number == lookFor)
            {
                remove = currentMissile;
                break;
            }
        }

        GameLogic.Singleton.missiles.Remove(remove);
        if (remove.reference != null)
        {
            Destroy(remove.reference);
        }
    }

    [MessageHandler((ushort)ServerToClientId.sendMissile)]

    private static void HandleMissile(Message message)
    {
        Vector3 pos = message.GetVector3();
        ushort type = message.GetUShort();
        ushort number = message.GetUShort();

        List<Missile> remove = new List<Missile>();

        foreach (Missile currentMissile in GameLogic.Singleton.missiles)
        {
            if (currentMissile.reference == null)
            {
                remove.Add(currentMissile);
            }
        }

        foreach (Missile currentMissile in remove)
        {
            GameLogic.Singleton.missiles.Remove(currentMissile);
        }

        bool missileFound = false;
        foreach (Missile currentMissile in GameLogic.Singleton.missiles)
        {
            if (currentMissile.number == number)
            {
                missileFound = true;
                currentMissile.reference.transform.position = pos;
            }
        }

        if (!missileFound)
        {
            if (list.TryGetValue(NetworkManager.Singleton.Client.Id, out Player player))
            {
                if (type == 5)
                {
                    player.audioScript.PlaySound(6);
                }
                else if (type == 6)
                {
                    player.audioScript.PlaySound(7);
                }
                else if (type == 10)
                {
                    player.audioScript.PlayResetSound(13);
                }
            }

            GameObject newObject = Instantiate(UIManager.Singleton.bulletPrefabs[type], pos, Quaternion.identity);

            GameLogic.Singleton.missiles.Add(new Missile(newObject, number, type));
        }
    }

    public void SetModel(ushort modelInput)
    {
        if (modelInput == 0 && transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        if (modelInput != 0)
        {
            if (transform.childCount > 0)
            {
                if (modelInput != modelId || !transform.GetChild(0).gameObject.activeSelf)
                {
                    Destroy(transform.GetChild(0).gameObject);

                    if (transform.childCount < 1)
                    {
                        GameObject newModel = Instantiate(UIManager.Singleton.shipModels[modelInput], transform.position, Quaternion.Euler(UIManager.Singleton.shipModels[modelInput].transform.eulerAngles));
                        newModel.transform.SetParent(transform);
                        newModel.transform.localPosition = UIManager.Singleton.shipOffsets[modelInput];
                    }

                    modelId = modelInput;
                }
            }
            
            if (transform.childCount < 1)
            {
                GameObject newModel = Instantiate(UIManager.Singleton.shipModels[modelInput], transform.position, Quaternion.Euler(UIManager.Singleton.shipModels[modelInput].transform.eulerAngles));
                newModel.transform.SetParent(transform);
                newModel.transform.localPosition = UIManager.Singleton.shipOffsets[modelInput];
            }

            modelId = modelInput;
        }
    }

    [MessageHandler((ushort)ServerToClientId.mapData)]

    private static void SpawnMap(Message message)
    {
        Debug.Log("GAF");
        ushort seed = message.GetUShort();
        ushort count = message.GetUShort();
        ushort rad = message.GetUShort();
        GameObject.Find("Map").GetComponent<MapGen>().Generate(seed, count, rad);
        GameObject.Find("Canvas").GetComponent<UIManager>().mapSize = rad;
        if (list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.SetModel(0);
            player.SetModel(UIManager.Singleton.selectedShip);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]

    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetVector3(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.spawnBullet)]

    private static void HandleBullet(Message message)
    {
        ushort playerId = message.GetUShort();
        if (list.TryGetValue(playerId, out Player player))
            player.FireBullet(message.GetVector3(), message.GetVector3(), message.GetUShort(), playerId);
    }

    [MessageHandler((ushort)ServerToClientId.playerHit)]

    private static void HandleHit(Message message)
    {
        ushort playerId = message.GetUShort();
        float damage = message.GetFloat();
        ushort type = message.GetUShort();
        if (list.TryGetValue(playerId, out Player player))
        {
            if (playerId == NetworkManager.Singleton.Client.Id)
            {
                if (type == 0)
                {
                    player.GetComponent<PlayerController>().UpdateHealth(damage);
                    player.audioScript.PlaySound(9);
                }
                else if (type == 1)
                {
                    player.GetComponent<PlayerController>().UpdateBattery(damage);
                }
            }
        }   
    }

    [MessageHandler((ushort)ServerToClientId.changeState)]

    private static void HandleState(Message message)
    {
        ushort state = message.GetUShort();

        if (list.TryGetValue(NetworkManager.Singleton.Client.Id, out Player player))
        {
            player.GetComponent<PlayerController>().state = state;
            if (state == 2)
            {
                player.GetComponent<PlayerController>().Spectate();
            }

            if (state == 1)
            {
                player.GetComponent<PlayerController>().healthDisplay.KillUI(0);
                player.GetComponent<PlayerController>().InitializeHUD();
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.gameOver)]

    private static void GameOver(Message message)
    {
        if (list.TryGetValue(NetworkManager.Singleton.Client.Id, out Player player))
        {
            player.GetComponent<PlayerController>().GameOver(message.GetUShort(), message.GetUShort());
            player.GetComponent<PlayerController>().healthDisplay.oldShipChoices = new List<ushort>();
        }
    }

    [MessageHandler((ushort)ServerToClientId.shipData)]

    private static void HandleShip(Message message)
    {
        if (list.TryGetValue(NetworkManager.Singleton.Client.Id, out Player player))
        {
            player.GetComponent<PlayerController>().maxHealth = (float)message.GetFloat();
            player.GetComponent<PlayerController>().health = player.GetComponent<PlayerController>().maxHealth;

            player.GetComponent<PlayerController>().maxBattery = (float)message.GetFloat();
            player.GetComponent<PlayerController>().battery = player.GetComponent<PlayerController>().maxBattery;
        }
    }

    [MessageHandler((ushort)ServerToClientId.modelData)]

    private static void HandleModel(Message message)
    {
        ushort namesCount = message.GetUShort();

        List<ushort> playerIds = new List<ushort>();

        for (int i = 0; i < namesCount; i++)
        {
            playerIds.Add(message.GetUShort());
        }

        for (int i = 0; i < namesCount; i++)
        {
            if (list.TryGetValue(playerIds[i], out Player _player))
                _player.SetModel(message.GetUShort());
        }
    }

    [MessageHandler((ushort)ServerToClientId.lobbyInfo)]

    private static void PlayerNames(Message message)
    {
        ushort namesCount = message.GetUShort();

        List<string> playerNames = new List<string>();
        List<ushort> playerChoices = new List<ushort>();
        List<ushort> playerIds = new List<ushort>();

        for (int i = 0; i < namesCount; i++)
        {
            playerNames.Add(message.GetString());
        }

        for (int i = 0; i < namesCount; i++)
        {
            playerIds.Add(message.GetUShort());
        }

        for (int i = 0; i < namesCount; i++)
        {
            ushort choice = message.GetUShort();
            playerChoices.Add(choice);
            if (list.TryGetValue(playerIds[i], out Player player))
            {
                player.SetModel(choice);
            }
        }

        if (list.TryGetValue(NetworkManager.Singleton.Client.Id, out Player _player))
        {
            if (_player.GetComponent<PlayerController>().healthDisplay != null)
            {
                _player.GetComponent<PlayerController>().healthDisplay.ListPlayers(playerNames, playerChoices);
                if (_player.Id == message.GetUShort())
                {
                    _player.GetComponent<PlayerController>().healthDisplay.host = true;
                }
                else
                {
                    _player.GetComponent<PlayerController>().healthDisplay.host = false;
                }
            }  
        }
    }
}
