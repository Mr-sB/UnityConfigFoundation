namespace GameUtil.Config
{
    /// <summary>
    /// CSV表格数据
    /// </summary>
    public class CSVTableReader
    {
        public readonly string CSVContent;
        public readonly string[] Headers;
        public readonly string[] Descriptions;
        public readonly CSVRecordReader[] Records;
        public int Column => Headers.Length;

        public CSVTableReader(string csvContent)
        {
            CSVContent = csvContent;
            string[] rows = CSVContent.GetCSVRows();
            int recordLen = rows.Length;
            Headers = recordLen > 0 ? rows[0].GetCSVDecodeRow().ToArray() : new string[0];
            Descriptions = recordLen > 1 ? rows[1].GetCSVDecodeRow(Column).ToArray() : new string[0];
            if (recordLen > 2)
            {
                //除去第一行的字段名和第二行的字段类型
                Records = new CSVRecordReader[recordLen - 2];
                for (int i = 2; i < recordLen; i++)
                    Records[i - 2] = new CSVRecordReader(Headers, rows[i]);
            }
            else
                Records = new CSVRecordReader[0];
        }
    }
}