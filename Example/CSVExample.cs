using System;
using Config.Convert;
using UnityEngine;

namespace Config.Example
{
    public class CSVExample : MonoBehaviour
    {
        private void OnEnable()
        {
            string csvStr = CSVGenerator.Class2CSV<ExampleTestData>();
            Debug.LogError("Auto generate csv:\n" + csvStr);

            string classStr = CSVGenerator.CSV2Class(nameof(ExampleTestData), csvStr);
            Debug.LogError("Auto generate class:\n" + classStr);
            
            csvStr += "1,#cccccc,2,1;2;3|4;5;6" + Environment.NewLine + "3,#dddddd,4,7;8;9|10;11;12|7;7;7";
            Debug.LogError("csv add data:\n" + csvStr);
            
            var dataArray = CSVConverter.Convert<ExampleTestData>(csvStr);
            Debug.LogError("Auto generate data Array:");
            foreach (var data in dataArray)
                Debug.LogError(data);

            Debug.LogError("Auto generate data IEnumerable:");
            foreach (var data in CSVConverter.ConvertNonAlloc<ExampleTestData>(csvStr))
                Debug.LogError(data);
        }
    }
}