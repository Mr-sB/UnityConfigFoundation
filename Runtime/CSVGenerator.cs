using System;
using System.Collections.Generic;
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
        private const string CLASS_HEAD_TEMPLATE =
@"using System;
using System.Collections.Generic;
using UnityEngine;

";

        private const string CLASS_DEFINE_WITH_NAMESPACE =
@"namespace {0}
{{
    [Serializable]
    public class {1}
    {{
{2}
    }}
}}
";

        private const string CLASS_DEFINE_WITHOUT_NAMESPACE =
@"[Serializable]
public class {0}
{{
{1}
}}
";

        public static string CSV2Class(string csvContent, string namespaceName, string className, char cellSeparator = CSVDataHelper.CommaCharacter)
        {
            return CSV2Class(new CSVTableReader(csvContent, CSVConverter.HeaderRow, cellSeparator, true, 0), namespaceName, className);
        }
        
        public static string CSV2Class(CSVTableReader table, string namespaceName, string className)
        {
            bool hasNameSpace = !string.IsNullOrWhiteSpace(namespaceName);
            string space = hasNameSpace ? "        " : "    ";
            StringBuilder sb = new StringBuilder();
            bool isFirstLine = true;
            var filedNames = table.Headers[0];
            var fieldTypes = table.Headers[1];
            for (int i = 0, column = filedNames.Length; i < column; i++)
            {
                //Skip empty header column.
                if (string.IsNullOrEmpty(filedNames[i])) continue;
                if (isFirstLine)
                    isFirstLine = false;
                else
                    sb.Append('\n');
                sb.Append(space);
                sb.Append("public ");
                sb.Append(fieldTypes[i]);
                sb.Append(" ");
                sb.Append(filedNames[i]);
                sb.Append(";");
            }

            if (hasNameSpace)
                return CLASS_HEAD_TEMPLATE + string.Format(CLASS_DEFINE_WITH_NAMESPACE, namespaceName, className, sb);
            return CLASS_HEAD_TEMPLATE + string.Format(CLASS_DEFINE_WITHOUT_NAMESPACE, className, sb);
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
            var fieldInfos = type.GetFields();
            var filedNames = new List<string>(fieldInfos.Length);
            var fieldTypes = new List<string>(fieldInfos.Length);
            csvTableWriter.AddHeader(filedNames);
            csvTableWriter.AddHeader(fieldTypes);
            foreach (var fieldInfo in fieldInfos)
            {
                filedNames.Add(fieldInfo.Name);
                fieldTypes.Add(GetTypeName(fieldInfo.FieldType));
            }
            return csvTableWriter;
        }

        public static string GetTypeName(Type type)
        {
            if (type.IsArray)
            {
                string fieldName = GetTypeName(type.GetElementType()) + '[';
                for (int i = 0, len = type.GetArrayRank() - 1; i < len; i++)
                    fieldName += ',';
                fieldName += ']';
                return fieldName;
            }

            if (type.IsGenericType)
            {
                Type definitionType = type.GetGenericTypeDefinition();
                string fieldName = definitionType.Name;
                fieldName = fieldName.Substring(0, fieldName.LastIndexOf('`')) + '<';
                var genericArguments = type.GetGenericArguments();
            
                fieldName += GetTypeName(genericArguments[0]);
                for (int i = 1, len = genericArguments.Length; i < len; i++)
                    fieldName += ',' + GetTypeName(genericArguments[i]);
            
                fieldName += '>';
                return fieldName;
            }
            return type.Name;
        }
    }
}