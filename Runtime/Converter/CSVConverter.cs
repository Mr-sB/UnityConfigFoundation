using System;
using System.Collections;
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
        #region Generic

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
            return TryInitConvert(typeof(T), csvContent, cellSeparator, supportCellMultiline, out csvTableReader, out fieldInfos);
        }

        private static IEnumerable<T> GetEnumerator<T>(CSVRecordReader[] records, FieldInfo[] fieldInfos) where T : new()
        {
            if(records == null || records.Length == 0 || fieldInfos == null || fieldInfos.Length == 0) yield break;
            int fieldLen = fieldInfos.Length;
            foreach (var record in records)
            {
                var cellArray = record.CellArray;
                //生成单个数据对象 使用object，如果是struct则会装箱，否则SetValue更改的是data拷贝对象的值
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

        private static bool TryInitConvertColumnByName<T>(string csvContent, string headerName, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader)
        {
            return TryInitConvertColumnByName(typeof(T), csvContent, headerName, cellSeparator, supportCellMultiline, out csvTableReader);
        }
        
        private static bool TryInitConvertColumnByIndex<T>(string csvContent, int headerIndex, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader)
        {
            return TryInitConvertColumnByIndex(typeof(T), csvContent, headerIndex, cellSeparator, supportCellMultiline, out csvTableReader);
        }

        #endregion


        #region Type

        /// <summary>
        /// Convert all table data.
        /// </summary>
        public static IList Convert(Type dataType, string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (!TryInitConvert(dataType, csvContent, cellSeparator, supportCellMultiline, out var csvTableReader, out var fieldInfos)) return null;
            var dataList = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType), csvTableReader.RecordRow) as IList;
            foreach (var data in GetEnumerator(dataType, csvTableReader.Records, fieldInfos))
                dataList.Add(data);
            return dataList;
        }
        
        /// <summary>
        /// Convert all table data.
        /// </summary>
        public static IEnumerable ConvertEnumerator(Type dataType, string csvContent, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (!TryInitConvert(dataType, csvContent, cellSeparator, supportCellMultiline, out var csvTableReader, out var fieldInfos)) yield break;
            foreach (var data in GetEnumerator(dataType, csvTableReader.Records, fieldInfos))
                yield return data;
        }

        /// <summary>
        /// Convert single column data by header name.
        /// </summary>
        public static IList ConvertColumn(Type dataType, string csvContent, string headerName, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (!TryInitConvertColumnByName(dataType, csvContent, headerName, cellSeparator, supportCellMultiline, out var csvTableReader)) return null;
            var dataList = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType), csvTableReader.RecordRow) as IList;
            foreach (var record in csvTableReader.Records)
                dataList.Add(FieldConverter.Convert(dataType, record.CellDict[headerName]));
            return dataList;
        }
        
        /// <summary>
        /// Convert single column data by header name.
        /// </summary>
        public static IEnumerable ConvertColumnEnumerator(Type dataType, string csvContent, string headerName, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (!TryInitConvertColumnByName(dataType, csvContent, headerName, cellSeparator, supportCellMultiline, out var csvTableReader)) yield break;
            foreach (var record in csvTableReader.Records)
                yield return FieldConverter.Convert(dataType, record.CellDict[headerName]);
        }
        
        /// <summary>
        /// Convert single column data by header index.
        /// </summary>
        public static IList ConvertColumn(Type dataType, string csvContent, int headerIndex = 0, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (headerIndex < 0) headerIndex = 0;
            if (!TryInitConvertColumnByIndex(dataType, csvContent, headerIndex, cellSeparator, supportCellMultiline, out var csvTableReader)) return null;
            var dataList = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType), csvTableReader.RecordRow) as IList;
            foreach (var record in csvTableReader.Records)
                dataList.Add(headerIndex < record.CellArray.Length
                    ? FieldConverter.Convert(dataType, record.CellArray[headerIndex])
                    : dataType.IsClass
                        ? null
                        : Activator.CreateInstance(dataType));
            return dataList;
        }
        
        /// <summary>
        /// Convert single column data by header index.
        /// </summary>
        public static IEnumerable ConvertColumnEnumerator(Type dataType, string csvContent, int headerIndex = 0, char cellSeparator = CSVDataHelper.CommaCharacter, bool supportCellMultiline = true)
        {
            if (headerIndex < 0) headerIndex = 0;
            if (!TryInitConvertColumnByIndex(dataType, csvContent, headerIndex, cellSeparator, supportCellMultiline, out var csvTableReader)) yield break;
            foreach (var record in csvTableReader.Records)
                yield return headerIndex < record.CellArray.Length
                    ? FieldConverter.Convert(dataType, record.CellArray[headerIndex])
                    : dataType.IsClass
                        ? null
                        : Activator.CreateInstance(dataType);
        }

        private static bool TryInitConvert(Type dataType, string csvContent, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader, out FieldInfo[] fieldInfos)
        {
            csvTableReader = new CSVTableReader(csvContent, cellSeparator, supportCellMultiline);
            fieldInfos = null;
            if (csvTableReader == null) return false;

            //生成所有的字段信息
            int fieldLen = csvTableReader.Column;
            fieldInfos = new FieldInfo[fieldLen];
            for (int i = 0; i < fieldLen; i++)
            {
                //Skip empty header column.
                if (string.IsNullOrEmpty(csvTableReader.Headers[i]))
                {
                    fieldInfos[i] = null;
                    continue;
                }
                try
                {
                    var fieldInfo = dataType.GetField(csvTableReader.Headers[i]);
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

        private static IEnumerable GetEnumerator(Type dataType, CSVRecordReader[] records, FieldInfo[] fieldInfos)
        {
            if(records == null || records.Length == 0 || fieldInfos == null || fieldInfos.Length == 0) yield break;
            int fieldLen = fieldInfos.Length;
            foreach (var record in records)
            {
                var cellArray = record.CellArray;
                //生成单个数据对象 使用object，如果是struct则会装箱，否则SetValue更改的是data拷贝对象的值
                object data = Activator.CreateInstance(dataType);
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

        private static bool TryInitConvertColumnByName(Type dataType, string csvContent, string headerName, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader)
        {
            csvTableReader = null;
            if (!FieldConverter.CanConvert(dataType))
            {
                Debug.LogError($"ConvertColumn Exception: Type {dataType.AssemblyQualifiedName} can not convert!");
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
        
        private static bool TryInitConvertColumnByIndex(Type dataType, string csvContent, int headerIndex, char cellSeparator, bool supportCellMultiline, out CSVTableReader csvTableReader)
        {
            csvTableReader = null;
            if (!FieldConverter.CanConvert(dataType))
            {
                Debug.LogError($"ConvertColumn Exception: Type {dataType.AssemblyQualifiedName} can not convert!");
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

        #endregion
    }
}