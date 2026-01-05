using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //CLIENT SIDE

    public ushort state;

    public UIManager healthDisplay;
    private RectTransform cursor;
    public Transform cam;
    
    [SerializeField] private float camSpeed;

    public float health;
    public float maxHealth;

    public float battery;
    public float maxBattery;

    //inputs to send to the server
    private Vector3 inputs;

    private bool attack;
    private bool ability;

    //variables for storing player inputs
    private float h;
    private float v;
    private float f;

    private ushort shipSelected;
    private Vector3 camOffset;

    private Audio audioScript;

    private ushort camLock;
    private bool winMusic;

    private ushort winningShip;

    private void Start()
    {
        audioScript = GameObject.Find("AudioManager").GetComponent<Audio>();
        camOffset = new Vector3();
        //setting variables
        cursor = GameObject.Find("Cursor").GetComponent<RectTransform>();
        cam = GameObject.Find("Main Camera").transform;
        healthDisplay = GameObject.Find("Canvas").GetComponent<UIManager>();

        //initializing inputs
        inputs = new Vector3(0, 0, 0);
    }

    private void Update()
    {
        if (camLock != 0 && Player.list.TryGetValue(camLock, out Player player))
        {
            healthDisplay.Win(player.username);
        }

        if (transform.position.x > healthDisplay.mapSize || transform.position.y > healthDisplay.mapSize || transform.position.z > healthDisplay.mapSize || transform.position.x < -healthDisplay.mapSize || transform.position.y < -healthDisplay.mapSize || transform.position.z < -healthDisplay.mapSize)
        {
            healthDisplay.inVoid = true;
        }
        else
        {
            healthDisplay.inVoid = false;
        }

        if (healthDisplay.mapSize == 0)
        {
            healthDisplay.inVoid = false;
        }

        if (state != 1)
        {
            inputs = Vector3.zero;
        }

        //this will eventually bring up the pause menu instead
        if (Input.GetKeyDown(KeyCode.Escape) && state != 0)
        {
            UIManager.Singleton.TogglePaused();
        }

        if (state == 0)
        {
            healthDisplay.tracking = false;
            ability = false;
            attack = false;
            audioScript.PlayMusic(1);
            winMusic = false;

            camLock = 0;
        }
        else
        {
            audioScript.PlayMusic(2);

            if (state == 1)
            {
                healthDisplay.tracking = true;
                camLock = 0;
            }
        }

        //Record keypresses
        h = Input.GetAxis("Horizontal");
        f = Input.GetAxis("Vertical");
        v = Input.GetAxis("Vertical 2");

        if (state == 0)
        {
            Cursor.lockState = CursorLockMode.None;
            cursor.gameObject.SetActive(false);
            healthDisplay.lobbyUI.SetActive(true);
        }
        else
        {
            healthDisplay.lobbyUI.SetActive(false);
            cursor.gameObject.SetActive(true);
            if (!UIManager.Singleton.paused)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }

            cursor.localPosition += new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0) * 8;

            if (state == 1)
            {
                inputs = new Vector3(cursor.localPosition.y, cursor.localPosition.x, Mathf.Clamp(f, 0, 1));

                //attack button
                if (Input.GetMouseButtonDown(0))
                {
                    attack = true;
                }
                else
                {
                    attack = false;
                }

                if (UIManager.Singleton.selectedShip == 2)
                {
                    //ability button, does nothing right now
                    if (Input.GetKey(KeyCode.Space))
                    {
                        ability = true;
                    }
                    else
                    {
                        ability = false;
                    }
                }
                else if (UIManager.Singleton.selectedShip == 1)
                {
                    //ability button, does nothing right now
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ability = true;
                    }
                    else
                    {
                        ability = false;
                    }
                }
                else if (UIManager.Singleton.selectedShip == 3)
                {
                    //ability button, does nothing right now
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ability = true;
                    }
                    else
                    {
                        ability = false;
                    }
                }
                else if (UIManager.Singleton.selectedShip == 4)
                {
                    //ability button, does nothing right now
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ability = true;
                    }
                    else
                    {
                        ability = false;
                    }
                }
                else if (UIManager.Singleton.selectedShip == 5)
                {
                    //ability button, does nothing right now
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        if (battery >= 6)
                        {
                            audioScript.PlaySound(14);
                        }
                        ability = true;
                    }
                    else
                    {
                        ability = false;
                    }
                }
                else if (UIManager.Singleton.selectedShip == 7)
                {
                    //ability button, does nothing right now
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        ability = true;
                    }
                    else
                    {
                        ability = false;
                    }
                }

                //sending them over
                if (attack == true || ability == true)
                {
                    SendButtons();
                }

                //reset the inputs
                SendInput();
                inputs = Vector3.zero;
            }
        }
    }

    private void FixedUpdate()
    { 
        //run either the normal ship controls or the freecam ones
        if (state == 1)
        {
            ShipControl();
        }

        if (state == 2)
        {
            Freecam();
        }

        if (state == 0)
        {
            cursor.localPosition = Vector3.zero;
        }

        if (UIManager.Singleton.selectedShip > 0 && transform.childCount < 1)
        {
            shipSelected = UIManager.Singleton.selectedShip;

            GameObject newModel = Instantiate(UIManager.Singleton.shipModels[UIManager.Singleton.selectedShip], transform.position, Quaternion.Euler(UIManager.Singleton.shipModels[UIManager.Singleton.selectedShip].transform.eulerAngles));
            newModel.transform.SetParent(transform);
            newModel.transform.localPosition = UIManager.Singleton.shipOffsets[UIManager.Singleton.selectedShip];
        }
        else if (UIManager.Singleton.selectedShip > 0 && shipSelected != UIManager.Singleton.selectedShip)
        {
            Destroy(transform.GetChild(0).gameObject);

            shipSelected = UIManager.Singleton.selectedShip;

            GameObject newModel = Instantiate(UIManager.Singleton.shipModels[UIManager.Singleton.selectedShip], transform.position, Quaternion.Euler(UIManager.Singleton.shipModels[UIManager.Singleton.selectedShip].transform.eulerAngles));
            newModel.transform.SetParent(transform);
            newModel.transform.localPosition = UIManager.Singleton.shipOffsets[UIManager.Singleton.selectedShip];
        }
        else if (UIManager.Singleton.selectedShip < 1 && transform.childCount > 0)
        {
            shipSelected = UIManager.Singleton.selectedShip;

            Destroy(transform.GetChild(0).gameObject);
        }
    }

    private IEnumerator ScreenShake()
    {
        for (int i = 0; i < 6; i++)
        {
            camOffset += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

            yield return new WaitForSeconds(0.025f);

            camOffset = Vector3.zero;

            yield return new WaitForSeconds(0.025f);
        }

        camOffset = Vector3.zero;
    }

    private void Freecam()
    {
        if (camLock == 0)
        {
            //The control scheme to be used when the player is spectating
            cam.position += (cam.forward * f) * camSpeed;
            cam.position += (cam.right * h) * camSpeed;
            cam.position += (cam.up * v) * camSpeed;
        }
        else if (Player.list.TryGetValue(camLock, out Player player))
        {
            healthDisplay.Win(player.username);
            cam.position = player.gameObject.transform.position + (-cam.forward * 10) * healthDisplay.shipScales[winningShip];
        }

        cam.rotation = Quaternion.Euler(cam.eulerAngles.x + cursor.localPosition.y * -0.075f, cam.eulerAngles.y + cursor.localPosition.x * 0.075f, cam.eulerAngles.z);
        cursor.localPosition = Vector3.Lerp(cursor.localPosition, Vector3.zero, 0.08f);
    }

    private void ShipControl()
    {
        cam.localPosition = (transform.position + -transform.forward * 20 * healthDisplay.shipScales[shipSelected] + transform.up * 3.5f * healthDisplay.shipScales[shipSelected]) + camOffset;
        cam.forward = transform.forward;

        cursor.localPosition = Vector3.Lerp(cursor.localPosition, Vector3.zero, 0.08f);
    }

    public void InitializeHUD()
    {
        healthDisplay.ConstructHealth(maxHealth, health, 0);
        healthDisplay.ConstructHealth(maxBattery, battery, 1);
    }

    public void Spectate()
    {
        //set the state to spectator mode
        state = 2;
        inputs = Vector3.zero;

        //hide the health and battery bars
        GameObject.Find("Canvas").GetComponent<UIManager>().KillUI(0);
        GameObject.Find("Canvas").GetComponent<UIManager>().KillUI(1);

        //no need for the model
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void UpdateHealth(float damageValue)
    {
        health -= damageValue;
        healthDisplay.ConstructHealth(maxHealth, health, 0);

        StartCoroutine(healthDisplay.ScreenShake());
        StartCoroutine(ScreenShake());
    }

    public void UpdateBattery(float damageValue)
    {
        battery -= damageValue;
        healthDisplay.ConstructHealth(maxBattery, battery, 1);
    }

    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.input);
        message.AddVector3(inputs);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendButtons()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.buttons);
        message.AddBool(attack);
        message.AddBool(ability);
        NetworkManager.Singleton.Client.Send(message);
    }

    IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(5.1f);

        state = 0;
    }

    public void GameOver(ushort alivePlayer, ushort shipType)
    {
        if (!winMusic)
        {
            StartCoroutine(ResetGame());
            audioScript.PlayMusic(-1);
            camLock = alivePlayer;
            winningShip = shipType;
            audioScript.PlayWin(shipType);
            healthDisplay.tracking = false;
            winMusic = true;
        }
    }
}
