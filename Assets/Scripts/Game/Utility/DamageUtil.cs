using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageUtil
{
    public static int GetDamage(Unit attack, Unit defense)
    {
        if (attack is Player player && defense is Enemy enemy)
            return GetDamage(player, enemy);
        if (attack is Enemy e && defense is Player p)
            return GetDamage(e, p);
        throw new System.NotImplementedException();
    }
    public static int GetDamage(Player player, Enemy enemy)
    {
        var atk = player.Data.BaseAtk + (player.Data.BaseAtk * Mathf.RoundToInt((player.Data.Atk + player.Data.WeaponPower - 8) / 16));
        return GetResult(ApplyDef(atk, enemy.Data.Def));
    }

    public static int GetDamage(Enemy enemy, Player player)
    {
        var atk = enemy.Data.Atk + enemy.Data.Atk * Mathf.RoundToInt((enemy.Data.Atk - 8) / 16);
        return GetResult(ApplyDef(atk, player.Data.Def));
    }

    // “Š±ƒ_ƒ[ƒW
    public static int GetDamage(Player player, int baseAtk)
        => player.Data.BaseAtk + Mathf.RoundToInt(player.Data.BaseAtk* (baseAtk - 8) / 16);

    private static float ApplyDef(int atk, int def) => atk * Mathf.Pow(0.9375f, def);

    private static int GetResult(float baseDamage) => Mathf.Max(0, (int)Random.Range(baseDamage * 0.875f, baseDamage * 1.1171875f));
}
