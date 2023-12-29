using System;

public interface IControllable
{
    void Up();
    void Down();
    void Left();
    void Right();
    void Cancel();
    void Submit();
    void Open(Action onComplete = null);
    void Close(Action onComplete = null);
}
