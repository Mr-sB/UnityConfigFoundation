using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameUtil.Config
{
    /// <summary>
    /// csv表格数据转换为实例对象
    /// </summary>
    public static class CSVConverter
    {
        public static List<T> Convert<T>(string csvContent) where T : new()
        {
            if (!TryInit<T>(csvContent, out var csvTable, out var fieldInfos)) return null;
            List<T> dataList = new List<T>(csvTable.Records.Length);
            foreach (var data in GetEnumerator<T>(csvTable.Records, fieldInfos))
                dataList.Add(data);
            return dataList;
        }
        
        public static IEnumerable<T> ConvertEnumerator<T>(string csvContent) where T : new()
        {
            if (!TryInit<T>(csvContent, out var csvTable, out var fieldInfos)) yield break;
            foreach (var data in GetEnumerator<T>(csvTable.Records, fieldInfos))
                yield return data;
        }

        private static bool TryInit<T>(string csvContent, out CSVTableReader csvTableReader, out FieldInfo[] fieldInfos)
        {
            csvTableReader = new CSVTableReader(csvContent);
            fieldInfos = null;
            if (csvTableReader == null) return false;

            //生成所有的字段信息
            int fieldLen = csvTableReader.Column;
            fieldInfos = new FieldInfo[fieldLen];
            Type fieldType = typeof(T);
            for (int i = 0; i < fieldLen; i++)
            {
                try
                {
                    var fieldInfo = fieldType.GetField(csvTableReader.Headers[i]);
                    fieldInfos[i] = fieldInfo;
                    if (fieldInfo == null)
                        Debug.LogError("fieldName Exception: field info is null! " + csvTableReader.Headers[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError("fieldName Exception. " + csvTableReader.Headers[i]);
                    Debug.LogError(e);
                }
            }
            return true;
        }

        private static IEnumerable<T> GetEnumerator<T>(CSVRecordReader[] records, FieldInfo[] fieldInfos) where T : new()
        {
            if(records == null || records.Length == 0 || fieldInfos == null || fieldInfos.Length == 0) yield break;
            int fieldLen = fieldInfos.Length;
            foreach (var record in records)
            {
                var cellArray = record.CellArray;
                //生成单个数据对象
                T data = new T();
                for (int j = 0, len = Mathf.Min(fieldLen, cellArray.Length); j < len; j++)
                {
                    var fieldInfo = fieldInfos[j];
                    if(fieldInfo == null) continue;
                    //根据fieldInfo.FieldType确定数据类型，根据
                    fieldInfo.SetValue(data, FieldConverter.Convert(fieldInfo.FieldType, cellArray[j]));
                }
                yield return data;
            }
        }
    }
}