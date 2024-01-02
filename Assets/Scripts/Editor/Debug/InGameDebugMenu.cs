using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class InGameDebugMenu : EditorWindow
{
    private Unit target = null;

    [MenuItem("Tools/Debug/InGameDebugTool")]
    public static void Open() => GetWindow<InGameDebugMenu>();

    private int changeHp = 0;
    private float changeStamina = 0;

    private int addGems = 0;

    public void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("実行中以外では使用できません");
            return;
        }

        EditorGUI.BeginChangeCheck();
        target = EditorGUILayout.ObjectField(target, typeof(Unit), true) as Unit;
        if (EditorGUI.EndChangeCheck())
        {
            changeHp = target.Hp;
        }
        AlwaysMenu();
        if (target == null) return;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("HP変更")) target.Hp = changeHp;
            changeHp = EditorGUILayout.IntField(changeHp);
        }
        PlayerMenu();
        EnemyMenu();
    }

    private void AlwaysMenu()
    {
        var player = FindAnyObjectByType<Player>();
        if (GUILayout.Button("敵全滅"))
        {
            foreach (var e in FindObjectsByType<Enemy>(FindObjectsSortMode.InstanceID))
                e.Dead(player);
        }
    }

    private void PlayerMenu()
    {
        if (target is Player player)
        {
            var playerData = player.PlayerData;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("満腹度変更")) playerData.Stamina = changeStamina;
                changeStamina = EditorGUILayout.Slider(changeStamina, 0f, playerData.MaxStamina);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("所持金追加"))
                {
                    playerData.Gems += addGems;
                    Debug.Log($"所持金を{addGems}増やした");
                }
                addGems = EditorGUILayout.IntField(addGems);
            }
        }
    }

    private void EnemyMenu()
    {
        if (target is Enemy enemy)
        {
            var player = FindAnyObjectByType<Player>();
            if (GUILayout.Button("撃破"))
            {
                enemy.Dead(player);
                target = null;
            }
        }
    }
}
