using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TitleMenuState : IState
{
    private TitleStateMachine stateMachine;
    private TMP_Text titleLogo;
    private VerticalMenu titleMenu;
    private TitleMenuItem titleMenuItemPrefab;
    private bool isInitialized = false;

    public TitleMenuState(
        TitleStateMachine stateMachine,
        TMP_Text titleLogo,
        VerticalMenu titleMenu,
        TitleMenuItem titleMenuItemPrefab
        )
    {
        this.stateMachine = stateMachine;
        this.titleLogo = titleLogo;
        this.titleMenu = titleMenu;
        this.titleMenuItemPrefab = titleMenuItemPrefab;
    }

    public void OnEnter()
    {
        if (isInitialized)
            return;

        isInitialized = true;
        InitializeMenu();
        PlayAnimation();
    }

    private void InitializeMenu()
    {
        CreateMenuItem("はじめから", OnClickNewGame);
        if (DataBank.Instance.ExistSaveFile("save"))
            CreateMenuItem("つづきから", OnClickContinue);
        if (DataBank.Instance.ExistSaveFile("dungeon"))
            CreateMenuItem("中断から再開", OnClickResume);
        CreateMenuItem("オプション", OnClickOption);
        CreateMenuItem("ギャラリー", OnClickGarrely);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        CreateMenuItem("ゲーム終了", OnClickQuitGame);
#endif
        titleMenu.Initialize();
    }

    private void CreateMenuItem(string label, UnityAction onSubmit)
    {
        var instance = UnityEngine.Object.Instantiate(titleMenuItemPrefab);
        instance.Label = label;
        instance.OnClick.AddListener(onSubmit);
        titleMenu.AddItem(instance);
    }

    private async void PlayAnimation()
    {
        titleMenu.Enable = false;
        foreach (TitleMenuItem item in titleMenu.Items)
            item.Ready();
        
        titleLogo.alpha = 0f;
        titleLogo.transform.SetLocalPositionY(300f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        await DOTween.Sequence()
            .Append(titleLogo.DOFade(1f, 1.0f))
            .Join(titleLogo.transform.DOLocalMoveY(200f, 1.0f))
            .ToUniTask(cancellationToken: titleMenu.destroyCancellationToken);

        var tasks = new List<UniTask>();
        foreach (TitleMenuItem item in titleMenu.Items)
        {
            tasks.Add(item.PlayAnimation());
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }
        await UniTask.WhenAll(tasks);
        titleMenu.Enable = true;
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (!titleMenu.Enable) return;
        if (InputUtility.Down.IsTrigger())
            titleMenu.Down();
        else if (InputUtility.Up.IsTrigger())
            titleMenu.Up();
        if (InputUtility.Submit.IsTrigger())
            titleMenu.Submit();
    }

    private async void OnClickNewGame()
    {
        Debug.Log("Start new game");
        await Fade.Instance.FadeOutAsync();
        SceneController.Instance.LoadScene(Scene.Game, new GameSceneData(false, false));
    }

    private async void OnClickContinue()
    {
        Debug.Log("Continue game");
        await Fade.Instance.FadeOutAsync();
        SceneController.Instance.LoadScene(Scene.Game, new GameSceneData(true, false));
    }

    private async void OnClickResume()
    {
        Debug.Log("Resume game");
        await Fade.Instance.FadeOutAsync();
        SceneController.Instance.LoadScene(Scene.Game, new GameSceneData(true, true));
    }

    private void OnClickOption()
    {
        Debug.Log("Open option");
        stateMachine.Goto(TitleState.Config);
    }

    private void OnClickGarrely()
    {
        Debug.Log("Open garrely");
    }

    private void OnClickQuitGame()
    {

        stateMachine.OpenCommonDialog("確認", "ゲームを終了しますか？", ("はい", () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }),
        ("いいえ", () => stateMachine.Goto(TitleState.MainMenu))
        );
    }
}
