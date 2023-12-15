using System;
using UnityEngine;

[Serializable]
public class FloorEnemySpawnInfo
{
    [SerializeField, CsvColumn("groupId")]
    private int groupId;
    [SerializeField, CsvColumn("enemyId")]
    private int enemyId;
    [SerializeField, CsvColumn("probability")]
    private int probability;

    public int GroupId => groupId;
    public int EnemyId => enemyId;
    public int Probability => probability;

    public FloorEnemySpawnInfo() { }

    public FloorEnemySpawnInfo Clone()
    {
        return new FloorEnemySpawnInfo()
        {
            groupId = groupId,
            enemyId = enemyId,
            probability = probability
        };
    }

#if UNITY_EDITOR
    public FloorEnemySpawnInfo(int groupId)
    {
        this.groupId = groupId;
    }
    public void SetEnemyId(int id) => enemyId = id;
    public void SetProbarbility(int probability) => this.probability = probability;
#endif
}
