using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene
{
    Title = 0,
    MainMenu,
    Game,
    Garrely,
}

/// <summary>
/// シーン管理クラス
/// シーンロード時に次の一度だけ読み出せるデータを1つだけ保持させられる。
/// </summary>
public class SceneController : MonoBehaviour
{
    private static SceneController instance;
    public static SceneController Instance
    {
        get
        {
            if (instance == null)
                return new GameObject(nameof(SceneController)).AddComponent<SceneController>();
            return instance;
        }
    }
    [SerializeField]
    private object bridgeData;

    private void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this)
        {
            Debug.LogWarning($"Duplicate instance create {nameof(SceneController)}, and destroy new instance");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 指定したシーンを読み込む
    /// </summary>
    /// <param name="nextScene"></param>
    /// <param name="data"></param>
    public void LoadScene(Scene nextScene, object data)
    {
        bridgeData = data;
        Debug.Log($"Load scene {nextScene}");
        SceneManager.LoadSceneAsync(nextScene.ToString(), LoadSceneMode.Single)
            .ToUniTask(cancellationToken: destroyCancellationToken)
            .Forget();
    }

    /// <summary>
    /// 受け渡すために保存したデータを受け取る
    /// 一度受け取ったらデータを削除し、メモリ上に存在する時間を減らす
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetData<T>()
    {
        var tmp = (T)bridgeData;
        bridgeData = null;
        return tmp;
    }

}
