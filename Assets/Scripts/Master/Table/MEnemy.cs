using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Master
{
    [CreateAssetMenu(menuName = "Master/MEnemy")]
    public class MEnemy : ScriptableObject
    {
        [SerializeField]
        private List<EnemyInfo> data;

        private Dictionary<int, EnemyInfo> dictionary = new();

        public List<EnemyInfo> All => data;

        public void OnEnable()
        {
            dictionary = data.ToDictionary(enemy => enemy.Id, enemy => enemy);
        }

        public EnemyInfo GetById(int id)
        {
            if (dictionary.TryGetValue(id, out var result)) return result;
            return null;
        }
    }
}
