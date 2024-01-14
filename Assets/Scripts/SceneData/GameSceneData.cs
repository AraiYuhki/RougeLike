public class GameSceneData
{
    public bool IsLoad { get; private set; }
    public bool IsResume { get; private set; }

    public GameSceneData(bool isLoad, bool isResume)
    {
        IsLoad = isLoad;
        IsResume = isResume;
    }
}
