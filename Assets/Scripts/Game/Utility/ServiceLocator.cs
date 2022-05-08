using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private DungeonUI dungeonUI;

    public GameController GameController => gameController;
    public FloorManager FloorManager => floorManager;
    public PlayerController PlayerController => playerController;
    public EnemyManager EnemyManager => enemyManager;
    public DungeonUI DungeonUI => dungeonUI;

    public static ServiceLocator Instance => instance;

    private static ServiceLocator instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        if (instance != this)
        {
            DestroyImmediate(gameObject);
        }
    }
}
