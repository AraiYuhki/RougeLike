using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IState
{
    void OnEnter();
    void OnExit();
    void Update();
}