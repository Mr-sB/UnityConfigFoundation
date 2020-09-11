using System;
using System.Text;
using UnityEngine;

namespace GameUtil.Config
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
            var table = new CSVTableReader(csvContent);
            StringBuilder sb = new StringBuilder();
            for (int i = 0, column = table.Column; i < column; i++)
            {
                sb.Append(space);
                sb.Append("public ");
                sb.Append(table.Descriptions[i]);
                sb.Append(" ");
                sb.Append(table.Headers[i]);
                sb.Append(";");
                if(i < column - 1)
                    sb.Append(Environment.NewLine);
            }

            if (hasNameSpace)
                return ClassHeadTemplate + string.Format(ClassDefineWithNameSpace, namespaceName, className, sb);
            return ClassHeadTemplate + string.Format(ClassDefineWithoutNameSpace, className, sb);
        }
        
        public static CSVTableWriter Class2CSVTable<T>()
        {
            return Class2CSVTable(typeof(T));
        }

        public static CSVTableWriter Class2CSVTable(Type type)
        {
            if (type == null)
            {
                Debug.LogError("Type is null!");
                return null;
            }
            var csvTableWriter = new CSVTableWriter();
            foreach (var fieldInfo in type.GetFields())
            {
                csvTableWriter.Headers.Add(fieldInfo.Name);
                csvTableWriter.Descriptions.Add(fieldInfo.FieldType.Name);
            }
            return csvTableWriter;
        }
    }
}