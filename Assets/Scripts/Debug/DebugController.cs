using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tayx.Graphy;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityDebugSheet.Runtime.Extensions.IngameDebugConsole;
using IngameDebugConsole;
using UnityDebugSheet.Runtime.Extensions.Graphy;
using Cysharp.Threading.Tasks;

public class DebugController : MonoBehaviour
{
#if !EXCLUDE_UNITY_DEBUG_SHEET 
    [SerializeField]
    private DebugSheet debugSheet;
#endif
    [SerializeField]
    private DebugLogManager debugLogManager;
    [SerializeField]
    private GraphyManager graphyManager;
    [SerializeField]
    private Canvas[] canvases;

    private void Awake()
    {
#if !EXCLUDE_UNITY_DEBUG_SHEET && !DEBUG
        Destroy(debugSheet.gameObject);
#endif
#if !DEBUG
        
        Destroy(debugLogManager.gameObject);
        Destroy(graphyManager.gameObject);
        Destroy(gameObject);
#else
        foreach(var canvas in canvases)
            canvas.enabled = true;
#endif
    }

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        var root = debugSheet.GetOrCreateInitialPage();
        root.AddPageLinkButton<PlayerDebugPage>(nameof(PlayerDebugPage));
        root.AddPageLinkButton<IngameDebugConsoleDebugPage>("In game debug console", onLoad: OnLoadDebugConsolePage);
        root.AddPageLinkButton<GraphyDebugPage>("Graphy", onLoad: OnLoadGraphy);

        var cancellationToken = this.GetCancellationTokenOnDestroy();
        await UniTask.RunOnThreadPool(async () =>
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);
#if !UNITY_EDITOR
        debugLogManager.PopupEnabled = false;
        debugLogManager.HideLogWindow();

        graphyManager.FpsModuleState = GraphyManager.ModuleState.OFF;
#else
            graphyManager.FpsModuleState = GraphyManager.ModuleState.TEXT;
#endif
            graphyManager.RamModuleState = GraphyManager.ModuleState.OFF;
            graphyManager.AudioModuleState = GraphyManager.ModuleState.OFF;
            graphyManager.AdvancedModuleState = GraphyManager.ModuleState.OFF;
        });
    }

    private void OnLoadDebugConsolePage((string pageId, IngameDebugConsoleDebugPage page) x)
    {
        x.page.Setup(debugLogManager);
    }

    private void OnLoadGraphy((string pageId, GraphyDebugPage page) x)
    {
        x.page.Setup(graphyManager);
    }
}
