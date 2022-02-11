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
                    .AddHeader(new CSVRecordWriter {"Item1"})
                    .AddHeader(new CSVRecordWriter {"List<float>"})
                    .AddRecord(new CSVRecordWriter {"1.25|3.33|2.5|4"})
                    .AddRecord(new CSVRecordWriter {"2.5|4|5.1"})
                    .GetEncodeTable()))
                Debug.Log(string.Join("|", data));
            
            //Using ValueTuple as the data type.
            Debug.Log("Auto generate data list:");
            foreach (var data in CSVConverter.Convert<ValueTuple<float, string>>(
                new CSVTableWriter()
                    .AddHeader(new CSVRecordWriter {"Item1", "Item2"})
                    .AddHeader(new CSVRecordWriter {"float", "string"})
                    .AddRecord(new CSVRecordWriter {"1.25", "first"})
                    .AddRecord(new CSVRecordWriter {"2.5", "second"})
                    .GetEncodeTable()))
                Debug.Log(data);
            
            //Custom cell separator.
            CSVTableWriter csvTableWriter = CSVGenerator.Class2CSVTable<ExampleTestData>(';');
            Debug.Log("Auto generate csv:\n" + csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix));

            string classStr = CSVGenerator.CSV2Class(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), null, nameof(ExampleTestData), csvTableWriter.CellSeparator);
            Debug.Log("Auto generate class:\n" + classStr);

            csvTableWriter.AddRecord(new CSVRecordWriter()
                .Add("1")
                .Add("#cccccc")
                .Add("2")
                .Add("normal string")
                .Add("\"string with double quote")
                .Add("1;2;3|4;5;6")
                .Add("#cccccc;string content")
                .Add("#cccccc;string content|#ffffff;second string")
                .Add("#cccccc;string content|#ffffff;second string"));
            csvTableWriter.AddRecord(new CSVRecordWriter()
                .Add("3")
                .Add("#dddddd")
                .Add("4")
                .Add("string with, comma")
                .Add("\"string with\", comma and \"double quote")
                .Add("7;8;9|10;11;12|7;7;7")
                .Add("#dddddd;string content2")
                .Add("#dddddd;string content2|#eeeeee;second string2")
                .Add("#dddddd;string content2|#eeeeee;second string2"));
            Debug.Log("csv add data:\n" + csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix));
            
            var dataList = CSVConverter.Convert<ExampleTestData>(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), csvTableWriter.CellSeparator);
            Debug.Log("Auto generate data list:");
            foreach (var data in dataList)
                Debug.Log(data);

            Debug.Log("Auto generate data IEnumerable:");
            foreach (var data in CSVConverter.ConvertEnumerator<ExampleTestData>(csvTableWriter.GetEncodeTable(NewLineStyle.NonUnix), csvTableWriter.CellSeparator))
                Debug.Log(data);

            string newContent = new CSVTableWriter()
                .AddHeader(new CSVRecordWriter {"Name", "Age"})
                .AddHeader(new CSVRecordWriter {"string", "int"})
                .AddRecord(new CSVRecordWriter {"Name1", "10"})
                .AddRecord(new CSVRecordWriter {"Name2", "20"})
                .AddRecord(new CSVRecordWriter {"Name3", "30"})
                .AddRecord(new CSVRecordWriter {"Name4", "40"})
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