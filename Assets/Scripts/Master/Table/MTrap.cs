using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Master/MTrap")]
public class MTrap : ScriptableObject
{
    [SerializeField]
    private List<TrapInfo> data;

    private Dictionary<int, TrapInfo> dict = new();

    public List<TrapInfo> All => data;

    public void OnEnable()
    {
        dict = data.ToDictionary(row => row.Id, row => row);
    }

    public TrapInfo GetById(int id) => dict.TryGetValue(id, out var result) ? result : null;
}
