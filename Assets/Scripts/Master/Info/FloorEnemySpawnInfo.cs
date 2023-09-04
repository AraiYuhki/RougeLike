using System;
using UnityEngine;

[Serializable]
public class FloorEnemySpawnInfo
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("groupId")]
    private int groupId;
    [SerializeField, CsvColumn("enemyId")]
    private int enemyId;
    [SerializeField, CsvColumn("probability")]
    private int probability;

    public int Id => id;
    public int GroupId => groupId;
    public int EnemyId => enemyId;
    public int Probability => probability;

    public FloorEnemySpawnInfo Clone()
    {
        return new FloorEnemySpawnInfo()
        {
            id = id,
            groupId = groupId,
            enemyId = enemyId,
            probability = probability
        };
    }
}
