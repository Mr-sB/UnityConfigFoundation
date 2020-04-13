using System;

namespace Config
{
    /// <summary>
    /// CSV表格数据
    /// </summary>
    public class CSVTable
    {
        public readonly string[] FieldNames;
        public readonly string[] FieldTypes;
        public readonly CSVRecord[] Records;

        private CSVTable(string[] rows)
        {
            FieldNames = rows[0].Split(',');
            FieldTypes = rows[1].Split(',');
            int recordLen = rows.Length;
            //除去第一行的字段名和第二行的字段类型
            Records = new CSVRecord[recordLen - 2];
            for (int i = 2; i < recordLen; i++)
                Records[i] = new CSVRecord(FieldNames, rows[i].Split(','));
        }
        
        public static CSVTable GetCSVTable(string csvContent)
        {
            string[] rows = GetCSVRows(csvContent);
            if (rows.Length < 2) return null;
            return new CSVTable(rows);
        }

        public static string[] GetCSVRows(string csvContent)
        {
            //替换回车换行为\r
            string csvText = csvContent.Replace(Environment.NewLine, "\r");
            //去除前后的\r字符
            csvText = csvText.Trim('\r');
            //替换字符
            csvText = csvText.Replace ("\r\r","\r");
            //分割所有行
            return csvText.Split('\r');
        }
    }
}