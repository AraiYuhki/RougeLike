using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TrapType
{
    Mine,       // 地雷
    HugeMine,   // 大きな地雷
    PoisonTrap, // 毒菱
    BearTrap,   // トラバサミ
    Pitfall,    // 落とし穴
}

[Serializable]
public class TrapInfo
{
    [SerializeField]
    private int id;
    [SerializeField]
    private string name;
    [SerializeField]
    private TrapType type;
    [SerializeField]
    private bool enableAilment;
    [SerializeField]
    private AilmentData ailmentData;
    [SerializeField]
    private bool enableDamage;
    [SerializeField]
    private int damage;

    [SerializeField]
    private Trap prefab;

    public int Id => id;
    public string Name => name;
    public TrapType Type => type;
    public bool EnableAilment => enableAilment;
    public AilmentData Ailment => ailmentData;
    public int Damage => damage;
    public Trap Prefab => prefab;

    public Trap Instantiate(Transform parent)
    {
        return GameObject.Instantiate(prefab, parent);
    }

    public TrapInfo Clone()
    {
        return new TrapInfo()
        {
            id = id,
            name = name,
            type = type,
            enableAilment = enableAilment,
            ailmentData = ailmentData.Clone(),
            enableDamage = enableDamage,
            damage = damage,
            prefab = prefab,
        };
    }
}
