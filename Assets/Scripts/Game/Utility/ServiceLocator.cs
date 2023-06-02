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

    public static GameController GameController => instance.gameController;

    
    public static FloorManager FloorManager => instance.floorManager;
    public static EnemyManager EnemyManager => instance.enemyManager;
    public static ItemManager ItemManager => instance.itemManager;
    public static DungeonUI DungeonUI => instance.dungeonUI;
    public static DialogManager DialogManager => instance.dialogManager;
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
