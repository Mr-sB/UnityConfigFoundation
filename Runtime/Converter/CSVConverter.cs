using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// Convert all table data.
        /// </summary>
        public static List<T> Convert<T>(string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true) where T : new()
        {
            if (!TryInitConvert<T>(csvContent, cellSeparator, supportCellMultiline, out var csvTableReader, out var fieldInfos)) return null;
            List<T> dataList = new List<T>(csvTableReader.RecordRow);
            foreach (var data in GetEnumerator<T>(csvTableReader.Records, fieldInfos))
                dataList.Add(data);
            return dataList;
        }
        
        /// <summary>
        /// Convert all table data.
        /// </summary>
        public static IEnumerable<T> ConvertEnumerator<T>(string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true) where T : new()
        {
            if (!TryInitConvert<T>(csvContent, cellSeparator, supportCellMultiline, out var csvTableReader, out var fieldInfos)) yield break;
            foreach (var data in GetEnumerator<T>(csvTableReader.Records, fieldInfos))
                yield return data;
        }

        /// <summary>
        /// Convert single column data by header name.
        /// </summary>
        public static List<T> ConvertColumn<T>(string csvContent, string headerName, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (!TryInitConvertColumnByName<T>(csvContent, headerName, cellSeparator, supportCellMultiline, out var csvTableReader)) return null;
            List<T> dataList = new List<T>(csvTableReader.RecordRow);
            foreach (var record in csvTableReader.Records)
                dataList.Add(FieldConverter.Convert<T>(record.CellDict[headerName]));
            return dataList;
        }
        
        /// <summary>
        /// Convert single column data by header name.
        /// </summary>
        public static IEnumerable<T> ConvertColumnEnumerator<T>(string csvContent, string headerName, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (!TryInitConvertColumnByName<T>(csvContent, headerName, cellSeparator, supportCellMultiline, out var csvTableReader)) yield break;
            foreach (var record in csvTableReader.Records)
                yield return FieldConverter.Convert<T>(record.CellDict[headerName]);
        }
        
        /// <summary>
        /// Convert single column data by header index.
        /// </summary>
        public static List<T> ConvertColumn<T>(string csvContent, int headerIndex = 0, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (headerIndex < 0) headerIndex = 0;
            if (!TryInitConvertColumnByIndex<T>(csvContent, headerIndex, cellSeparator, supportCellMultiline, out var csvTableReader)) return null;
            List<T> dataList = new List<T>(csvTableReader.RecordRow);
            foreach (var record in csvTableReader.Records)
                dataList.Add(headerIndex < record.CellArray.Length ? FieldConverter.Convert<T>(record.CellArray[headerIndex]) : default);
            return dataList;
        }
        
        /// <summary>
        /// Convert single column data by header index.
        /// </summary>
        public static IEnumerable<T> ConvertColumnEnumerator<T>(string csvContent, int headerIndex = 0, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (headerIndex < 0) headerIndex = 0;
            if (!TryInitConvertColumnByIndex<T>(csvContent, headerIndex, cellSeparator, supportCellMultiline, out var csvTableReader)) yield break;
            foreach (var record in csvTableReader.Records)
                yield return headerIndex < record.CellArray.Length ? FieldConverter.Convert<T>(record.CellArray[headerIndex]) : default;
        }

        private static bool TryInitConvert<T>(string csvContent, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader, out FieldInfo[] fieldInfos)
        {
            csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
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
            if (typeof(T).IsClass)
            {
                foreach (var record in records)
                {
                    var cellArray = record.CellArray;
                    //生成单个数据对象
                    T data = new T();
                    for (int j = 0, len = Mathf.Min(fieldLen, cellArray.Length); j < len; j++)
                    {
                        var fieldInfo = fieldInfos[j];
                        if(fieldInfo == null) continue;
                        //根据fieldInfo.FieldType确定数据类型
                        fieldInfo.SetValue(data, FieldConverter.Convert(fieldInfo.FieldType, cellArray[j]));
                    }
                    yield return data;
                }
            }
            else
            {
                foreach (var record in records)
                {
                    var cellArray = record.CellArray;
                    //生成单个数据对象 使用object装箱struct，否则SetValue更改的是data拷贝对象的值
                    object data = new T();
                    for (int j = 0, len = Mathf.Min(fieldLen, cellArray.Length); j < len; j++)
                    {
                        var fieldInfo = fieldInfos[j];
                        if(fieldInfo == null) continue;
                        //根据fieldInfo.FieldType确定数据类型
                        fieldInfo.SetValue(data, FieldConverter.Convert(fieldInfo.FieldType, cellArray[j]));
                    }
                    yield return (T)data;
                }
            }
        }

        private static bool TryInitConvertColumnByName<T>(string csvContent, string headerName, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader)
        {
            csvTableReader = null;
            if (!FieldConverter.CanConvert(typeof(T)))
            {
                Debug.LogError($"ConvertColumn Exception: Type {typeof(T).AssemblyQualifiedName} can not convert!");
                return false;
            }
            csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            if (!csvTableReader.Headers.Contains(headerName))
            {
                Debug.LogError($"ConvertColumn Exception: Header {headerName} does not exist!");
                return false;
            }
            return true;
        }
        
        private static bool TryInitConvertColumnByIndex<T>(string csvContent, int headerIndex, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader)
        {
            csvTableReader = null;
            if (!FieldConverter.CanConvert(typeof(T)))
            {
                Debug.LogError($"ConvertColumn Exception: Type {typeof(T).AssemblyQualifiedName} can not convert!");
                return false;
            }
            csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            if (csvTableReader.Column <= headerIndex)
            {
                Debug.LogError($"ConvertColumn Exception: headerIndex {headerIndex} is out of range! Table column is {csvTableReader.Column}.");
                return false;
            }
            return true;
        }
    }
}