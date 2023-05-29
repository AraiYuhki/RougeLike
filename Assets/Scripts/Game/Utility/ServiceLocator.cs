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
    private EnemyManager enemyManager;
    [SerializeField]
    private ItemManager itemManager;
    [SerializeField]
    private DungeonUI dungeonUI;
    [SerializeField]
    private DialogManager dialogManager;

    public GameController GameController => gameController;
    public FloorManager FloorManager => floorManager;
    public EnemyManager EnemyManager => enemyManager;
    public ItemManager ItemManager => itemManager;
    public DungeonUI DungeonUI => dungeonUI;
    public DialogManager DialogManager => dialogManager;
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
