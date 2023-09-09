using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

#if !EXCLUDE_UNITY_DEBUG_SHEET
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityDebugSheet.Runtime.Core.Scripts.DefaultImpl.Cells;
using UnityDebugSheet.Runtime.Core.Scripts.DefaultImpl.CellParts;
public class DatabaseDebugPage : DefaultDebugPageBase
{
    protected override string Title => "Database debug menu";
    private Player player;
    protected override void Start()
    {
        player = FindObjectOfType<Player>();
        base.Start();
    }

    public override IEnumerator Initialize()
    {
        AddPageLinkButton("Enemy debug top", onLoad: ((string pageId, EnemiesDebugPage page) x) =>
        {
            x.page.Setup(DB.Instance.MEnemy.All);
        });
        yield break;
    }
}

public class EnemiesDebugPage : DefaultDebugPageBase
{
    protected override string Title => "Enemy Debug Top";
    private List<EnemyInfo> enemies = new List<EnemyInfo>();
    private bool initialized = false;

    public void Setup(List<EnemyInfo> enemies)
    {
        this.enemies = enemies;
        initialized = true;
    }
    public override IEnumerator Initialize()
    {
        if (!initialized)
        {
            throw new System.Exception($"{typeof(EnemiesDebugPage)} is not set up");
        }
        foreach(var enemy in enemies)
        {
            AddPageLinkButton($"ID: {enemy.Id}", enemy.Name, onLoad: ((string pageId, EnemyDebugPage page) x) =>
            {
                x.page.Setup(enemy);
            });
        }
        return base.Initialize();
    }
}

public class EnemyDebugPage : DefaultDebugPageBase
{
    protected override string Title => "Enemy";
    private EnemyInfo data;
    private bool initialized = false;

    public void Setup(EnemyInfo data)
    {
        this.data = data;
        initialized = true;
    }
    public override IEnumerator Initialize()
    {
        if (!initialized)
            throw new System.Exception($"{typeof(EnemiesDebugPage)} is not set up");
        AddLabel($"ID: {data.Id}");
        AddInputField("–¼‘O", value: data.Name, valueChanged: value => data.SetName(value));
        AddInputField("HP", value: data.MaxHP.ToString(), valueChanged: value =>
        {
            if (int.TryParse(value, out var newValue))
                data.SetHp(newValue);
        });
        AddButton("íœ", textColor: Color.red, clicked: () => DebugSheet.Instance.PopPage(true));
        return base.Initialize();
    }
}

#endif
