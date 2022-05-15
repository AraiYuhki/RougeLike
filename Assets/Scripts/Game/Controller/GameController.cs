using System.Collections;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject uiController = null;

    private Player Player => playerController.Player;
    private FloorManager floorManager => ServiceLocator.Instance.FloorManager;
    private PlayerController playerController => ServiceLocator.Instance.PlayerController;
    private EnemyManager enemyManager => ServiceLocator.Instance.EnemyManager;

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
        
        floorManager.Clear();
        floorManager.Create(20, 20, 4, false);

        playerController.SetFloor(floorManager);
        playerController.Spawn(floorManager.FloorData.SpawnPoint);
        Player.OnMoved += floorManager.OnMoveUnit;

        turnControll = StartCoroutine(TurnControll());

        enemyManager.Initialize(playerController.Player);
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
            yield return enemyManager.Controll();
            foreach (var unit in FindObjectsOfType<Unit>())
                unit.TurnEnd();
        }
    }
}
