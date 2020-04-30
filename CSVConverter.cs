using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Config.Convert
{
    /// <summary>
    /// csv表格数据转换为实例对象
    /// </summary>
    public static class CSVConverter
    {
        public static T[] Convert<T>(string csvContent) where T : new()
        {
            if (!TryInit<T>(csvContent, out var rows, out var fieldInfos)) return null;
            T[] dataArray = new T[rows.Length - 2];
            int index = 0;
            foreach (var data in GetEnumerator<T>(rows, fieldInfos))
            {
                dataArray[index] = data;
                index++;
            }
            return dataArray;
        }
        
        public static IEnumerable<T> ConvertNonAlloc<T>(string csvContent) where T : new()
        {
            if (!TryInit<T>(csvContent, out var rows, out var fieldInfos)) yield break;
            foreach (var data in GetEnumerator<T>(rows, fieldInfos))
                yield return data;
        }

        private static bool TryInit<T>(string csvContent, out string[] rows, out FieldInfo[] fieldInfos)
        {
            rows = CSVTable.GetCSVRows(csvContent);
            fieldInfos = null;
            if (rows.Length < 2) return false;
            //字段名称
            string[] fieldNames = rows[0].Split(',');

            //生成所有的字段信息
            int fieldLen = fieldNames.Length;
            fieldInfos = new FieldInfo[fieldLen];
            Type fieldType = typeof(T);
            for (int i = 0; i < fieldLen; i++)
            {
                try
                {
                    fieldInfos[i] = fieldType.GetField(fieldNames[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError("fieldName Exception:" + fieldNames[i]);
                    Debug.LogError(e);
                }
            }
            return true;
        }

        private static IEnumerable<T> GetEnumerator<T>(string[] rows, FieldInfo[] fieldInfos) where T : new()
        {
            if(rows == null || rows.Length == 0 || fieldInfos == null || fieldInfos.Length == 0) yield break;
            int fieldLen = fieldInfos.Length;
            //遍历所有数据行 从2开始。0为字段名，1为字段类型或者备注
            for (int i = 2, rowLen = rows.Length; i < rowLen; i++)
            {
                //分割数据行
                string[] fieldContents = rows[i].Split(',');
                //生成单个数据对象
                T data = new T();
                for (int j = 0; j < fieldLen; j++)
                {
                    var fieldInfo = fieldInfos[j];
                    if(fieldInfo == null) continue;
                    //根据fieldInfo.FieldType确定数据类型，根据
                    fieldInfo.SetValue(data, FieldConverter.Convert(fieldInfo.FieldType, fieldContents[j]));
                }
                yield return data;
            }
        }
    }
}