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
        private static readonly string ConfigClassDefineWithNameSpace = "namespace {0}" + Environment.NewLine
                                                                                        + "{{" + Environment.NewLine
                                                                                        + "    public class {1} : DataConfigBase<{2}>" + Environment.NewLine
                                                                                        + "    {{" + Environment.NewLine
                                                                                        + "    }}" + Environment.NewLine
                                                                                        + "}}" + Environment.NewLine;

        private static readonly string ConfigClassDefineWithoutNameSpace = "public class {0} : DataConfigBase<{1}>" + Environment.NewLine
                                                                                                                    + "{{" + Environment.NewLine
                                                                                                                    + "}}" + Environment.NewLine;

        public static void CreateClass(string csvPath, string modelPath, string namespaceName, string modelName, string configPath, string configName)
        {
            string modelContent = CSVGenerator.CSV2Class(TextAssetLoader.LoadInEditor(csvPath), namespaceName, modelName);
            File.WriteAllText(Path.Combine(Application.dataPath, modelPath, modelName + ".cs"), modelContent);

            bool hasNameSpace = !string.IsNullOrWhiteSpace(namespaceName);
            string configContent = hasNameSpace
                ? string.Format(ConfigClassDefineWithNameSpace, namespaceName, configName, modelName)
                : string.Format(ConfigClassDefineWithoutNameSpace, configName, modelName);
            File.WriteAllText(Path.Combine(Application.dataPath, configPath, configName + ".cs"), configContent);

            Refresh();
        }

        public static TDataConfig CreateData<TModel, TDataConfig>(string csvPath, string scriptObjectPath)
            where TModel : new() where TDataConfig : DataConfigBase<TModel>
        {
            var dataConfig = ScriptableObject.CreateInstance<TDataConfig>();
            dataConfig.Data = CSVConverter.Convert<TModel>(TextAssetLoader.LoadInEditor(csvPath));

            var oldConfig = AssetDatabase.LoadAssetAtPath<TDataConfig>(scriptObjectPath);
            if (oldConfig != null)
                AssetDatabase.DeleteAsset(scriptObjectPath);
            AssetDatabase.CreateAsset(dataConfig, scriptObjectPath);

            SaveAsset(dataConfig);
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