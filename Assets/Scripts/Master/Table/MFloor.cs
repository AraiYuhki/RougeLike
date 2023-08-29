using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Xeon.Master
{
    [CreateAssetMenu(menuName = "Master/MFloor")]
    public class MFloor : ScriptableObject
    {
        [SerializeField]
        private List<FloorInfo> data;

        private Dictionary<int, FloorInfo> dictionary = new();
        private Dictionary<int, List<FloorInfo>> groups = new();
        public List<FloorInfo> All => data;

        public FloorInfo GetByeId(int id)
        {
            if (dictionary.TryGetValue(id, out var result)) return result;
            return null;
        }

        public List<FloorInfo> GetByDungeonId(int dungeonId)
        {
            if (groups.TryGetValue(dungeonId, out var result)) return result;
            return null;
        }

        public void OnEnable()
        {
            dictionary = data.ToDictionary(row => row.Id, row => row);
            foreach (var row in data)
            {
                if (!groups.ContainsKey(row.DungeonId))
                    groups.Add(row.DungeonId, new());
                groups[row.DungeonId].Add(row);
            }
        }
    }
}
