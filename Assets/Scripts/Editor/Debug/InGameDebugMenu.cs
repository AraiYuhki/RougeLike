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
    private int addExp = 0;

    private int addGems = 0;
    private int selectItemId = 0;
    private int selectWeaponId = 0;
    private int selectShieldId = 0;

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
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("経験値増減")) target.AddExp(addExp);
            addExp = EditorGUILayout.IntField(addExp);
        }
        PlayerMenu();
        EnemyMenu();
    }

    private void AlwaysMenu()
    {
        var player = FindObjectOfType<Player>();
        if (GUILayout.Button("敵全滅"))
        {
            foreach (var e in FindObjectsOfType<Enemy>())
                e.Dead(player);
        }
    }

    private void PlayerMenu()
    {
        if (target is Player player)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("満腹度変更")) player.Data.Stamina = changeStamina;
                changeStamina = EditorGUILayout.Slider(changeStamina, 0f, player.Data.MaxStamina);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("所持金追加"))
                {
                    player.Data.Gems += addGems;
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
            var player = FindObjectOfType<Player>();
            if (GUILayout.Button("撃破"))
            {
                enemy.Dead(player);
                target = null;
            }
        }
    }
}
