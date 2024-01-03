using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private Player player;

    [SerializeField]
    private Minimap minimap;
    [SerializeField]
    private NoticeGroup notice;

    [SerializeField]
    private TMP_Text ailmentsLabel;
    [SerializeField]
    private MainMenuUI mainMenu;
    [SerializeField]
    private InventoryUI inventory;
    [SerializeField]
    private UseMenuUI useMenu;

    private IControllable current => IsOpened ? uiStack.Peek() : null;
    private Stack<IControllable> uiStack = new Stack<IControllable>();
    public bool IsOpened => uiStack.Any();

    public void Initialize()
    {
        mainMenu.Initialize(
            OpenInventory,
            CheckStep,
            Suspend,
            Retire,
            CloseAll
            );
        floorManager.SetMinimap(minimap);
        inventory.Initialize(floorManager, this, notice, player);
    }

    private void Update()
    {
        var builder = new StringBuilder();
        foreach(var ailment in player.Data.Ailments.Values)
        {
            builder.AppendLine(ailment.ToString());
        }
        ailmentsLabel.text = builder.ToString();
    }

    public void OpenMainMenu(Action onComplete = null)
    {
        uiStack.Push(mainMenu);
        mainMenu.Open(onComplete);
        minimap.SetMode(MinimapMode.Menu);
    }

    public void OpenInventory() => OpenInventory(null);
    public void OpenInventory(Action onComplete)
    {
        inventory.Initialize();
        minimap.SetMode(MinimapMode.Normal);
        uiStack.Push(inventory);
        inventory.Open(onComplete);
    }

    public void OpenUseMenu(List<(string title, Action onSubmit)> selections, Action onComplete = null)
    {
        useMenu.Initialize(selections);
        uiStack.Push(useMenu);
        useMenu.Open(onComplete);
    }

    public void OpenCommonDialog(string title, string message, Action onComplete = null, params (string label, Action onClick)[] data)
    {
        var dialog = DialogManager.Instance.Create<CommonDialog>();
        dialog.Initialize(title, message, data);
        uiStack.Push(dialog);
        dialog.Open(onComplete);
    }

    /// <summary>
    /// 現在開いているUIを閉じる。
    /// </summary>
    /// <param name="onComplete"></param>
    /// <returns>まだUIが開かれているか？</returns>
    public bool CloseCurrent(Action onComplete = null)
    {
        // TODO: 種類ごとに色々やらないといけない
        if (current == null) return false;
        var ui = uiStack.Pop();
        if (ui is InventoryUI)
            minimap.SetMode(MinimapMode.Menu);
        else if (ui is MainMenuUI)
            minimap.SetMode(MinimapMode.Normal);
        if (ui is DialogBase dialog)
            DialogManager.Instance.Close(dialog, onComplete);
        else
            ui.Close(onComplete);
        return current != null;
    }
    public void CloseAll() => CloseAll(null);


    public void CloseAll(Action onComplete)
    {
        while(current != null)
        {
            var ui = uiStack.Pop();
            if (!IsOpened)
                ui.Close(onComplete);
            else
                ui.Close();
        }
        minimap.SetMode(MinimapMode.Normal);
    }

    public void CloseMainMenu(Action onComplete = null)
    {
        inventory.Close();
        useMenu.Close();
        mainMenu.Close(() =>
        {
            onComplete?.Invoke();
            uiStack.Clear();
        });
    }

    public void UpdateUI()
    {
        if (current == null) return;
        if (InputUtility.Right.IsTriggerd())
            current.Right();
        else if (InputUtility.Left.IsTriggerd())
            current.Left();
        else if (InputUtility.Up.IsTriggerd())
            current.Up();
        else if (InputUtility.Down.IsTriggerd())
            current.Down();
        else if (InputUtility.Submit.IsTriggerd())
            current.Submit();
        else if (InputUtility.Cancel.IsTriggerd())
            CloseCurrent();
    }

    public void ForceUpdateMinimap() => minimap.SetVisibleMap(player.Position);
    public void ClearMinimap() => minimap.Clear();

    private void CheckStep()
    {
    }

    private void Suspend()
    {
    }

    private void Retire()
    { 
    }
}
