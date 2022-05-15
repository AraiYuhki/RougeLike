using UnityEngine;

public abstract class UnitData
{
    protected int totalExp = 0;
    public int MaxHP { get; protected set; } = 15;
    public float Hp { get; set; } = 15f;
    public int Lv { get; set; } = 1;
    public int Atk { get; set; } = 1;
    public int Def { get; set; } = 0;
    public UnitData(int hp) => Hp = MaxHP = hp;
    public virtual void AddExp(int exp) => totalExp += exp;
}

public partial class PlayerData : UnitData
{
    public float Stamina { get; set; } = 100f;
    public float MaxStamina { get; set; } = 100f;
    public int Gems { get; set; } = 0;
    public PlayerData(int hp) : base(hp) 
    {
        Atk = 8;
    }
    public override void AddExp(int exp)
    {
        base.AddExp(exp);
        while (totalExp >= levelData[Lv].RequireExp)
        {
            MaxHP += Random.Range(1, 6);
            Hp = MaxHP;
            Lv++;
        }
    }
    public int BaseAtk => levelData[Lv - 1].Atk;
    public int WeaponPower => 0;

    public int CurrentLevelExp => totalExp - levelData[Lv - 1].RequireExp;
    public int NextLevelExp => levelData[Lv].RequireExp - levelData[Lv - 1].RequireExp;
}

public class EnemyData : UnitData
{
    public int Exp { get; set; } = 5;
    public EnemyData(int hp) : base(hp) { }
}
