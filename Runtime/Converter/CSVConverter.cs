using System;
using System.Collections.Generic;
using System.Reflection;
using TinyCSV;
using UnityEngine;

namespace GameUtil.Config
{
    /// <summary>
    /// csv表格数据转换为实例对象
    /// </summary>
    public static class CSVConverter
    {
        public static List<T> Convert<T>(string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true) where T : new()
        {
            CSVTableReader csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            List<T> dataList = new List<T>(csvTableReader.Records.Length);
            if (FieldConverter.CanConvert(typeof(T)))
            {
                foreach (var record in csvTableReader.Records)
                    dataList.Add(FieldConverter.Convert<T>(record.CellArray[0]));
            }
            else
            {
                var fieldInfos = InitFieldInfos<T>(csvTableReader);
                foreach (var data in GetEnumerator<T>(csvTableReader.Records, fieldInfos))
                    dataList.Add(data);
            }
            return dataList;
        }
        
        public static IEnumerable<T> ConvertEnumerator<T>(string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true) where T : new()
        {
            CSVTableReader csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            if (FieldConverter.CanConvert(typeof(T)))
            {
                foreach (var record in csvTableReader.Records)
                    yield return FieldConverter.Convert<T>(record.CellArray[0]);
            }
            else
            {
                var fieldInfos = InitFieldInfos<T>(csvTableReader);
                foreach (var data in GetEnumerator<T>(csvTableReader.Records, fieldInfos))
                    yield return data;
            }
        }
        
        public static List<T> ConvertNonPublicCtor<T>(string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            CSVTableReader csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            List<T> dataList = new List<T>(csvTableReader.Records.Length);
            if (FieldConverter.CanConvert(typeof(T)))
            {
                foreach (var record in csvTableReader.Records)
                    dataList.Add(FieldConverter.Convert<T>(record.CellArray[0]));
            }
            else
            {
                var fieldInfos = InitFieldInfos<T>(csvTableReader);
                foreach (var data in GetEnumeratorNonPublicCtor<T>(csvTableReader.Records, fieldInfos))
                    dataList.Add(data);
            }
            return dataList;
        }
        
        public static IEnumerable<T> ConvertEnumeratorNonPublicCtor<T>(string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            CSVTableReader csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            if (FieldConverter.CanConvert(typeof(T)))
            {
                foreach (var record in csvTableReader.Records)
                    yield return FieldConverter.Convert<T>(record.CellArray[0]);
            }
            else
            {
                var fieldInfos = InitFieldInfos<T>(csvTableReader);
                foreach (var data in GetEnumeratorNonPublicCtor<T>(csvTableReader.Records, fieldInfos))
                    yield return data;
            }
        }
        
        private static FieldInfo[] InitFieldInfos<T>(CSVTableReader csvTableReader)
        {
            //生成所有的字段信息
            int fieldLen = csvTableReader.Column;
            FieldInfo[] fieldInfos = new FieldInfo[fieldLen];
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
            return fieldInfos;
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
        
        private static IEnumerable<T> GetEnumeratorNonPublicCtor<T>(CSVRecordReader[] records, FieldInfo[] fieldInfos)
        {
            if(records == null || records.Length == 0 || fieldInfos == null || fieldInfos.Length == 0) yield break;
            int fieldLen = fieldInfos.Length;
            foreach (var record in records)
            {
                var cellArray = record.CellArray;
                T data;
                bool generated = false;
                //生成单个数据对象
                try
                {
                    data = Activator.CreateInstance<T>();
                    generated = true;
                }
                catch (Exception e)
                {
                    data = default;
                    Debug.LogError(e);
                }
                if (generated)
                {
                    for (int j = 0, len = Mathf.Min(fieldLen, cellArray.Length); j < len; j++)
                    {
                        var fieldInfo = fieldInfos[j];
                        if (fieldInfo == null) continue;
                        //根据fieldInfo.FieldType确定数据类型，根据
                        fieldInfo.SetValue(data, FieldConverter.Convert(fieldInfo.FieldType, cellArray[j]));
                    }
                }
                yield return data;
            }
        }
    }
}
