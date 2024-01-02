using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
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

    public List<TrapData> TrapList { get; private set; } = new List<TrapData>();

    public void Clear()
    {
        foreach (var trap in TrapList)
            Destroy(trap.gameObject);
        TrapList.Clear();
    }

    public void Initialize(FloorInfo floorInfo)
    {
        var trapInfo = DB.Instance.MFloorTrap.GetByGroupId(floorInfo.TrapSettingGroupId);
        var mTrap = DB.Instance.MTrap;
        var count = Random.Range(floorInfo.InstallTrapMinNum, floorInfo.InstallTrapMaxNum);
        for(var i = 0; i < count; i++)
        {
            var trapId = Lottery.Get(trapInfo).TrapId;
            var master = mTrap.GetById(trapId);
            var trap = master.Instantiate(floorManager.transform);
            var tile = floorManager.GetEmptyRoomTiles().Random();
            var data = new TrapData(trap, tile, master, floorManager, noticeGroup);
            floorManager.SetTrap(data, tile.Position);
        }
    }
}
