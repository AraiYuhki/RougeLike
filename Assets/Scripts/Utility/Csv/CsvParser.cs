using Cysharp.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using System;
using System.CodeDom;

public class CsvParser
{
    public static List<Dictionary<string, string>> Parse(string path, string separator = "\t")
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

    public static List<T> Parse<T>(string path, string separator = "\t") where T : new()
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
                    SetValue(instance, member as PropertyInfo, pair.Value);
                else if (member.MemberType == MemberTypes.Field)
                    SetValue(instance, member as FieldInfo, pair.Value);
                else
                    Debug.LogError($"{pair.Key} is not property or field");
            }
            result.Add(instance);
        }
        return result;
    }

    private static void SetValue<T>(T instance, PropertyInfo property, string value) where T : new()
    {
        if (property.PropertyType == typeof(int))
            property.SetValue(instance, int.Parse(value));
        else if (property.PropertyType == typeof(long))
            property.SetValue(instance, long.Parse(value));
        else if (property.PropertyType == typeof(uint))
            property.SetValue(instance, uint.Parse(value));
        else if (property.PropertyType == typeof(float))
            property.SetValue(instance, float.Parse(value));
        else if (property.PropertyType == typeof(bool))
            property.SetValue(instance, bool.Parse(value));
        else if (property.PropertyType == typeof(string))
            property.SetValue(instance, value);
        else if (property.PropertyType == typeof(Vector2))
        {
            var array = value.Split(",");
            property.SetValue(instance, new Vector2(float.Parse(array[0]), float.Parse(array[1])));
        }
        else if (property.PropertyType == typeof(Vector2Int))
        {
            var array = value.Split(",");
            property.SetValue(instance, new Vector2Int(int.Parse(array[0]), int.Parse(array[1])));
        }
        else if (property.PropertyType == typeof(Vector3))
        {
            var array = value.Split(",");
            property.SetValue(instance, new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2])));
        }
        else if (property.PropertyType == typeof(Vector3Int))
        {
            var array = value.Split(",");
            property.SetValue(instance, new Vector3Int(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2])));
        }
        else if (property.PropertyType.BaseType == typeof(Enum))
            property.SetValue(instance, Enum.Parse(property.PropertyType, value));
        else
            throw new NotImplementedException($"{property.PropertyType.Name} is not supported");
    }

    private static void SetValue<T>(T instance, FieldInfo field, string value) where T : new()
    {
        if (field.FieldType == typeof(int))
            field.SetValue(instance, int.Parse(value));
        else if (field.FieldType == typeof(long))
            field.SetValue(instance, long.Parse(value));
        else if (field.FieldType == typeof(uint))
            field.SetValue(instance, uint.Parse(value));
        else if (field.FieldType == typeof(float))
            field.SetValue(instance, float.Parse(value));
        else if (field.FieldType == typeof(bool))
            field.SetValue(instance, bool.Parse(value));
        else if (field.FieldType == typeof(string))
            field.SetValue(instance, value);
        else if (field.FieldType == typeof(Vector2))
        {
            var array = value.Split(",");
            field.SetValue(instance, new Vector2(float.Parse(array[0]), float.Parse(array[1])));
        }
        else if (field.FieldType == typeof(Vector2Int))
        {
            var array = value.Split(",");
            field.SetValue(instance, new Vector2Int(int.Parse(array[0]), int.Parse(array[1])));
        }
        else if (field.FieldType == typeof(Vector3))
        {
            var array = value.Split(",");
            field.SetValue(instance, new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2])));
        }
        else if (field.FieldType == typeof(Vector3Int))
        {
            var array = value.Split(",");
            field.SetValue(instance, new Vector3Int(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2])));
        }
        else if (field.FieldType.BaseType == typeof(Enum))
            field.SetValue(instance, Enum.Parse(field.FieldType, value));
        else
            throw new NotImplementedException($"{field.FieldType.Name} is not supported");
    }

    public static string ToCSV<T>(List<T> data, string separator = "\t")
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
                object value = null;
                if (member.MemberType == MemberTypes.Property)
                    value = type.GetProperty(member.Name).GetValue(row);
                else if (member.MemberType == MemberTypes.Field)
                    value = type.GetField(member.Name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(row);
                else
                    continue;
                values.Add(ToString(value));
            }
            builder.AppendLine(string.Join(separator, values));
        }
        return builder.ToString();
    }

    private static string ToString(object value)
    {
        if (value is Vector2 vector2)
            return $"{vector2.x},{vector2.y}";
        if (value is Vector3 vector3)
            return $"{vector3.x},{vector3.y},{vector3.z}";
        if (value is Vector2Int vector2Int)
            return $"{vector2Int.x},{vector2Int.y}";
        if (value is Vector3Int vector3Int)
            return $"{vector3Int.x},{vector3Int.y},{vector3Int.z}";
        return value.ToString();

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
}
