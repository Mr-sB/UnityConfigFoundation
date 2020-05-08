using System;
using System.Collections.Generic;
using UnityEngine;

namespace Config.Convert
{
    /// <summary>
    /// csv单元格数据转换为字段对象
    /// </summary>
    public static class FieldConverter
    {
        private static Dictionary<Type, Func<string, object>> mConverters;

        private static void Init()
        {
            if(mConverters != null) return;
            //注册转换函数
            mConverters = new Dictionary<Type, Func<string, object>>
            {
                //--------------单个数据转换--------------
                {typeof(char), fieldContent => fieldContent.CharConverter()},
                {typeof(string), StringConverter},
                {typeof(bool), fieldContent => fieldContent.BoolConverter()},
                {typeof(byte), fieldContent => fieldContent.ByteConverter()},
                {typeof(short), fieldContent => fieldContent.ShortConverter()},
                {typeof(int), fieldContent => fieldContent.IntConverter()},
                {typeof(float), fieldContent => fieldContent.FloatConverter()},
                {typeof(double), fieldContent => fieldContent.DoubleConverter()},
                {typeof(Vector2), fieldContent => fieldContent.Vector2Converter()},
                {typeof(Vector3), fieldContent => fieldContent.Vector3Converter()},
                {typeof(Vector4), fieldContent => fieldContent.Vector4Converter()},
                {typeof(Vector2Int), fieldContent => fieldContent.Vector2IntConverter()},
                {typeof(Vector3Int), fieldContent => fieldContent.Vector3IntConverter()},
                {typeof(Quaternion), fieldContent => fieldContent.QuaternionConverter()},
                {typeof(Color), fieldContent => fieldContent.ColorConverter()},
                //--------------数组数据转换--------------
                {typeof(char[]), CharArrayConverter},
                {typeof(string[]), StringArrayConverter},
                {typeof(bool[]), BoolArrayConverter},
                {typeof(byte[]), ByteArrayConverter},
                {typeof(short[]), ShortArrayConverter},
                {typeof(int[]), IntArrayConverter},
                {typeof(float[]), FloatArrayConverter},
                {typeof(double[]), DoubleArrayConverter},
                {typeof(Vector2[]), Vector2ArrayConverter},
                {typeof(Vector3[]), Vector3ArrayConverter},
                {typeof(Vector4[]), Vector4ArrayConverter},
                {typeof(Vector2Int[]), Vector2IntArrayConverter},
                {typeof(Vector3Int[]), Vector3IntArrayConverter},
                {typeof(Quaternion[]), QuaternionArrayConverter},
                {typeof(Color[]), ColorArrayConverter}
            };
        }

        public static void Dispose()
        {
            if(mConverters == null) return;
            mConverters.Clear();
            mConverters = null;
        }

        public static object Convert(Type fieldType, string fieldContent)
        {
            //初始化
            if(mConverters == null)
                Init();
            if (mConverters.TryGetValue(fieldType, out var converter))
                return converter(fieldContent);
            if (fieldType.IsEnum)
                return fieldContent.IntConverter();
            Debug.LogError(string.Format("Can not convert {0} type data!", fieldType));
            return null;
        }
        
        public static T Convert<T>(string fieldContent)
        {
            return (T)Convert(typeof(T), fieldContent);
        }

        
        #region SingleDataConverters
        public static char CharConverter(this string fieldContent)
        {
            return string.IsNullOrEmpty(fieldContent) ? ' ' : fieldContent[0];
        }
        
        public static string StringConverter(this string fieldContent)
        {
            return fieldContent;
        }
        
        /// <param name="fieldContent">可以是整数(大于0为true,其余为false)/TRUE/True/true等</param>
        public static bool BoolConverter(this string fieldContent)
        {
            if (string.IsNullOrEmpty(fieldContent)) return false;
            //数字转换
            if (char.IsDigit(fieldContent[0]) || fieldContent[0] == '-' || fieldContent[0] == '+')
                return fieldContent.IntConverter() > 0;
            //字符串转换
            return bool.TryParse(fieldContent, out var result) && result;
        }
        
        public static byte ByteConverter(this string fieldContent)
        {
            return byte.TryParse(fieldContent, out var result) ? result : (byte)0;
        }
        
        public static short ShortConverter(this string fieldContent)
        {
            return short.TryParse(fieldContent, out var result) ? result : (short)0;
        }
        
        public static int IntConverter(this string fieldContent)
        {
            return int.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        public static float FloatConverter(this string fieldContent)
        {
            return float.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        public static double DoubleConverter(this string fieldContent)
        {
            return double.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        public static Vector2 Vector2Converter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector2 vector2 = new Vector2();
            for (int i = 0, len = Mathf.Min(strs.Length, 2); i < len; i++)
                vector2[i] = strs[i].FloatConverter();
            return vector2;
        }

        public static Vector3 Vector3Converter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector3 vector3 = new Vector3();
            for (int i = 0, len = Mathf.Min(strs.Length, 3); i < len; i++)
                vector3[i] = strs[i].FloatConverter();
            return vector3;
        }

        public static Vector4 Vector4Converter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector4 vector4 = new Vector4();
            for (int i = 0, len = Mathf.Min(strs.Length, 4); i < len; i++)
                vector4[i] = strs[i].FloatConverter();
            return vector4;
        }
        
        public static Vector2Int Vector2IntConverter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector2Int vector2Int = new Vector2Int();
            for (int i = 0, len = Mathf.Min(strs.Length, 2); i < len; i++)
                vector2Int[i] = strs[i].IntConverter();
            return vector2Int;
        }
        
        public static Vector3Int Vector3IntConverter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector3Int vector3Int = new Vector3Int();
            for (int i = 0, len = Mathf.Min(strs.Length, 3); i < len; i++)
                vector3Int[i] = strs[i].IntConverter();
            return vector3Int;
        }

        public static Quaternion QuaternionConverter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Quaternion quaternion = new Quaternion();
            for (int i = 0, len = Mathf.Min(strs.Length, 4); i < len; i++)
                quaternion[i] = strs[i].FloatConverter();
            return quaternion;
        }
        
        /// <param name="fieldContent">可以是颜色字符串([#]ffffff)或者0-255的字节数据</param>
        public static Color ColorConverter(this string fieldContent)
        {
            Color color = Color.black;
            //Color32形式(255;255;255) 或者 单独的数字形式(255)
            if (fieldContent.Contains(";") || fieldContent.Length <= 3)
            {
                var strs = FieldSplit(fieldContent);
                for (int i = 0, len = Mathf.Min(strs.Length, 4); i < len; i++)
                    color[i] = strs[i].ByteConverter() / (float)255;
                return color;
            }
            if(string.IsNullOrEmpty(fieldContent)) return color;
            if (fieldContent[0] != '#') fieldContent = '#' + fieldContent;
            return ColorUtility.TryParseHtmlString(fieldContent, out var result) ? result : color;
        }
        #endregion

        
        #region ArrayDataConverters
        public static char[] CharArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, CharConverter);
        }
        
        public static string[] StringArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, StringConverter);
        }
        
        public static bool[] BoolArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, BoolConverter);
        }
        
        public static byte[] ByteArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, ByteConverter);
        }
        
        public static short[] ShortArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, ShortConverter);
        }
        
        public static int[] IntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, IntConverter);
        }
        
        public static float[] FloatArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, FloatConverter);
        }
        
        public static double[] DoubleArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, DoubleConverter);
        }
        
        public static Vector2[] Vector2ArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector2Converter);
        }

        public static Vector3[] Vector3ArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector3Converter);
        }

        public static Vector4[] Vector4ArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector4Converter);
        }
        
        public static Vector2Int[] Vector2IntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector2IntConverter);
        }

        public static Vector3Int[] Vector3IntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector3IntConverter);
        }

        public static Quaternion[] QuaternionArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, QuaternionConverter);
        }

        public static Color[] ColorArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, ColorConverter);
        }
        #endregion

        /// <summary>
        /// 数组转换
        /// </summary>
        private static T[] ArrayConverter<T>(string fieldContent, Func<string, T> converter)
        {
            var strs = ArraySplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            T[] dataArray = new T[len];
            for (int i = 0; i < len; i++)
                dataArray[i] = converter(strs[i]);
            return dataArray;
        }
        
        /// <summary>
        /// 字段分割用;
        /// </summary>
        private static string[] FieldSplit(string fieldContent)
        {
            return fieldContent.Split(';');
        }
        
        /// <summary>
        /// 数组分割用|
        /// </summary>
        private static string[] ArraySplit(string fieldContent)
        {
            return fieldContent.Split('|');
        }
    }
}