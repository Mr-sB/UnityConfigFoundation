using System;
using System.IO;
using GameUtil.Config;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameUtil.Config.Editor
{
    public static class DataGeneratorHelper
    {
        private static readonly string ConfigClassDefineWithNameSpace = "using GameUtil.Config;" + Environment.NewLine + Environment.NewLine
                                                                        + "namespace {0}" + Environment.NewLine
                                                                        + "{{" + Environment.NewLine
                                                                        + "    public class {1} : DataConfigBase<{2}>" +
                                                                        Environment.NewLine
                                                                        + "    {{" + Environment.NewLine
                                                                        + "    }}" + Environment.NewLine
                                                                        + "}}" + Environment.NewLine;

        private static readonly string ConfigClassDefineWithoutNameSpace = "using GameUtil.Config;" + Environment.NewLine + Environment.NewLine
                                                                           + "public class {0} : DataConfigBase<{1}>" + Environment.NewLine
                                                                           + "{{" + Environment.NewLine
                                                                           + "}}" + Environment.NewLine;

        public static void CreateClass(string csvPath, string modelPath, string namespaceName, string modelName, string configPath, string configName, bool refresh = true)
        {
            string modelContent = CSVGenerator.CSV2Class(TextAssetLoader.LoadInEditor(csvPath), namespaceName, modelName);
            var fullPath = Path.Combine(Application.dataPath, modelPath, modelName + ".cs");
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, modelContent);

            bool hasNameSpace = !string.IsNullOrWhiteSpace(namespaceName);
            string configContent = hasNameSpace
                ? string.Format(ConfigClassDefineWithNameSpace, namespaceName, configName, modelName)
                : string.Format(ConfigClassDefineWithoutNameSpace, configName, modelName);
            fullPath = Path.Combine(Application.dataPath, configPath, configName + ".cs");
            directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, configContent);

            if (refresh)
                Refresh();
        }

        public static TDataConfig CreateData<TModel, TDataConfig>(string csvPath, string scriptObjectPath, bool refresh = true)
            where TModel : new() where TDataConfig : DataConfigBase<TModel>
        {
            var dataConfig = ScriptableObject.CreateInstance<TDataConfig>();
            dataConfig.Data = CSVConverter.Convert<TModel>(TextAssetLoader.LoadInEditor(csvPath));

            var oldConfig = AssetDatabase.LoadMainAssetAtPath(scriptObjectPath);
            if (oldConfig != null)
                AssetDatabase.DeleteAsset(scriptObjectPath);
            AssetDatabase.CreateAsset(dataConfig, scriptObjectPath);

            if (refresh)
                SaveAsset(dataConfig);
            else
                EditorUtility.SetDirty(dataConfig);
            return dataConfig;
        }
        
        public static ScriptableObject CreateData(Type modelType, Type dataConfigType, string csvPath, string scriptObjectPath, bool refresh = true)
        {
            if (!typeof(DataConfigBase<>).MakeGenericType(modelType).IsAssignableFrom(dataConfigType))
            {
                Debug.LogErrorFormat("dataConfigType: {0} must inherited from DataConfigBase<{1}>!", dataConfigType, modelType);
                return null;
            }
            var dataConfig = ScriptableObject.CreateInstance(dataConfigType);
            (dataConfig as IDataConfig).Assign(CSVConverter.Convert(modelType, TextAssetLoader.LoadInEditor(csvPath)));

            var oldConfig = AssetDatabase.LoadMainAssetAtPath(scriptObjectPath);
            if (oldConfig != null)
                AssetDatabase.DeleteAsset(scriptObjectPath);
            AssetDatabase.CreateAsset(dataConfig, scriptObjectPath);

            if (refresh)
                SaveAsset(dataConfig);
            else
                EditorUtility.SetDirty(dataConfig);
            return dataConfig;
        }

        public static void SaveAsset(Object obj)
        {
            EditorUtility.SetDirty(obj);
            Refresh();
        }

        public static void Refresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}