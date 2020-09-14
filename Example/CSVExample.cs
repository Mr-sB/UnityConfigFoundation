using TinyCSV;
using UnityEngine;

namespace GameUtil.Config.Example
{
    public class CSVExample : MonoBehaviour
    {
        private void OnEnable()
        {
            CSVTableWriter csvTableWriter = CSVGenerator.Class2CSVTable<ExampleTestData>();
            Debug.LogError("Auto generate csv:\n" + csvTableWriter);

            string classStr = CSVGenerator.CSV2Class(csvTableWriter.ToString(), null, nameof(ExampleTestData));
            Debug.LogError("Auto generate class:\n" + classStr);

            var record1 = new CSVRecordWriter();
            record1.AddCell("1");
            record1.AddCell("#cccccc");
            record1.AddCell("2");
            record1.AddCell("normal string");
            record1.AddCell("\"string with double quote");
            record1.AddCell("1;2;3|4;5;6");
            csvTableWriter.AddRecord(record1);
            var record2 = new CSVRecordWriter();
            record2.AddCell("3");
            record2.AddCell("#dddddd");
            record2.AddCell("4");
            record2.AddCell("string with, comma");
            record2.AddCell("\"string with\", comma and \"double quote");
            record2.AddCell("7;8;9|10;11;12|7;7;7");
            csvTableWriter.AddRecord(record2);
            Debug.LogError("csv add data:\n" + csvTableWriter);
            
            var dataArray = CSVConverter.Convert<ExampleTestData>(csvTableWriter.ToString());
            Debug.LogError("Auto generate data Array:");
            foreach (var data in dataArray)
                Debug.LogError(data);

            Debug.LogError("Auto generate data IEnumerable:");
            foreach (var data in CSVConverter.ConvertEnumerator<ExampleTestData>(csvTableWriter.ToString()))
                Debug.LogError(data);
        }
    }
}