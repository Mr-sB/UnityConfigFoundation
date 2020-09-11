using System.Collections.Generic;
using System.Text;

namespace GameUtil.Config
{
    public class CSVTableWriter
    {
        public readonly List<string> Headers;
        public readonly List<string> Descriptions;
        public readonly List<CSVRecordWriter> Records;

        public CSVTableWriter()
        {
            Headers = new List<string>();
            Descriptions = new List<string>();
            Records = new List<CSVRecordWriter>();
        }

        public void AddHeader(string header)
        {
            Headers.Add(header);
        }

        public void AddDescription(string description)
        {
            Descriptions.Add(description);
        }

        public void AddRecord(CSVRecordWriter csvRecordWriter)
        {
            Records.Add(csvRecordWriter);
        }
        
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Headers.GetCSVEncodeRow());
            stringBuilder.AppendLine(Descriptions.GetCSVEncodeRow());
            foreach (var record in Records)
                stringBuilder.AppendLine(record.ToString());
            return stringBuilder.ToString();
        }
    }
}