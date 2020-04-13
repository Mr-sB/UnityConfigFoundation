using System;
using System.Text;
using UnityEngine;

namespace Config
{
    /// <summary>
    /// csv与class文件互相转换
    /// </summary>
    public static class CVSGenerator
    {
        private static readonly string ClassTemplate = "using System;" + Environment.NewLine
                                                       + "using System.Collections.Generic;" + Environment.NewLine
                                                       + "using UnityEngine;" + Environment.NewLine
                                                       + Environment.NewLine
                                                       + "namespace Config.Data" + Environment.NewLine
                                                       + "{{" + Environment.NewLine
                                                       + "\tpublic class {0}" + Environment.NewLine
                                                       + "\t{{" + Environment.NewLine
                                                       + "{1}" + Environment.NewLine
                                                       + "\t}}" + Environment.NewLine
                                                       + "}}" + Environment.NewLine;
        
        public static string CSV2Class(string className, string csvContent)
        {
            var table = CSVTable.GetCSVTable(csvContent);
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = table.FieldNames.Length; i < len; i++)
            {
                sb.Append("\t\tpublic ");
                sb.Append(table.FieldTypes[i]);
                sb.Append(" ");
                sb.Append(table.FieldNames[i]);
                sb.Append(";");
                sb.Append(Environment.NewLine);
            }
            return string.Format(ClassTemplate, className, sb);
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