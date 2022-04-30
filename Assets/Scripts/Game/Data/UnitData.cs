public abstract class UnitData
{
    public int MaxHP { get; private set; } = 10;
    public int Hp { get; set; } = 10;
    public int Lv { get; set; } = 1;
    public int Atk { get; set; } = 1;
    public int Def { get; set; } = 1;
    public UnitData(int hp) => Hp = MaxHP = hp;
}

public class PlayerData : UnitData
{
    public int TotalExp { get; set; } = 0;
    public float Stamina { get; set; } = 100f;
    public int MaxStamina { get; set; } = 100;
    public PlayerData(int hp) : base(hp) { }
}

public class EnemyData : UnitData
{
    public int Exp { get; set; } = 1;
    public EnemyData(int hp) : base(hp) { }
}
