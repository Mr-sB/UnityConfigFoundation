using TinyCSV;
using UnityEngine;

namespace GameUtil.Config.Example
{
    public class CSVExample : MonoBehaviour
    {
        private void OnEnable()
        {
            //Custom cell separator.
            CSVTableWriter csvTableWriter = CSVGenerator.Class2CSVTable<ExampleTestData>(';');
            Debug.LogError("Auto generate csv:\n" + csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix));

            string classStr = CSVGenerator.CSV2Class(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), null, nameof(ExampleTestData), csvTableWriter.CellSeparator);
            Debug.LogError("Auto generate class:\n" + classStr);

            csvTableWriter.AddRecord(new CSVRecordWriter()
                .AddCell("1")
                .AddCell("#cccccc")
                .AddCell("2")
                .AddCell("normal string")
                .AddCell("\"string with double quote")
                .AddCell("1;2;3|4;5;6"));
            csvTableWriter.AddRecord(new CSVRecordWriter()
                .AddCell("3")
                .AddCell("#dddddd")
                .AddCell("4")
                .AddCell("string with, comma")
                .AddCell("\"string with\", comma and \"double quote")
                .AddCell("7;8;9|10;11;12|7;7;7"));
            Debug.LogError("csv add data:\n" + csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix));
            
            var dataArray = CSVConverter.Convert<ExampleTestData>(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), csvTableWriter.CellSeparator);
            Debug.LogError("Auto generate data Array:");
            foreach (var data in dataArray)
                Debug.LogError(data);

            Debug.LogError("Auto generate data IEnumerable:");
            foreach (var data in CSVConverter.ConvertEnumerator<ExampleTestData>(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), csvTableWriter.CellSeparator))
                Debug.LogError(data);
        }
    }
}