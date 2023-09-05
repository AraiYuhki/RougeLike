using System.IO;
using UnityEditor;
using UnityEngine;

public static class MasterCsvConverter
{
    [MenuItem("Tools/Master/ExportToCsv")]
    public static void ConvertToCsv()
    {
        var db = DB.Instance;
        File.WriteAllText(GetPath("MEnemy"), CsvParser.ToCSV(db.MEnemy.All));
        File.WriteAllText(GetPath("MDungeon"), CsvParser.ToCSV(db.MDungeon.All));
        File.WriteAllText(GetPath("MFloor"), CsvParser.ToCSV(db.MFloor.All));
        File.WriteAllText(GetPath("MFloorEnemySpawn"), CsvParser.ToCSV(db.MFloorEnemySpawn.All));
        File.WriteAllText(GetPath("MCard"), CsvParser.ToCSV(db.MCard.All));
        File.WriteAllText(GetPath("MAttack"), CsvParser.ToCSV(db.MAttack.All));
        File.WriteAllText(GetPath("MAttackArea"), CsvParser.ToCSV(db.MAttackArea.All));
        File.WriteAllText(GetPath("MPassiveEffect"), CsvParser.ToCSV(db.MPassiveEffect.All));
        AssetDatabase.Refresh();
    }
    private static string GetPath(string fileName) => System.IO.Path.Combine(Application.streamingAssetsPath, $"{fileName}.tsv");
}
