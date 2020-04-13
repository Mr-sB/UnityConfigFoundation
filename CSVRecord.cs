using System.Collections.Generic;

namespace Config
{
    public class CSVRecord
    {
        public readonly Dictionary<string, string> RecordDict;
        public readonly List<string> RecordList;

        public CSVRecord(string[] fieldNames, string[] records)
        {
            int fieldLen = fieldNames.Length;
            RecordDict = new Dictionary<string, string>(fieldLen);
            RecordList = new List<string>();
            for (int i = 0; i < fieldLen; i++)
            {
                string record = records[i];
                RecordDict.Add(fieldNames[i], record);
                RecordList.Add(record);
            }
        }
    }
}