using System.Collections;
using System.Collections.Generic;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

public class EnemyDebugPage : DefaultDebugPageBase
{
    private Player player;

    protected override string Title => "Enemy Debuging";

    protected override void Start()
    {
        player = FindObjectOfType<Player>();
    }
    public override IEnumerator Initialize()
    {
        AddButton("すべての敵に10ダメージ", clicked: () =>
        {
            var enemyManager = FindObjectOfType<EnemyManager>();
            if (enemyManager == null) return;
            foreach (var enemy in enemyManager.Enemies)
                enemy.Damage(10, player);
        });
        yield break;
    }
}
