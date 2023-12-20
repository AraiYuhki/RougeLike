using UnityEngine;
using UnityEngine.Events;
using System;

public class InventoryUI : ScrollMenu
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private UnityEvent onUseItem;

    private FloorManager floorManager;
    private UIManager uiManager;
    private NoticeGroup notice;
    private Player player;
    private PlayerData data => player.Data;
    public CanvasGroup Group => group;

    public override void Open(Action onComplete = null)
    {
        canvas.enabled = true;
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        base.Close(() =>
        {
            onComplete?.Invoke();
            canvas.enabled = false;
        });
    }

    public void Initialize(FloorManager floorManager, UIManager uiManager, NoticeGroup notice, Player player)
    {
        this.floorManager = floorManager;
        this.uiManager = uiManager;
        this.notice = notice;
        this.player = player;
        Initialize();
    }

    public void Initialize()
    {
        Clean();
    }
}
