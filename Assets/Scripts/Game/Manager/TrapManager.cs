using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrapManager : MonoBehaviour
{
    [SerializeField]
    private Player player;
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private Minimap minimap;
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
        var count = Random.Range(floorInfo.InstallTrapMinNum, floorInfo.InstallTrapMaxNum);
        for(var i = 0; i < count; i++)
        {
            var trapId = Lottery.Get(trapInfo).TrapId;
            var tile = floorManager.GetEmptyRoomTiles().Random();
            Create(trapId, floorInfo, tile);
        }
    }

    public void LoadFromJson(List<TrapData> traps, FloorInfo floorInfo)
    {
        foreach (var trap in traps)
        {
            var data = Create(trap.Master.Id, floorInfo, floorManager.GetTile(trap.Position));
            data.SetVisible(trap.IsVisible);
        }
    }

    private TrapData Create(int trapId, FloorInfo floorInfo, TileData tile)
    {
        var master = DB.Instance.MTrap.GetById(trapId);
        var trap = master.Instantiate(floorManager.transform);
        
        var data = new TrapData(trap, tile, master, floorManager, noticeGroup);
        trap.SetMaterials(floorInfo.WallMaterial, floorInfo.FloorMaterial);
        TrapList.Add(data);
        minimap.AddSymbol(data);
        floorManager.SetTrap(data, tile.Position);
        return data;
    }
}
