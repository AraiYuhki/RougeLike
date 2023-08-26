using Cysharp.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CsvParser
{
    public static List<Dictionary<string, string>> Parse(string path, string separator = ",")
    {
        var result = new List<Dictionary<string, string>>();
        using (var streamReader = new StreamReader(path))
        {
            var headers = streamReader.ReadLine().Trim().Split(separator);
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine().Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var columns = line.Split(separator);
                var row = new Dictionary<string, string>();
                foreach ((var key, var index) in headers.Select((key, index) => (key, index)))
                    row[key] = columns[index];
                result.Add(row);
            }
        }
        return result;
    }

    public static List<T> Parse<T>(string path, string separator = ",") where T : new()
    {
        var parsed = Parse(path, separator);
        var type = typeof(T);
        (var attributes, var members) = GetProperties<T>();
        var result = new List<T>();
        foreach(var row in parsed)
        {
            var instance = new T();
            foreach(var pair in row)
            {
                if (!attributes.ContainsKey(pair.Key) || !members.ContainsKey(pair.Key)) continue;
                var member = members[pair.Key];
                if (member.MemberType == MemberTypes.Property)
                    type.GetProperty(member.Name).SetValue(instance, pair.Value);
                else if (member.MemberType == MemberTypes.Field)
                    type.GetField(member.Name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, pair.Value);
                else
                    Debug.LogError($"{pair.Key} is not property or field");
            }
            result.Add(instance);
        }
        return result;
    }

    public static string ToCSV<T>(List<T> data, string separator = ",")
    {
        var builder = new StringBuilder();
        (var attributes, var members) = GetProperties<T>();
        builder.AppendLine(string.Join(separator, attributes.Keys));
        var type = typeof(T);
        foreach(var row in data)
        {
            var values = new List<string>();
            foreach((_ , var member) in members)
            {
                if (member.MemberType == MemberTypes.Property)
                    values.Add(type.GetProperty(member.Name).GetValue(row).ToString());
                else if (member.MemberType == MemberTypes.Field)
                    values.Add(type.GetField(member.Name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(row).ToString());
            }
            builder.AppendLine(string.Join(separator, values));
        }
        return builder.ToString();
    }

    private static (Dictionary<string, CsvColumn> attributes, Dictionary<string, MemberInfo> members) GetProperties<T>()
    {
        var type = typeof(T);
        var attributes = new Dictionary<string, CsvColumn>();
        var members = new Dictionary<string, MemberInfo>();
        foreach (var member in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Select(field => field as MemberInfo).Concat(type.GetProperties()))
        {
            var csvColumn = member.GetCustomAttribute<CsvColumn>();
            if (csvColumn == null) continue;
            attributes.Add(csvColumn.Name, csvColumn);
            members.Add(csvColumn.Name, member);
        }
        return (attributes, members);
    }

#if UNITY_EDITOR
    private class CSVParserTester : EditorWindow
    {
        private DataBase dataBase;
        [MenuItem("Tools/CSVParseTester")]
        public static void Open()
        {
            GetWindow<CSVParserTester>();
        }

        public void OnGUI()
        {
            dataBase = EditorGUILayout.ObjectField(dataBase, typeof(DataBase), false) as DataBase;
            if (dataBase != null)
            {
                dataBase.Initialize();
            }
            using (new EditorGUI.DisabledGroupScope(dataBase == null))
            {
                if (GUILayout.Button("MEnemy"))
                {
                    var content = ToCSV(dataBase.GetTable<MEnemy>().Data);
                    Debug.LogError(content);
                }
                if (GUILayout.Button("MCard"))
                {
                    var content = ToCSV(dataBase.GetTable<MCard>().Data);
                    Debug.LogError(content);
                }
                if (GUILayout.Button("MPassiveEffect"))
                {
                    var content = ToCSV(dataBase.GetTable<MPassiveEffect>().Data);
                    Debug.LogError(content);
                }

                if (GUILayout.Button("MAttackArea"))
                {
                    var content = ToCSV(dataBase.GetTable<MAttackArea>().Data);
                    Debug.LogError(content);
                }
            }
        }
    }
    
    
#endif
}
