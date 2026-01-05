using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //SERVER SIDE

    public ushort state;

    [SerializeField] public Player player;
    [SerializeField] private float oldThrottle;
    [SerializeField] private float throttleFactor;
    [SerializeField] private float rotFactor;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private GameObject bulletPrefab;

    public float health;
    public float maxHealth;

    public float battery;
    public float maxBattery;

    //variables for taking in player inputs
    private Vector3 inputs;

    private bool attack;
    private bool ability;

    private ushort selectedShip;

    private float battRegen;

    private float voidTime;
    private float laserTimer;

    private void OnValidate()
    {
        if (player == null)
            player = GetComponent<Player>();
    }

    private void Start()
    {
        laserTimer = Time.time;
        voidTime = Time.time;
        inputs = new Vector3(0, 0, 0);
        battRegen = Time.time;
    }

    private void Update()
    {
        if (Time.time > battRegen + GameLogic.Singleton.ships[selectedShip].batteryRegen)
        {
            battRegen = Time.time;
            if (battery < maxBattery)
            {
                battery += 1;
                SendHit(player.Id, -1, 1);
            }
        }

        if (health < 1 && state == 1)
        {
            Spectate();
        }
        
        if (state == 1 && transform.childCount > 0)
        {
            if (!transform.GetChild(0).gameObject.activeSelf)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        foreach(Profile currentPlayer in GameLogic.Singleton.players)
        {
            if (currentPlayer.id == player.Id)
            {
                if (currentPlayer.state == 2)
                {
                    Spectate();
                }

                if (currentPlayer.state == 0 && state != 0)
                {
                    state = 0;
                }

                if (currentPlayer.state == 1 && state != 1)
                {
                    maxHealth = GameLogic.Singleton.ships[selectedShip].health;
                    health = maxHealth;
                    maxBattery = GameLogic.Singleton.ships[selectedShip].battery;
                    battery = maxBattery;
                    SendShipData();

                    velocity = Vector3.zero;
                    ushort mapSize = GameLogic.Singleton.mapRadius;
                    transform.position = new Vector3(Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize), Random.Range(-mapSize, mapSize));
                    transform.rotation = Quaternion.Euler(Vector3.zero);

                    state = 1;
                    SendStateChange(player.Id, 1);
                }

                selectedShip = currentPlayer.ship;
            }
        }
    }

    private void FixedUpdate()
    {
        if (state == 1)
        {
            if (transform.position.x > GameLogic.Singleton.mapRadius || transform.position.y > GameLogic.Singleton.mapRadius || transform.position.z > GameLogic.Singleton.mapRadius || transform.position.x < -GameLogic.Singleton.mapRadius || transform.position.y < -GameLogic.Singleton.mapRadius || transform.position.z < -GameLogic.Singleton.mapRadius)
            {
                if (Time.time > voidTime + 1)
                {
                    health--;
                    SendHit(player.Id, 1, 0);

                    voidTime = Time.time;
                }
            }

            Move();
        }

        if (selectedShip > 0 && transform.childCount < 1)
        {
            GameObject newModel = Instantiate(GameLogic.Singleton.ships[selectedShip].model, transform.position, Quaternion.Euler(GameLogic.Singleton.ships[selectedShip].model.transform.eulerAngles));
            newModel.transform.SetParent(transform);
            newModel.transform.localPosition = GameLogic.Singleton.ships[selectedShip].offset;
        }
        else if (selectedShip > 0 && transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds != GameLogic.Singleton.ships[selectedShip].model.GetComponent<MeshFilter>().sharedMesh.bounds)
        {
            Destroy(transform.GetChild(0).gameObject);

            GameObject newModel = Instantiate(GameLogic.Singleton.ships[selectedShip].model, transform.position, Quaternion.Euler(GameLogic.Singleton.ships[selectedShip].model.transform.eulerAngles));
            newModel.transform.SetParent(transform);
            newModel.transform.localPosition = GameLogic.Singleton.ships[selectedShip].offset;
        }
        else if (selectedShip < 1 && transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        if (state == 0 && transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }

        throttleFactor = GameLogic.Singleton.ships[selectedShip].speed;
        rotFactor = GameLogic.Singleton.ships[selectedShip].agility;
    }

    private void SendShipData()
    {
        //right now these are constants but they will change to reflect the player's choice of ship
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.shipData);
        message.AddFloat(GameLogic.Singleton.ships[selectedShip].health);
        message.AddFloat(GameLogic.Singleton.ships[selectedShip].battery);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }

    public void Spectate()
    {
        state = 2;
        health = 0;

        SendStateChange(player.Id, 2);

        ability = false;
        attack = false;

        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        int kill = -1;
        foreach (Profile currentPlayer in NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players)
        {
            if (currentPlayer.id == player.Id)
            {
                kill = NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players.IndexOf(currentPlayer);
                break;
            }
        }
        NetworkManager.Singleton.gameObject.GetComponent<GameLogic>().players[kill].state = 2;
    }

    private void Move()
    {
        float throttle = inputs.z * throttleFactor;

        if (throttle != 0 && oldThrottle <= throttle)
        {
            if (selectedShip == 5)
            {
                throttle = 1 * throttleFactor;
                velocity = transform.forward;
            }
            else
            {
                velocity = Vector3.Lerp(velocity, transform.forward * throttle, 0.01f);
            }
        }
        else if (throttle != 0 && oldThrottle > throttle && selectedShip == 5)
        {
            velocity = Vector3.zero;
        }
        oldThrottle = throttle;

        transform.position += velocity;

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + inputs.x * (-rotFactor), transform.eulerAngles.y + inputs.y * (rotFactor), transform.eulerAngles.z);

        if (attack == true)
        {
            if (selectedShip == 1)
            {
                if (battery >= 1)
                {
                    SendHit(player.Id, 1, 1);
                    battery -= 1;

                    GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[GameLogic.Singleton.ships[selectedShip].shipAttack], transform.position, Quaternion.Euler(transform.forward));
                    newBullet.GetComponent<Bullet>().velocity = velocity + transform.forward * 1;
                    newBullet.GetComponent<Bullet>().playerSource = this;
                    newBullet.GetComponent<Bullet>().id = player.Id;
                    SendBullet(transform.position, velocity + transform.forward * 1, GameLogic.Singleton.ships[selectedShip].shipAttack);
                }
            }
            else if (selectedShip == 2)
            {
                if (battery >= 2)
                {
                    SendHit(player.Id, 2, 1);
                    battery -= 2;

                    GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[GameLogic.Singleton.ships[selectedShip].shipAttack], transform.position, Quaternion.Euler(transform.forward));
                    newBullet.GetComponent<Bullet>().velocity = velocity + transform.forward * 1;
                    newBullet.GetComponent<Bullet>().playerSource = this;
                    newBullet.GetComponent<Bullet>().id = player.Id;
                    SendBullet(transform.position, velocity + transform.forward * 1, GameLogic.Singleton.ships[selectedShip].shipAttack);
                }
            }
            else if (selectedShip == 3)
            {
                if (battery >= 1)
                {
                    SendHit(player.Id, 1, 1);
                    battery -= 1;

                    GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[GameLogic.Singleton.ships[selectedShip].shipAttack], transform.position, Quaternion.Euler(transform.forward));
                    newBullet.GetComponent<Bullet>().velocity = velocity + transform.forward * 1;
                    newBullet.GetComponent<Bullet>().playerSource = this;
                    newBullet.GetComponent<Bullet>().id = player.Id;
                    SendBullet(transform.position, velocity + transform.forward * 1, GameLogic.Singleton.ships[selectedShip].shipAttack);
                }
            }
            else if (selectedShip == 4)
            {
                if (battery >= 8)
                {
                    ushort newNumber = 0;
                    bool numberFound = false;

                    if (GameLogic.Singleton.missiles.Count < 1)
                    {
                        numberFound = true;
                    }
                    else
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            newNumber = (ushort)i;
                            numberFound = true;

                            foreach (Missile currentMissile in GameLogic.Singleton.missiles)
                            {
                                if (currentMissile.number == newNumber)
                                {
                                    numberFound = false;
                                }
                            }

                            if (numberFound)
                            {
                                break;
                            }
                        }
                    }

                    if (numberFound)
                    {
                        GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[6], transform.position, Quaternion.Euler(transform.forward));
                        newBullet.transform.forward = transform.forward;
                        newBullet.GetComponent<Bullet>().velocity = velocity + transform.forward;
                        newBullet.GetComponent<Homing>().number = newNumber;
                        newBullet.GetComponent<Bullet>().playerSource = this;
                        newBullet.GetComponent<Bullet>().id = player.Id;
                        newBullet.GetComponent<Homing>().type = 6;
                        newBullet.GetComponent<Homing>().Initialize();

                        GameLogic.Singleton.missiles.Add(new Missile(newBullet, newNumber, 6));

                        SendHit(player.Id, 8, 1);
                        battery -= 8;
                    }
                }
            }
            else if (selectedShip == 5)
            {
                float closest = 0;
                Transform reference = null;
                ushort foundPlayer = 0;

                foreach (Missile currentMissile in GameLogic.Singleton.missiles)
                {
                    if (currentMissile.reference != null)
                    {
                        if (Vector3.Distance(transform.position, currentMissile.reference.transform.position) < 40)
                        {
                            if (closest == 0 || Vector3.Distance(transform.position, currentMissile.reference.transform.position) < closest)
                            {
                                foundPlayer = 0;
                                closest = Vector3.Distance(transform.position, currentMissile.reference.transform.position);
                                reference = currentMissile.reference.transform;
                            }
                        }
                    }
                }

                if (reference == null)
                {
                    foreach (Player currentPlayer in Player.list.Values)
                    {
                        if (Vector3.Distance(transform.position, currentPlayer.transform.position) < 40 && currentPlayer.Id != GetComponent<Player>().Id)
                        {
                            if (closest == 0 || Vector3.Distance(transform.position, currentPlayer.transform.position) < closest)
                            {
                                foundPlayer = currentPlayer.Id;
                                closest = Vector3.Distance(transform.position, currentPlayer.transform.position);
                                reference = currentPlayer.transform;
                            }
                        }
                    }
                }

                if (reference != null)
                {
                    if (battery > 6 && Time.time > laserTimer + 0.5f)
                    {
                        laserTimer = Time.time;
                        int laserDistance = Mathf.RoundToInt(closest);

                        for (int i = 0; i < laserDistance; i++)
                        {
                            GameObject laserSegment = Instantiate(GameLogic.Singleton.bulletPrefabs[8], transform.position + ((reference.position - transform.position).normalized * i), Quaternion.identity);
                            laserSegment.transform.forward = (reference.position - transform.position).normalized;

                            SendBullet(transform.position + ((reference.position - transform.position).normalized * i), (reference.position - transform.position).normalized, 8);
                        }

                        if (reference.gameObject.GetComponent<Fighter>() != null)
                        {
                            reference.gameObject.GetComponent<Fighter>().health -= 2;
                        }
                        else if (reference.gameObject.GetComponent<Homing>() != null)
                        {
                            reference.gameObject.GetComponent<Homing>().health -= 2;
                        }
                        else if (reference.gameObject.GetComponent<PlayerMovement>() != null)
                        {
                            reference.gameObject.GetComponent<PlayerMovement>().SendHit(reference.gameObject.GetComponent<Player>().Id, 2, 0);
                            reference.gameObject.GetComponent<PlayerMovement>().health -= 2;
                        }

                        SendHit(player.Id, 6, 1);
                        battery -= 6;
                    }
                }
            }
            if (selectedShip == 7)
            {
                if (battery >= 6)
                {
                    SendHit(player.Id, 6, 1);
                    battery -= 6;

                    GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[GameLogic.Singleton.ships[selectedShip].shipAttack], transform.position, Quaternion.Euler(transform.forward));
                    newBullet.GetComponent<Bullet>().velocity = velocity + transform.forward * 1;
                    newBullet.GetComponent<Bullet>().playerSource = this;
                    newBullet.GetComponent<Bullet>().id = player.Id;
                    SendBullet(transform.position, velocity + transform.forward * 1, GameLogic.Singleton.ships[selectedShip].shipAttack);
                }
            }
        }

        if (ability == true)
        {
            if (selectedShip == 1)
            {
                GameObject newBomb = Instantiate(GameLogic.Singleton.bulletPrefabs[3], transform.position, Quaternion.Euler(transform.forward));
                newBomb.GetComponent<Explosion>().playerSource = this;
                newBomb.GetComponent<Explosion>().id = player.Id;
                SendBullet(transform.position, Vector3.zero, 3);
                newBomb.GetComponent<Explosion>().Explode();
                health = 0;
            }
            else if (selectedShip == 2)
            {
                if (battery >= 2)
                {
                    GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[2], transform.position, Quaternion.Euler(transform.forward));
                    newBullet.GetComponent<Fire>().velocity = Vector3.zero;
                    newBullet.GetComponent<Fire>().playerSource = this;
                    newBullet.GetComponent<Fire>().id = player.Id;
                    SendBullet(transform.position, Vector3.zero, 2);

                    transform.position += transform.forward * 3;
                    velocity = transform.forward * throttleFactor;
                    SendHit(player.Id, 1, 1);
                    battery -= 1;
                }
            }
            else if (selectedShip == 3)
            {
                if (battery >= 3)
                {
                    ushort newNumber = 0;
                    bool numberFound = false;

                    if (GameLogic.Singleton.missiles.Count < 1)
                    {
                        numberFound = true;
                    }
                    else
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            newNumber = (ushort)i;
                            numberFound = true;

                            foreach (Missile currentMissile in GameLogic.Singleton.missiles)
                            {
                                if (currentMissile.number == newNumber)
                                {
                                    numberFound = false;
                                }
                            }

                            if (numberFound)
                            {
                                break;
                            }
                        }
                    }

                    if (numberFound)
                    {
                        GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[5], transform.position, Quaternion.Euler(transform.forward));
                        newBullet.transform.forward = -transform.forward;
                        newBullet.GetComponent<Bullet>().velocity = velocity + -transform.forward;
                        newBullet.GetComponent<Homing>().number = newNumber;
                        newBullet.GetComponent<Bullet>().playerSource = this;
                        newBullet.GetComponent<Bullet>().id = player.Id;
                        newBullet.GetComponent<Homing>().type = 5;
                        newBullet.GetComponent<Homing>().Initialize();

                        GameLogic.Singleton.missiles.Add(new Missile(newBullet, newNumber, 5));

                        SendHit(player.Id, 3, 1);
                        battery -= 3;
                    }
                }
            }
            else if (selectedShip == 4)
            {
                float closest = 0;
                Transform reference = null;
                ushort foundPlayer = 0;

                foreach (Missile currentMissile in GameLogic.Singleton.missiles)
                {
                    if (currentMissile.reference != null)
                    {
                        if (Vector3.Distance(transform.position, currentMissile.reference.transform.position) < 40)
                        {
                            if (closest == 0 || Vector3.Distance(transform.position, currentMissile.reference.transform.position) < closest)
                            {
                                foundPlayer = 0;
                                closest = Vector3.Distance(transform.position, currentMissile.reference.transform.position);
                                reference = currentMissile.reference.transform;
                            }
                        }
                    }
                }

                if (reference == null)
                {
                    foreach (Player currentPlayer in Player.list.Values)
                    {
                        if (Vector3.Distance(transform.position, currentPlayer.transform.position) < 40 && currentPlayer.Id != GetComponent<Player>().Id)
                        {
                            if (closest == 0 || Vector3.Distance(transform.position, currentPlayer.transform.position) < closest)
                            {
                                foundPlayer = currentPlayer.Id;
                                closest = Vector3.Distance(transform.position, currentPlayer.transform.position);
                                reference = currentPlayer.transform;
                            }
                        }
                    }
                }

                if (reference != null)
                {
                    //this is where the earthling's laser logic will go
                    if (battery > 2 && Time.time > laserTimer + 0.5f)
                    {
                        laserTimer = Time.time;
                        int laserDistance = Mathf.RoundToInt(closest);

                        for (int i = 0; i < laserDistance; i++)
                        {
                            GameObject laserSegment = Instantiate(GameLogic.Singleton.bulletPrefabs[7], transform.position + ((reference.position - transform.position).normalized * i), Quaternion.identity);
                            laserSegment.transform.forward = (reference.position - transform.position).normalized;

                            SendBullet(transform.position + ((reference.position - transform.position).normalized * i), (reference.position - transform.position).normalized, 7);
                        }

                        if (reference.gameObject.GetComponent<Fighter>() != null)
                        {
                            reference.gameObject.GetComponent<Fighter>().health -= 2;
                        }
                        else if (reference.gameObject.GetComponent<Homing>() != null)
                        {
                            reference.gameObject.GetComponent<Homing>().health -= 2;
                        }
                        else if (reference.gameObject.GetComponent<PlayerMovement>() != null)
                        {
                            reference.gameObject.GetComponent<PlayerMovement>().SendHit(reference.gameObject.GetComponent<Player>().Id, 1, 0);
                            reference.gameObject.GetComponent<PlayerMovement>().health -= 1;
                        }

                        SendHit(player.Id, 2, 1);
                        battery -= 2;
                    }
                }
            }
            else if (selectedShip == 5)
            {
                if (battery >= 6)
                {
                    transform.position = new Vector3(Random.Range(-GameLogic.Singleton.mapRadius, GameLogic.Singleton.mapRadius), Random.Range(-GameLogic.Singleton.mapRadius, GameLogic.Singleton.mapRadius), Random.Range(-GameLogic.Singleton.mapRadius, GameLogic.Singleton.mapRadius));
                    SendHit(player.Id, 6, 1);
                    battery -= 6;
                }
            }
            else if (selectedShip == 7)
            {
                if (battery >= 4)
                {
                    ushort newNumber = 0;
                    bool numberFound = false;

                    if (GameLogic.Singleton.missiles.Count < 1)
                    {
                        numberFound = true;
                    }
                    else
                    {
                        for (int i = 0; i < 200; i++)
                        {
                            newNumber = (ushort)i;
                            numberFound = true;

                            foreach (Missile currentMissile in GameLogic.Singleton.missiles)
                            {
                                if (currentMissile.number == newNumber)
                                {
                                    numberFound = false;
                                }
                            }

                            if (numberFound)
                            {
                                break;
                            }
                        }
                    }

                    if (numberFound)
                    {
                        GameObject newBullet = Instantiate(GameLogic.Singleton.bulletPrefabs[10], transform.position, Quaternion.Euler(transform.forward));
                        newBullet.transform.forward = transform.forward;
                        newBullet.GetComponent<Fighter>().velocity = velocity + transform.forward;
                        newBullet.GetComponent<Fighter>().number = newNumber;
                        newBullet.GetComponent<Fighter>().playerSource = this;
                        newBullet.GetComponent<Fighter>().id = player.Id;
                        newBullet.GetComponent<Fighter>().type = 10;
                        newBullet.GetComponent<Fighter>().Initialize();

                        GameLogic.Singleton.missiles.Add(new Missile(newBullet, newNumber, 10));

                        SendHit(player.Id, 4, 1);
                        SendHit(player.Id, 1, 0);
                        health -= 1;
                        battery -= 4;
                    }
                }
            }
        }

        attack = false;
        ability = false;

        SendMovement();
    }

    public void SendMissile(Vector3 position, ushort bulletType, ushort bulletId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.sendMissile);
        message.AddVector3(position);
        message.AddUShort(bulletType);
        message.AddUShort(bulletId);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void KillMissile(ushort id, ushort type)
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.killMissile);
        message.AddUShort(id);
        message.AddUShort(type);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void SetInput(Vector3 _inputs)
    {
        this.inputs = _inputs;
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddVector3(transform.position);
        message.AddVector3(transform.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void SendBullet(Vector3 startPos, Vector3 velocity, ushort bulletType)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.spawnBullet);
        message.AddUShort(player.Id);
        message.AddVector3(startPos);
        message.AddVector3(velocity);
        message.AddUShort(bulletType);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void SendHit(ushort hitPlayer, float damageValue, ushort damageType)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerHit);
        message.AddUShort(hitPlayer);
        message.AddFloat(damageValue);
        message.AddUShort(damageType);
        NetworkManager.Singleton.Server.Send(message, hitPlayer);
    }

    private void SendStateChange(ushort playerId, ushort state)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.changeState);
        message.AddUShort(state);
        NetworkManager.Singleton.Server.Send(message, playerId);
    }

    public void HandleButtons(bool _attack, bool _ability)
    {
        this.attack = _attack;
        this.ability = _ability;
    }
}
