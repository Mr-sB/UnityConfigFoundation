using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GameUtil.Config
{
    /// <summary>
    /// csv单元格数据转换为字段对象
    /// </summary>
    public static partial class FieldConverter
    {
        private static Dictionary<Type, MethodInfo> mConverters;
        private static readonly Type mVoidType = typeof(void);
        private static readonly Type mStringType = typeof(string);
        private static readonly Type mListGenericType = typeof(List<>);
        private static readonly string mValueTupleNameStart = nameof(ValueTuple) + '`';

        private static void Init()
        {
            if(mConverters != null) return;
            //注册转换函数
            mConverters = new Dictionary<Type, MethodInfo>();
            foreach (var methodInfo in typeof(FieldConverter).GetMethods(BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic))
            {
                if(methodInfo.GetCustomAttribute<FieldConverterAttribute>() == null) continue;
                var convertType = methodInfo.ReturnType;
                if (convertType == mVoidType)
                {
                    Debug.LogError($"FieldConverter method's return type can not be void! MethodInfo: {methodInfo}");
                    continue;
                }
                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != mStringType)
                {
                    Debug.LogError($"FieldConverter method's parameters can be only one string! MethodInfo: {methodInfo}");
                    continue;
                }
                if (mConverters.ContainsKey(convertType))
                {
                    Debug.LogError($"The same return type method has been added! MethodInfo: {methodInfo}");
                    continue;
                }
                mConverters.Add(convertType, methodInfo);
            }
        }

        public static void Dispose()
        {
            if(mConverters == null) return;
            mConverters.Clear();
            mConverters = null;
        }

        public static bool CanConvert(Type fieldType)
        {
            //初始化
            if(mConverters == null)
                Init();
            //Custom Enum
            if (mConverters.ContainsKey(fieldType) || fieldType.IsEnum)
                return true;
            //One dimensional array
            if (fieldType.IsArray && fieldType.GetArrayRank() == 1 && CanConvert(fieldType.GetElementType()))
                return true;
            //List
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == mListGenericType && CanConvert(fieldType.GetGenericArguments()[0]))
                return true;
            //ValueTuple
            if (fieldType.Name.StartsWith(mValueTupleNameStart) && !fieldType.IsArray)
            {
                foreach (var genericArgument in fieldType.GetGenericArguments())
                    if (!CanConvert(genericArgument)) return false;
                return true;
            }
            return false;
        }
        
        public static object Convert(Type fieldType, string fieldContent)
        {
            //Check
            if (!CanConvert(fieldType))
            {
                Debug.LogError($"Can not convert {fieldType} type data!");
                return null;
            }
            //Custom
            if (mConverters.TryGetValue(fieldType, out var converter))
            {
                try
                {
                    return converter.Invoke(null, new object[] {fieldContent});
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return null;
                }
            }
            //Enum
            if (fieldType.IsEnum)
                return fieldContent.EnumConverter(fieldType);
            //One dimensional array
            if (fieldType.IsArray && fieldType.GetArrayRank() == 1)
                return fieldContent.ArrayConverter(fieldType.GetElementType());
            //List
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == mListGenericType)
                return fieldContent.ListConverter(fieldType.GetGenericArguments()[0]);
            //ValueTuple                
            if (fieldType.Name.StartsWith(mValueTupleNameStart) && !fieldType.IsArray)
                return fieldContent.ValueTupleConverter(fieldType);

            Debug.LogError($"Can not convert {fieldType} type data!");
            return null;
        }
        
        public static T Convert<T>(string fieldContent)
        {
            return (T)Convert(typeof(T), fieldContent);
        }

        #region SingleDataConverters
        public static object EnumConverter(this string fieldContent, Type fieldType)
        {
            if (int.TryParse(fieldContent, out var value)) return value;
            try
            {
                return Enum.Parse(fieldType, fieldContent);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return 0;
            }
        }
        
        public static object ValueTupleConverter(this string fieldContent, Type fieldType)
        {
            //Get GenericArguments
            Type[] genericArguments = fieldType.GetGenericArguments();
            int argsLength = genericArguments.Length;
            //Get Create method
            var createMethodInfo = typeof(ValueTuple).GetMethods().First(info => info.Name == nameof(ValueTuple.Create) && info.GetParameters().Length == argsLength)?.MakeGenericMethod(genericArguments);
            if (createMethodInfo == null)
            {
                Debug.LogError($"Can not find ValueTuple.Create method with {argsLength} arguments.");
                return Activator.CreateInstance(fieldType);
            }

            //Create arguments
            string[] strs = FieldSplit(fieldContent);
            object[] args = new object[argsLength];
            for (int i = 0, len = Mathf.Min(argsLength, strs.Length); i < len; i++)
                args[i] = Convert(genericArguments[i], strs[i]);
            //Invoke Create method
            try
            {
                return createMethodInfo.Invoke(null, args);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return Activator.CreateInstance(fieldType);
            }
        }
        
        public static T ValueTupleConverter<T>(this string fieldContent)
        {
            var fieldType = typeof(T);
            if (fieldType.Name.StartsWith(mValueTupleNameStart) && !fieldType.IsArray)
                return (T) ValueTupleConverter(fieldContent, typeof(T));
            return default;
        }
        
        [FieldConverter]
        public static char CharConverter(this string fieldContent)
        {
            return string.IsNullOrEmpty(fieldContent) ? ' ' : fieldContent[0];
        }
        
        [FieldConverter]
        public static string StringConverter(this string fieldContent)
        {
            return fieldContent;
        }
        
        /// <param name="fieldContent">可以是整数(大于0为true,其余为false)/TRUE/True/true等</param>
        [FieldConverter]
        public static bool BoolConverter(this string fieldContent)
        {
            if (string.IsNullOrEmpty(fieldContent)) return false;
            //数字转换
            if (char.IsDigit(fieldContent[0]) || fieldContent[0] == '-' || fieldContent[0] == '+')
                return fieldContent.IntConverter() > 0;
            //字符串转换
            return bool.TryParse(fieldContent, out var result) && result;
        }
        
        [FieldConverter]
        public static byte ByteConverter(this string fieldContent)
        {
            return byte.TryParse(fieldContent, out var result) ? result : (byte)0;
        }
        
        [FieldConverter]
        public static short ShortConverter(this string fieldContent)
        {
            return short.TryParse(fieldContent, out var result) ? result : (short)0;
        }
        
        [FieldConverter]
        public static int IntConverter(this string fieldContent)
        {
            return int.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        [FieldConverter]
        public static uint UIntConverter(this string fieldContent)
        {
            return uint.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        [FieldConverter]
        public static long LongConverter(this string fieldContent)
        {
            return long.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        [FieldConverter]
        public static float FloatConverter(this string fieldContent)
        {
            return float.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        [FieldConverter]
        public static double DoubleConverter(this string fieldContent)
        {
            return double.TryParse(fieldContent, out var result) ? result : 0;
        }
        
        [FieldConverter]
        public static Vector2 Vector2Converter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector2 vector2 = new Vector2();
            for (int i = 0, len = Mathf.Min(strs.Length, 2); i < len; i++)
                vector2[i] = strs[i].FloatConverter();
            return vector2;
        }

        [FieldConverter]
        public static Vector3 Vector3Converter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector3 vector3 = new Vector3();
            for (int i = 0, len = Mathf.Min(strs.Length, 3); i < len; i++)
                vector3[i] = strs[i].FloatConverter();
            return vector3;
        }

        [FieldConverter]
        public static Vector4 Vector4Converter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector4 vector4 = new Vector4();
            for (int i = 0, len = Mathf.Min(strs.Length, 4); i < len; i++)
                vector4[i] = strs[i].FloatConverter();
            return vector4;
        }
        
        [FieldConverter]
        public static Vector2Int Vector2IntConverter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector2Int vector2Int = new Vector2Int();
            for (int i = 0, len = Mathf.Min(strs.Length, 2); i < len; i++)
                vector2Int[i] = strs[i].IntConverter();
            return vector2Int;
        }
        
        [FieldConverter]
        public static Vector3Int Vector3IntConverter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Vector3Int vector3Int = new Vector3Int();
            for (int i = 0, len = Mathf.Min(strs.Length, 3); i < len; i++)
                vector3Int[i] = strs[i].IntConverter();
            return vector3Int;
        }

        [FieldConverter]
        public static Quaternion QuaternionConverter(this string fieldContent)
        {
            var strs = FieldSplit(fieldContent);
            Quaternion quaternion = new Quaternion();
            for (int i = 0, len = Mathf.Min(strs.Length, 4); i < len; i++)
                quaternion[i] = strs[i].FloatConverter();
            return quaternion;
        }
        
        /// <param name="fieldContent">可以是颜色字符串([#]ffffff)或者0-255的字节数据</param>
        [FieldConverter]
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
        public static Array ArrayConverter(this string fieldContent, Type elementType)
        {
            var strs = ArraySplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            Array dataArray = Array.CreateInstance(elementType, len);
            for (int i = 0; i < len; i++)
                dataArray.SetValue(Convert(elementType, strs[i]), i);
            return dataArray;
        }
        
        public static T[] ArrayConverter<T>(this string fieldContent)
        {
            var strs = ArraySplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            T[] dataArray = new T[len];
            for (int i = 0; i < len; i++)
                dataArray[i] = (T)Convert(typeof(T), strs[i]);
            return dataArray;
        }
        
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
        
        [FieldConverter]
        public static char[] CharArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, CharConverter);
        }
        
        [FieldConverter]
        public static string[] StringArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, StringConverter);
        }
        
        [FieldConverter]
        public static bool[] BoolArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, BoolConverter);
        }
        
        [FieldConverter]
        public static byte[] ByteArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, ByteConverter);
        }
        
        [FieldConverter]
        public static short[] ShortArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, ShortConverter);
        }
        
        [FieldConverter]
        public static int[] IntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, IntConverter);
        }
        
        [FieldConverter]
        public static uint[] UIntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, UIntConverter);
        }
        
        [FieldConverter]
        public static long[] LongArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, LongConverter);
        }
        
        [FieldConverter]
        public static float[] FloatArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, FloatConverter);
        }
        
        [FieldConverter]
        public static double[] DoubleArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, DoubleConverter);
        }
        
        [FieldConverter]
        public static Vector2[] Vector2ArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector2Converter);
        }

        [FieldConverter]
        public static Vector3[] Vector3ArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector3Converter);
        }

        [FieldConverter]
        public static Vector4[] Vector4ArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector4Converter);
        }
        
        [FieldConverter]
        public static Vector2Int[] Vector2IntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector2IntConverter);
        }

        [FieldConverter]
        public static Vector3Int[] Vector3IntArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, Vector3IntConverter);
        }

        [FieldConverter]
        public static Quaternion[] QuaternionArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, QuaternionConverter);
        }

        [FieldConverter]
        public static Color[] ColorArrayConverter(this string fieldContent)
        {
            return ArrayConverter(fieldContent, ColorConverter);
        }
        
        public static T[] ValueTupleArrayConverter<T>(this string fieldContent)
        {
            var strs = ArraySplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            T[] dataArray = new T[len];
            for (int i = 0; i < len; i++)
                dataArray[i] = ValueTupleConverter<T>(strs[i]);
            return dataArray;
        }
        #endregion
        
        #region ListDataConverters
        public static IList ListConverter(this string fieldContent, Type elementType)
        {
            var strs = ListSplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            IList dataList = Activator.CreateInstance(mListGenericType.MakeGenericType(elementType)) as IList;
            for (int i = 0; i < len; i++)
                dataList.Add(Convert(elementType, strs[i]));
            return dataList;
        }
        
        public static List<T> ListConverter<T>(this string fieldContent)
        {
            var strs = ListSplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            List<T> dataList = new List<T>(len);
            for (int i = 0; i < len; i++)
                dataList.Add((T)Convert(typeof(T), strs[i]));
            return dataList;
        }
        
        /// <summary>
        /// List转换
        /// </summary>
        private static List<T> ListConverter<T>(string fieldContent, Func<string, T> converter)
        {
            var strs = ListSplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            List<T> dataList = new List<T>(len);
            for (int i = 0; i < len; i++)
                dataList.Add(converter(strs[i]));
            return dataList;
        }
        
        [FieldConverter]
        public static List<char> CharListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, CharConverter);
        }
        
        [FieldConverter]
        public static List<string> StringListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, StringConverter);
        }
        
        [FieldConverter]
        public static List<bool> BoolListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, BoolConverter);
        }
        
        [FieldConverter]
        public static List<byte> ByteListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, ByteConverter);
        }
        
        [FieldConverter]
        public static List<short> ShortListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, ShortConverter);
        }
        
        [FieldConverter]
        public static List<int> IntListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, IntConverter);
        }
        
        [FieldConverter]
        public static List<uint> UIntListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, UIntConverter);
        }
        
        [FieldConverter]
        public static List<long> LongListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, LongConverter);
        }
        
        [FieldConverter]
        public static List<float> FloatListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, FloatConverter);
        }
        
        [FieldConverter]
        public static List<double> DoubleListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, DoubleConverter);
        }
        
        [FieldConverter]
        public static List<Vector2> Vector2ListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, Vector2Converter);
        }

        [FieldConverter]
        public static List<Vector3> Vector3ListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, Vector3Converter);
        }

        [FieldConverter]
        public static List<Vector4> Vector4ListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, Vector4Converter);
        }
        
        [FieldConverter]
        public static List<Vector2Int> Vector2IntListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, Vector2IntConverter);
        }

        [FieldConverter]
        public static List<Vector3Int> Vector3IntListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, Vector3IntConverter);
        }

        [FieldConverter]
        public static List<Quaternion> QuaternionListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, QuaternionConverter);
        }

        [FieldConverter]
        public static List<Color> ColorListConverter(this string fieldContent)
        {
            return ListConverter(fieldContent, ColorConverter);
        }
        
        public static List<T> ValueTupleListConverter<T>(this string fieldContent)
        {
            var strs = ListSplit(fieldContent);
            if (strs == null || strs.Length <= 0) return null;
            int len = strs.Length;
            List<T> dataList = new List<T>(len);
            for (int i = 0; i < len; i++)
                dataList.Add(ValueTupleConverter<T>(strs[i]));
            return dataList;
        }
        #endregion
        
        /// <summary>
        /// 字段分割用;
        /// </summary>
        private static string[] FieldSplit(string fieldContent)
        {
            return string.IsNullOrEmpty(fieldContent) ? new string[0] : fieldContent.Split(';');
        }
        
        /// <summary>
        /// 数组分割用|
        /// </summary>
        private static string[] ArraySplit(string fieldContent)
        {
            return string.IsNullOrEmpty(fieldContent) ? new string[0] : fieldContent.Split('|');
        }
        
        /// <summary>
        /// List分割用|
        /// </summary>
        private static string[] ListSplit(string fieldContent)
        {
            return string.IsNullOrEmpty(fieldContent) ? new string[0] : fieldContent.Split('|');
        }
    }
}