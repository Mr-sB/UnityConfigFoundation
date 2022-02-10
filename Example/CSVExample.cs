using System;
using System.Collections.Generic;
using TinyCSV;
using UnityEngine;

namespace GameUtil.Config.Example
{
    [ExecuteAlways]
    public class CSVExample : MonoBehaviour
    {
        public void CSVReadAndWriteTest()
        {
            //Support List<> data type.
            Debug.Log("Auto generate data list:");
            foreach (var data in CSVConverter.ConvertColumn<List<float>>(
                new CSVTableWriter()
                    .AddHeader(new List<string> {"Item1"})
                    .AddRecord(new CSVRecordWriter().AddCell("1.25|3.33|2.5|4"))
                    .AddRecord(new CSVRecordWriter().AddCell("2.5|4|5.1"))
                    .GetEncodeTable()))
                Debug.Log(string.Join("|", data));
            
            //Using ValueTuple as the data type.
            Debug.Log("Auto generate data list:");
            foreach (var data in CSVConverter.Convert<ValueTuple<float, string>>(
                new CSVTableWriter()
                    .AddHeader(new List<string>{"Item1", "Item2"})
                    .AddHeader(new List<string>{"float", "string"})
                    .AddRecord(new CSVRecordWriter().AddCell("1.25").AddCell("first"))
                    .AddRecord(new CSVRecordWriter().AddCell("2.5").AddCell("second"))
                    .GetEncodeTable()))
                Debug.Log(data);
            
            //Custom cell separator.
            CSVTableWriter csvTableWriter = CSVGenerator.Class2CSVTable<ExampleTestData>(';');
            Debug.Log("Auto generate csv:\n" + csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix));

            string classStr = CSVGenerator.CSV2Class(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), null, nameof(ExampleTestData), csvTableWriter.CellSeparator);
            Debug.Log("Auto generate class:\n" + classStr);

            csvTableWriter.AddRecord(new CSVRecordWriter()
                .AddCell("1")
                .AddCell("#cccccc")
                .AddCell("2")
                .AddCell("normal string")
                .AddCell("\"string with double quote")
                .AddCell("1;2;3|4;5;6")
                .AddCell("#cccccc;string content")
                .AddCell("#cccccc;string content|#ffffff;second string")
                .AddCell("#cccccc;string content|#ffffff;second string"));
            csvTableWriter.AddRecord(new CSVRecordWriter()
                .AddCell("3")
                .AddCell("#dddddd")
                .AddCell("4")
                .AddCell("string with, comma")
                .AddCell("\"string with\", comma and \"double quote")
                .AddCell("7;8;9|10;11;12|7;7;7")
                .AddCell("#dddddd;string content2")
                .AddCell("#dddddd;string content2|#eeeeee;second string2")
                .AddCell("#dddddd;string content2|#eeeeee;second string2"));
            Debug.Log("csv add data:\n" + csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix));
            
            var dataList = CSVConverter.Convert<ExampleTestData>(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), csvTableWriter.CellSeparator);
            Debug.Log("Auto generate data list:");
            foreach (var data in dataList)
                Debug.Log(data);

            Debug.Log("Auto generate data IEnumerable:");
            foreach (var data in CSVConverter.ConvertEnumerator<ExampleTestData>(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), csvTableWriter.CellSeparator))
                Debug.Log(data);

            string newContent = new CSVTableWriter()
                .AddHeader(new List<string>{"Name", "Age"})
                .AddHeader(new List<string>{"string", "int"})
                .AddRecord(new CSVRecordWriter().AddCell("Name1").AddCell("10"))
                .AddRecord(new CSVRecordWriter().AddCell("Name2").AddCell("20"))
                .AddRecord(new CSVRecordWriter().AddCell("Name3").AddCell("30"))
                .AddRecord(new CSVRecordWriter().AddCell("Name4").AddCell("40"))
                .GetEncodeTable(NewLineStyle.NonUnix);
            
            Debug.Log("Auto generate column data by header name:");
            foreach (var name in CSVConverter.ConvertColumn<string>(newContent, "Name"))
                Debug.Log(name);
            
            Debug.Log("Auto generate column data by header index:");
            foreach (var age in CSVConverter.ConvertColumn<int>(newContent, 1))
                Debug.Log(age);
        }
    }
}