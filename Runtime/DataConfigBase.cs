using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtil.Config
{
    public interface IDataConfig
    {
        void Init();
        void Assign(IEnumerable enumerable);
        void Add(object item);
    }
    
    public class DataConfigBase<T> : ScriptableObject, IDataConfig
    {
        public List<T> Data;
        
        public void Add(T item)
        {
            Data.Add(item);
        }

        public void Init()
        {
            if (Data == null)
                Data = new List<T>();
        }

        public void Assign(IEnumerable enumerable)
        {
            if (enumerable is IEnumerable<T> enumerableT)
            {
                if (enumerableT is List<T> list)
                    Data = list;
                else
                {
                    Init();
                    Data.Clear();
                    Data.AddRange(enumerableT);
                }
            }
            else
                Debug.LogFormat("Can not assign enumerable: {0}, type: {1}, to List<{2}> Data!", enumerable, enumerable.GetType(), typeof(T));
        }

        public void Add(object item)
        {
            if (item is T tItem)
                Data.Add(tItem);
            else
                Debug.LogFormat("Can not add item: {0}, type: {1}, to List<{2}> Data!", item, item.GetType(), typeof(T));
        }
    }
}