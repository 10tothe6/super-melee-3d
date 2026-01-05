using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [Header("Connect UI")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject spritePrefab;
    [SerializeField] private InputField usernameField;
    [SerializeField] private InputField ipField;

    
    [SerializeField] public GameObject startGameButton;

    [Header("HUD")]
    [SerializeField] private Sprite[] points;
    [SerializeField] private Transform[] guides;
    [SerializeField] private Sprite backgroundSprite;

    [Header("Lobby UI")]
    [SerializeField] private GameObject playerDisplayPrefab;
    public GameObject lobbyUI;

    [Header("Ship Data")]
    [SerializeField] public Sprite[] shipSprites;
    [SerializeField] public GameObject[] shipModels;
    [SerializeField] public Vector3[] shipOffsets;
    [SerializeField] public GameObject[] bulletPrefabs;
    [SerializeField] private RectTransform[] shipButtons;
    [SerializeField] public float[] shipScales;

    public List<ushort> oldShipChoices;
    public ushort selectedShip;

    [SerializeField] private string version;    

    public bool host;

    public GameObject[] menus;

    public bool paused;
    private GameObject cursor;

    private List<GameObject> trackers;
    public GameObject trackerPrefab;

    public GameObject voidWarning;
    private float voidTime;
    public bool inVoid;

    public bool tracking;

    public ushort mapSize;

    private Audio audioScript;
    private Vector3[] oldLocalPos;
    
    public GameObject winGame;

    

    public IEnumerator ScreenShake()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int _i = 0; _i < guides.Length; _i++)
            {
                guides[_i].position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)) * 100;
            }

            yield return new WaitForSeconds(0.025f);

            for (int _i = 0; _i < guides.Length; _i++)
            {
                guides[_i].position = oldLocalPos[_i];
            }

            yield return new WaitForSeconds(0.025f);
        }

        for (int _i = 0; _i < guides.Length; _i++)
        {
            guides[_i].position = oldLocalPos[_i];
        }
    }

    public void Win(string winUser)
    {
        winGame.transform.GetChild(0).gameObject.GetComponent<Text>().text = winUser + " Won";
        winGame.SetActive(true);
    }

    private void Awake()
    {
        oldLocalPos = new Vector3[guides.Length];
        for (int i = 0; i < guides.Length; i++)
        {
            oldLocalPos[i] = guides[i].position;
        }

        audioScript = GameObject.Find("AudioManager").GetComponent<Audio>();
        trackers = new List<GameObject>();
        Singleton = this;
        cursor = GameObject.Find("Cursor");
        cursor.SetActive(false);

        SwitchMenu(0);

        voidTime = Time.time;

        audioScript.PlayMusic(0);
    }

    public void SetTracker(ushort playerId, Vector3 input)
    {
        if (tracking)
        {
            GameObject foundTracker = null;
            foreach (GameObject currentTracker in trackers)
            {
                if (currentTracker.name == playerId.ToString())
                {
                    foundTracker = currentTracker;
                    break;
                }
            }

            if (foundTracker != null)
            {
                foundTracker.transform.localPosition = Camera.main.WorldToScreenPoint(input) - new Vector3(Screen.width / 2, Screen.height / 2, 0);

                Vector3 m = Camera.main.gameObject.transform.forward;
                Vector3 n = input - Camera.main.gameObject.transform.position;

                float a = Mathf.Asin(Vector3.Dot(m, n) / (m.magnitude * n.magnitude));

                if (a < 0)
                {
                    foundTracker.SetActive(false);
                }
                else
                {
                    foundTracker.SetActive(true);
                }
            }
            else
            {
                foundTracker = Instantiate(trackerPrefab, Vector3.zero, Quaternion.identity);
                foundTracker.name = playerId.ToString();
                foundTracker.transform.SetParent(transform);
                foundTracker.transform.localPosition = Camera.main.WorldToScreenPoint(input) - new Vector3(Screen.width / 2, Screen.height / 2, 0);

                Vector3 m = Camera.main.gameObject.transform.forward;
                Vector3 n = input - Camera.main.gameObject.transform.position;

                float a = Mathf.Asin(Vector3.Dot(m, n) / (m.magnitude * n.magnitude));

                if (a < 0)
                {
                    foundTracker.SetActive(false);
                }
                else
                {
                    foundTracker.SetActive(true);
                }

                trackers.Add(foundTracker);
            }
        }
        else
        {
            for (int i = trackers.Count; i > 0; i--)
            {
                Destroy(trackers[i - 1]);
                trackers.RemoveAt(i - 1);
            }
        }
    }

    public void ConnectClicked()
    {
        cursor.SetActive(true);
        usernameField.interactable = false;
        ipField.interactable = false;
        connectUI.SetActive(false);

        if (string.IsNullOrEmpty(ipField.text))
        {
            NetworkManager.Singleton.ip = "127.0.0.1";
        }
        else
        {
            NetworkManager.Singleton.ip = ipField.text;
        }

        NetworkManager.Singleton.Connect();
    }

    public void SwitchMenu(int menuId)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (i == menuId)
            {
                menus[i].SetActive(true);
            }
            else
            {
                menus[i].SetActive(false);
            }
        }
    }

    public void TogglePaused()
    {
        paused = !paused;

        if (!paused)
        {
            SwitchMenu(-1);
        }
        else
        {
            SwitchMenu(3);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        ipField.interactable = true;
        connectUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        message.AddString(version);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void StartGame()
    {
        if (guides[3].transform.childCount > 1)
        {
            
        }

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.startGame);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void KillUI(ushort type)
    {
        for (int i = guides[type].childCount; i > 0; i--)
        {
            Destroy(guides[type].GetChild(i - 1).gameObject);
        }

        for (int i = guides[3].childCount; i > 0; i--)
        {
            Destroy(guides[3].GetChild(i - 1).gameObject);
        }
    }

    public void SetShip(int input)
    {
        selectedShip = (ushort)input;

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.shipChoice);
        message.AddUShort(selectedShip);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void Update()
    {
        if (lobbyUI.activeSelf)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && shipButtons[shipButtons.Length - 1].localPosition.y < -100)
            {
                for (int i = 0; i < shipButtons.Length; i++)
                {
                    shipButtons[i].localPosition += new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * -200, 0);
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0 && shipButtons[0].localPosition.y > 160)
            {
                for (int i = 0; i < shipButtons.Length; i++)
                {
                    shipButtons[i].localPosition += new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * -200, 0);
                }
            }

            winGame.SetActive(false);
        }

        if (Time.time > voidTime + 0.75f && inVoid && !lobbyUI.activeSelf)
        {
            voidWarning.SetActive(!voidWarning.activeSelf);
            voidTime = Time.time;
        }

        if (!inVoid || lobbyUI.activeSelf)
        {
            voidWarning.SetActive(false);
        }

        if (host)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }

    public void ListPlayers(List<string> names, List<ushort> shipChoices)
    {
        if (oldShipChoices == null)
        {
            oldShipChoices = new List<ushort>();
        }

        bool choiceChange = false;
        for (int i = 0; i < shipChoices.Count; i++)
        {
            if (oldShipChoices.Count <= i)
            {
                choiceChange = true;
                break;
            }
            else
            {
                if (shipChoices[i] != oldShipChoices[i])
                {
                    choiceChange = true;
                    break;
                }
            }
        }

        if (choiceChange)
        {
            for (int i = guides[3].childCount; i > 0; i--)
            {
                Destroy(guides[3].GetChild(i - 1).gameObject);
            }

            int x = 0;
            int y = 0;

            for (int i = 0; i < names.Count; i++)
            {
                GameObject newObject = Instantiate(playerDisplayPrefab, guides[3].position, Quaternion.identity);
                if (!string.IsNullOrEmpty(names[i]))
                {
                    newObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = names[i];
                }
                else
                {
                    newObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Guest";
                }
                newObject.transform.SetParent(guides[3]);

                newObject.transform.localPosition += new Vector3((400 * x) * (Screen.width / 1920), (-480 * y) * (Screen.height / 1080), 0);

                newObject.GetComponent<Image>().sprite = shipSprites[shipChoices[i]];
                if (shipSprites[shipChoices[i]] != null)
                {
                    newObject.GetComponent<RectTransform>().sizeDelta = new Vector2(shipSprites[shipChoices[i]].bounds.extents.x * 1000, shipSprites[shipChoices[i]].bounds.extents.y * 1000);
                    newObject.GetComponent<RectTransform>().localScale = Vector3.one;
                }

                x++;
                if (x > 2)
                {
                    y++;
                    x = 0;
                }
            }

            oldShipChoices = shipChoices;
        }
    }

    public void ConstructHealth(float maxValue, float currentValue, ushort type)
    {
        for (int i = guides[type].childCount; i > 0; i--)
        {
            Destroy(guides[type].GetChild(i - 1).gameObject);
        }

        float roundValue = 0;
        if (maxValue % 2 != 0)
        {
            roundValue = (maxValue + 1) / 2;
        }
        else
        {
            roundValue = maxValue / 2;
        }

        for (int i = 0; i < roundValue; i++)
        {
            GameObject newSprite = Instantiate(spritePrefab, guides[type].position, Quaternion.identity);
            newSprite.GetComponent<Image>().sprite = backgroundSprite;
            newSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 40);
            newSprite.transform.SetParent(guides[type]);
            newSprite.GetComponent<RectTransform>().localPosition += new Vector3(0, 40 * i, 0);

            if (currentValue > 0)
            {
                GameObject newPoint = Instantiate(spritePrefab, newSprite.transform.position + new Vector3(0, 0, 0), Quaternion.identity);
                newPoint.GetComponent<Image>().sprite = points[type];
                newPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
                newPoint.GetComponent<RectTransform>().localPosition += new Vector3(30, 0, 0);
                newPoint.transform.SetParent(guides[type]);
                currentValue--;
            }

            if (currentValue > 0)
            {
                GameObject newPoint = Instantiate(spritePrefab, newSprite.transform.position + new Vector3(0, 0, 0), Quaternion.identity);
                newPoint.GetComponent<Image>().sprite = points[type];
                newPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
                newPoint.GetComponent<RectTransform>().localPosition += new Vector3(-30, 0, 0);
                newPoint.transform.SetParent(guides[type]);
                currentValue--;
            }
        }
    }
}
