using System;
using System.Text;
using UnityEngine;

namespace Config
{
    /// <summary>
    /// csv与class文件互相转换
    /// </summary>
    public static class CSVGenerator
    {
        private static readonly string ClassHeadTemplate = "using System;" + Environment.NewLine
                                                           + "using System.Collections.Generic;" + Environment.NewLine
                                                           + "using UnityEngine;" + Environment.NewLine
                                                           + Environment.NewLine;

        private static readonly string ClassDefineWithNameSpace = "namespace {0}" + Environment.NewLine
                                                                  + "{{" + Environment.NewLine
                                                                  + "\t[Serializable]" + Environment.NewLine
                                                                  + "\tpublic class {1}" + Environment.NewLine
                                                                  + "\t{{" + Environment.NewLine
                                                                  + "{2}" + Environment.NewLine
                                                                  + "\t}}" + Environment.NewLine
                                                                  + "}}" + Environment.NewLine;

        private static readonly string ClassDefineWithoutNameSpace = "[Serializable]" + Environment.NewLine
                                                                     + "public class {0}" + Environment.NewLine
                                                                     + "{{" + Environment.NewLine
                                                                     + "{1}" + Environment.NewLine
                                                                     + "}}" + Environment.NewLine;

        public static string CSV2Class(string csvContent, string namespaceName, string className)
        {
            bool hasNameSpace = !string.IsNullOrWhiteSpace(namespaceName);
            string space = hasNameSpace ? "\t\t" : "\t";
            var table = CSVTable.GetCSVTable(csvContent);
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = table.FieldNames.Length; i < len; i++)
            {
                sb.Append(space);
                sb.Append("public ");
                sb.Append(table.FieldTypes[i]);
                sb.Append(" ");
                sb.Append(table.FieldNames[i]);
                sb.Append(";");
                if(i < len - 1)
                    sb.Append(Environment.NewLine);
            }

            if (hasNameSpace)
                return ClassHeadTemplate + string.Format(ClassDefineWithNameSpace, namespaceName, className, sb);
            return ClassHeadTemplate + string.Format(ClassDefineWithoutNameSpace, className, sb);
        }
        
        public static string Class2CSV<T>()
        {
            return Class2CSV(typeof(T));
        }

        public static string Class2CSV(string classNameWithNamespace)
        {
            return Class2CSV(Type.GetType(classNameWithNamespace));
        }

        public static string Class2CSV(Type type)
        {
            if (type == null)
            {
                Debug.LogError("Type is null!");
                return string.Empty;
            }

            StringBuilder fieldNames = new StringBuilder();
            StringBuilder fieldTypes = new StringBuilder();
            foreach (var fieldInfo in type.GetFields())
            {
                if (fieldNames.Length != 0)
                    fieldNames.Append(",");
                fieldNames.Append(fieldInfo.Name);

                
                if (fieldTypes.Length != 0)
                    fieldTypes.Append(",");
                fieldTypes.Append(fieldInfo.FieldType.Name);
            }

            if (fieldNames.Length != 0)
                fieldNames.Append(Environment.NewLine);
            if (fieldTypes.Length != 0)
                fieldTypes.Append(Environment.NewLine);
            return fieldNames.ToString() + fieldTypes;
        }
    }
}