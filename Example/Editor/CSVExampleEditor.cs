using System.IO;
using UnityEditor;
using UnityEngine;
using GameUtil.Config.Editor;

namespace GameUtil.Config.Example.Editor
{
    [CustomEditor(typeof(CSVExample)), CanEditMultipleObjects]
    public class CSVExampleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("CSVReadAndWriteTest"))
            {
                ((CSVExample)target).CSVReadAndWriteTest();
            }
            if (GUILayout.Button("CreateOrRefreshDataConfig"))
            {
                var directoryName = Path.GetDirectoryName(((CSVExample)target).gameObject.scene.path);
                DataGeneratorHelper.CreateDataByPath<ExampleTestData, ExampleTestDataConfig>(directoryName + "/ExampleTestData.csv", directoryName + "/ExampleTestDataConfig.asset");
                Debug.Log("CreateOrRefreshDataConfig finish. Click me to select data config asset!", AssetDatabase.LoadMainAssetAtPath(directoryName + "/ExampleTestDataConfig.asset"));
            }
        }
    }
}