using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    public List<Missile> missiles;

    private void Awake()
    {
        missiles = new List<Missile>();
        Singleton = this;
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
