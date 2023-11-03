using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private NoticeGroup noticeGroup;
    [SerializeField]
    private Trap[] trapTemplates = new Trap[0];

    public List<Trap> TrapList { get; private set; } = new List<Trap>();

    public void Clear()
    {
        foreach (var trap in TrapList)
            Destroy(trap);
        TrapList.Clear();
    }

    public void Initialize(int num)
    {
        for (var count = 0; count < num; count++)
        {
            var trap = trapTemplates.Random();
            var instance = Instantiate(trap, floorManager.transform);
            var tile = floorManager.GetEmptyRoomTiles().Random();
            instance.Setup(floorManager, noticeGroup);
            instance.SetPosition(tile.Position);
            floorManager.SetTrap(instance, instance.Position);
            TrapList.Add(instance);
        }
    }
}
