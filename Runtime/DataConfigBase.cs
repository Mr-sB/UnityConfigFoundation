using System.Collections.Generic;
using UnityEngine;

namespace GameUtil.Config
{
    public class DataConfigBase<T> : ScriptableObject where T : new()
    {
        public List<T> Data;
    }
}