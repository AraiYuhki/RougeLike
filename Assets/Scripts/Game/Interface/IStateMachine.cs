using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateMachine<T> where T : Enum
{
    public void Goto<T>(T state);
}
