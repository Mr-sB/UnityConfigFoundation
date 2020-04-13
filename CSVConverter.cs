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
            string[] rows = CSVTable.GetCSVRows(csvContent);
            if (rows.Length < 2) return null;
            //字段名称
            string[] fieldNames = rows[0].Split(',');

            //生成所有的字段信息
            int fieldLen = fieldNames.Length;
            FieldInfo[] fieldInfos = new FieldInfo[fieldLen];
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

            int rowLen = rows.Length;
            T[] dataArray = new T[rowLen - 2];
            //遍历所有数据行 从2开始。0位字段名，1为字段类型或者备注
            for (int i = 2; i < rowLen; i++)
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
                dataArray[i - 2] = data;
            }
            return dataArray;
        }
        
        public static IEnumerable<T> ConvertNonAlloc<T>(string csvContent) where T : new()
        {
            string[] rows = CSVTable.GetCSVRows(csvContent);
            if (rows.Length < 2) yield break;
            //字段名称
            string[] fieldNames = rows[0].Split(',');

            //生成所有的字段信息
            int fieldLen = fieldNames.Length;
            FieldInfo[] fieldInfos = new FieldInfo[fieldLen];
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

            //遍历所有数据行 从2开始。0位字段名，1为字段类型或者备注
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