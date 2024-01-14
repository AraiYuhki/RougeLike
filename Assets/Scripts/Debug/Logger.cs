using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class Logger
{
    private static Logger instance = null;
    public static Logger Instance
    {
        get
        {
            if (instance == null)
                instance = new Logger();
            return instance;
        }
    }
    private static readonly string LogPath = System.IO.Path.Combine(Application.dataPath, "../debugLogs");

    private StreamWriter sw;
    public bool EnableLogOutput { get; set; } = true;
    public bool EnableStackTraceOutput { get; set; } = true;

    [RuntimeInitializeOnLoadMethod]
    private static void Activate()
    {
#if DEBUG && !UNITY_EDITOR
        if (instance != null) return;
        instance = new Logger();
        Debug.Log("Activate Logger");
        instance.Initialize();
#endif
    }

    private void Initialize()
    {
        Application.logMessageReceived -= OnReceivedLogMessage;
        Application.logMessageReceived += OnReceivedLogMessage;
        Application.quitting -= Release;
        Application.quitting += Release;

        var filePath = System.IO.Path.Combine(LogPath, $"{DateTime.Now.ToString("yyyy-MM-dd")}.log");
        if (!File.Exists(filePath))
        {
            var file = new FileInfo(filePath);
            file.Directory.Create();
        }
        sw = new StreamWriter(filePath, true);

    }

    private async void OnReceivedLogMessage(string message, string stackTrace, LogType type)
    {
        if (!EnableLogOutput) return;

        var text = EnableStackTraceOutput
            ? $"{DateTime.Now} [{type}] {message}\n=================StackTrace=================\n{stackTrace}"
            : $"{DateTime.Now} [{type}] {message}";

        await sw.WriteLineAsync(text);
    }

    private void Release()
    {
        Application.logMessageReceived -= OnReceivedLogMessage;
        sw.Close();
        sw = null;
        Debug.Log("Release logger");
    }
}
