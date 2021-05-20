using System;
using System.Text;
using TinyCSV;
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
                                                                  + "    [Serializable]" + Environment.NewLine
                                                                  + "    public class {1}" + Environment.NewLine
                                                                  + "    {{" + Environment.NewLine
                                                                  + "{2}" + Environment.NewLine
                                                                  + "    }}" + Environment.NewLine
                                                                  + "}}" + Environment.NewLine;

        private static readonly string ClassDefineWithoutNameSpace = "[Serializable]" + Environment.NewLine
                                                                     + "public class {0}" + Environment.NewLine
                                                                     + "{{" + Environment.NewLine
                                                                     + "{1}" + Environment.NewLine
                                                                     + "}}" + Environment.NewLine;

        public static string CSV2Class(string csvContent, string namespaceName, string className, char cellSeparator = CSVDataHelper.CommaCharacter)
        {
            return CSV2Class(new CSVTableReader(csvContent, cellSeparator, true, false), namespaceName, className);
        }
        
        public static string CSV2Class(CSVTableReader table, string namespaceName, string className)
        {
            bool hasNameSpace = !string.IsNullOrWhiteSpace(namespaceName);
            string space = hasNameSpace ? "        " : "    ";
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
        
        public static CSVTableWriter Class2CSVTable<T>(char cellSeparator = CSVDataHelper.CommaCharacter)
        {
            return Class2CSVTable(typeof(T), cellSeparator);
        }

        public static CSVTableWriter Class2CSVTable(Type type, char cellSeparator = CSVDataHelper.CommaCharacter)
        {
            if (type == null)
            {
                Debug.LogError("Type is null!");
                return null;
            }
            var csvTableWriter = new CSVTableWriter(cellSeparator);
            foreach (var fieldInfo in type.GetFields())
            {
                csvTableWriter.Headers.Add(fieldInfo.Name);
                csvTableWriter.Descriptions.Add(fieldInfo.FieldType.Name);
            }
            return csvTableWriter;
        }
    }
}