using System.Collections;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Floor floor = null;
    [SerializeField]
    private PlayerController playerController = null;
    [SerializeField]
    private GameObject uiController = null;

    private UnitManager unitManager = null;

    private static readonly RuntimePlatform[] EnableUIControllerPlatforms = new RuntimePlatform[]
    {
        RuntimePlatform.Android,
        RuntimePlatform.IPhonePlayer,
        RuntimePlatform.WebGLPlayer
    };

    Coroutine turnControll = null;

    private void Start()
    {
        uiController.gameObject.SetActive(EnableUIControllerPlatforms.Contains(Application.platform));
        floor.Clear();
        floor.Create(20, 20, 4, false);
        playerController.SetFloor(floor);
        playerController.Spawn(floor.FloorData.SpawnPoint);
        turnControll = StartCoroutine(TurnControll());
        unitManager = new UnitManager(playerController.Player);
        unitManager.Initialize(floor);
    }

    private void OnDestroy()
    {
        StopCoroutine(turnControll);
    }

    public IEnumerator TurnControll()
    {
        while (true)
        {
            yield return playerController.Controll();
        }
    }
}
