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
            EditorGUILayout.LabelField("���s���ȊO�ł͎g�p�ł��܂���");
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
            if (GUILayout.Button("HP�ύX")) target.Hp = changeHp;
            changeHp = EditorGUILayout.IntField(changeHp);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("�o���l����")) target.AddExp(addExp);
            addExp = EditorGUILayout.IntField(addExp);
        }
        PlayerMenu();
        EnemyMenu();
    }

    private void AlwaysMenu()
    {
        var player = FindObjectOfType<Player>();
        if (GUILayout.Button("�G�S��"))
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
                if (GUILayout.Button("�����x�ύX")) player.Data.Stamina = changeStamina;
                changeStamina = EditorGUILayout.Slider(changeStamina, 0f, player.Data.MaxStamina);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("�A�C�e���ǉ�"))
                {
                    var item = DataBase.Instance.GetTable<MItem>().Data[selectItemId];
                    player.Data.TakeItem(item.Clone() as ItemBase);
                    Debug.Log($"{item.Name}���擾����");
                }
                selectItemId = EditorGUILayout.Popup(selectItemId, DataBase.Instance.GetTable<MItem>().Data.Select(row => row.Name).ToArray());
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("���푕��"))
                {
                    var weapon = DataBase.Instance.GetTable<MWeapon>().Data[selectWeaponId];
                    player.Data.EquipmentWeapon = weapon.Clone() as WeaponData;
                    Debug.Log($"{weapon.Name}�𑕔�����");
                }
                selectWeaponId = EditorGUILayout.Popup(selectWeaponId, DataBase.Instance.GetTable<MWeapon>().Data.Select(row => row.Name).ToArray());
            }
            using (new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button("������"))
                {
                    var shield = DataBase.Instance.GetTable<MShield>().Data[selectShieldId];
                    player.Data.EquipmentShield = shield.Clone() as ShieldData;
                    Debug.Log($"{shield.Name}�𑕔�����");
                }
                selectShieldId = EditorGUILayout.Popup(selectShieldId, DataBase.Instance.GetTable<MShield>().Data.Select(row => row.Name).ToArray());
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("�������ǉ�"))
                {
                    player.Data.Gems += addGems;
                    Debug.Log($"��������{addGems}���₵��");
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
            if (GUILayout.Button("���j"))
            {
                enemy.Dead(player);
                target = null;
            }
        }
    }
}
