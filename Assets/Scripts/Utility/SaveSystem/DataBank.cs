using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Text;

public class DataBank
{
    private static DataBank instance = new DataBank();
    private static Dictionary<string, object> bank = new();

    private const string path = "SaveData";
    private const string extension = "dat";
    private static readonly string fullPath = Path.Combine(Application.dataPath, "../", path);
    public static bool IsEncript { get; set; }

    public string SavePath => fullPath;

    private DataBank() { }

    public static DataBank Instance => instance;

    public bool IsEmpty() => bank.Count == 0;

    public bool ExistsKey(string key) => bank.ContainsKey(key);

    public void Store(string key, object obj) => bank[key] = obj;

    public void Clear() => bank.Clear();

    public void Remove(string key) => bank.Remove(key);

    public DataType Get<DataType>(string key) => ExistsKey(key) ? (DataType)bank[key] : default;

    public void SaveAll()
    {
        foreach (var key in bank.Keys)
            Save(key);
    }

    public bool Save(string key)
    {
        if (!ExistsKey(key)) return false;

        var filePath = $"{fullPath}/{key}.{extension}";
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var json = JsonUtility.ToJson(bank[key]);
        if (!IsEncript)
        {
            using (var streamWrite = new StreamWriter(filePath))
                streamWrite.WriteLine(json);
            return true;
        }

        var data = Encoding.UTF8.GetBytes(json);
        data = Compressor.Compress(data);
        data = Cryptor.Encrypt(data);


        using (var fileStream = File.Create(filePath))
            fileStream.Write(data, 0, data.Length);

        return true;
    }

    public bool Load<DataType>(string key)
    {
        var filePath = $"{fullPath}/{key}.{extension}";

        if (!File.Exists(filePath)) return false;

        byte[] data = null;
        using (var fileStream = File.OpenRead(filePath))
        {
            data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);
        }

        data = Cryptor.Decrypt(data);
        data = Compressor.Decompress(data);

        var json = Encoding.UTF8.GetString(data);

        bank[key] = JsonUtility.FromJson<DataType>(json);

        return true;
    }
}
