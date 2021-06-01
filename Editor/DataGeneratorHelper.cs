using System;
using System.IO;
using TinyCSV;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameUtil.Config.Editor
{
    public static class DataGeneratorHelper
    {
        private const string CONFIG_CLASS_DEFINE_WITH_NAMESPACE =
@"using GameUtil.Config;

namespace {0}
{{
    public class {1} : DataConfigBase<{2}>
    {{
    }}
}}
";

        private const string CONFIG_CLASS_DEFINE_WITHOUT_NAMESPACE =
@"using GameUtil.Config;

public class {0} : DataConfigBase<{1}>
{{
}}
";

        public static void CreateClass(string csvPath, string modelPath, string namespaceName, string modelName, string configPath, string configName, bool refresh = true)
        {
            Write(CSVGenerator.CSV2Class(TextAssetLoader.LoadInEditor(csvPath), namespaceName, modelName),
                modelPath, namespaceName, modelName, configPath, configName, refresh);
        }
        
        public static void CreateClass(CSVTableReader csvTableReader, string modelPath, string namespaceName, string modelName, string configPath, string configName, bool refresh = true)
        {
            Write(CSVGenerator.CSV2Class(csvTableReader, namespaceName, modelName), modelPath,
                namespaceName, modelName, configPath, configName, refresh);
        }

        private static void Write(string modelContent, string modelPath, string namespaceName, string modelName, string configPath, string configName, bool refresh)
        {
            var fullPath = Path.Combine(Application.dataPath, modelPath, modelName + ".cs");
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, modelContent);

            bool hasNameSpace = !string.IsNullOrWhiteSpace(namespaceName);
            string configContent = hasNameSpace
                ? string.Format(CONFIG_CLASS_DEFINE_WITH_NAMESPACE, namespaceName, configName, modelName)
                : string.Format(CONFIG_CLASS_DEFINE_WITHOUT_NAMESPACE, configName, modelName);
            fullPath = Path.Combine(Application.dataPath, configPath, configName + ".cs");
            directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, configContent);

            if (refresh)
                Refresh();
        }

        #region CreateData Generic
        [Obsolete("Use CreateDataByPath instead.")]
        public static TDataConfig CreateData<TModel, TDataConfig>(string csvPath, string scriptObjectPath, bool refresh = true)
            where TModel : new() where TDataConfig : DataConfigBase<TModel>
        {
            return CreateDataByPath<TModel, TDataConfig>(csvPath, scriptObjectPath, refresh);
        }
        
        public static TDataConfig CreateDataByPath<TModel, TDataConfig>(string csvPath, string scriptObjectPath, bool refresh = true)
            where TModel : new() where TDataConfig : DataConfigBase<TModel>
        {
            return CreateDataByContent<TModel, TDataConfig>(TextAssetLoader.LoadInEditor(csvPath), scriptObjectPath, refresh);
        }
        
        public static TDataConfig CreateDataByContent<TModel, TDataConfig>(string csvContent, string scriptObjectPath, bool refresh = true)
            where TModel : new() where TDataConfig : DataConfigBase<TModel>
        {
            var dataConfig = ScriptableObject.CreateInstance<TDataConfig>();
            dataConfig.Data = CSVConverter.Convert<TModel>(csvContent);

            CreateAsset(scriptObjectPath, dataConfig, refresh);
            return dataConfig;
        }

        public static TDataConfig CreateColumnDataByPath<TModel, TDataConfig>(string csvPath, string scriptObjectPath, int headerIndex = 0, bool refresh = true)
            where TDataConfig : DataConfigBase<TModel>
        {
            return CreateColumnDataByContent<TModel, TDataConfig>(TextAssetLoader.LoadInEditor(csvPath), scriptObjectPath, headerIndex, refresh);
        }
        
        public static TDataConfig CreateColumnDataByPath<TModel, TDataConfig>(string csvPath, string scriptObjectPath, string headerName, bool refresh = true)
            where TDataConfig : DataConfigBase<TModel>
        {
            return CreateColumnDataByContent<TModel, TDataConfig>(TextAssetLoader.LoadInEditor(csvPath), scriptObjectPath, headerName, refresh);
        }
        
        public static TDataConfig CreateColumnDataByContent<TModel, TDataConfig>(string csvContent, string scriptObjectPath, int headerIndex = 0, bool refresh = true)
            where TDataConfig : DataConfigBase<TModel>
        {
            var dataConfig = ScriptableObject.CreateInstance<TDataConfig>();
            dataConfig.Data = CSVConverter.ConvertColumn<TModel>(csvContent, headerIndex);
            
            CreateAsset(scriptObjectPath, dataConfig, refresh);
            return dataConfig;
        }
        
        public static TDataConfig CreateColumnDataByContent<TModel, TDataConfig>(string csvContent, string scriptObjectPath, string headerName, bool refresh = true)
            where TDataConfig : DataConfigBase<TModel>
        {
            var dataConfig = ScriptableObject.CreateInstance<TDataConfig>();
            dataConfig.Data = CSVConverter.ConvertColumn<TModel>(csvContent, headerName);
            
            CreateAsset(scriptObjectPath, dataConfig, refresh);
            return dataConfig;
        }
        #endregion

        #region CreateData Type
        [Obsolete("Use CreateDataByPath instead.")]
        public static ScriptableObject CreateData(Type modelType, Type dataConfigType, string csvPath, string scriptObjectPath, bool refresh = true)
        {
            return CreateDataByPath(modelType, dataConfigType, csvPath, scriptObjectPath, refresh);
        }
        
        public static ScriptableObject CreateDataByPath(Type modelType, Type dataConfigType, string csvPath, string scriptObjectPath, bool refresh = true)
        {
            return CreateDataByContent(modelType, dataConfigType, TextAssetLoader.LoadInEditor(csvPath), scriptObjectPath, refresh);
        }
        
        public static ScriptableObject CreateDataByContent(Type modelType, Type dataConfigType, string csvContent, string scriptObjectPath, bool refresh = true)
        {
            if (!typeof(DataConfigBase<>).MakeGenericType(modelType).IsAssignableFrom(dataConfigType))
            {
                Debug.LogErrorFormat("dataConfigType: {0} must inherited from DataConfigBase<{1}>!", dataConfigType, modelType);
                return null;
            }
            var dataConfig = ScriptableObject.CreateInstance(dataConfigType);
            (dataConfig as IDataConfig).Assign(CSVConverter.Convert(modelType, csvContent));

            CreateAsset(scriptObjectPath, dataConfig, refresh);
            return dataConfig;
        }
        
        public static ScriptableObject CreateColumnDataByPath(Type modelType, Type dataConfigType, string csvPath, string scriptObjectPath, int headerIndex = 0, bool refresh = true)
        {
            return CreateColumnDataByContent(modelType, dataConfigType, TextAssetLoader.LoadInEditor(csvPath), scriptObjectPath, headerIndex, refresh);
        }
        
        public static ScriptableObject CreateColumnDataByPath(Type modelType, Type dataConfigType, string csvPath, string scriptObjectPath, string headerName, bool refresh = true)
        {
            return CreateColumnDataByContent(modelType, dataConfigType, TextAssetLoader.LoadInEditor(csvPath), scriptObjectPath, headerName, refresh);
        }
        
        public static ScriptableObject CreateColumnDataByContent(Type modelType, Type dataConfigType, string csvContent, string scriptObjectPath, int headerIndex = 0, bool refresh = true)
        {
            if (!typeof(DataConfigBase<>).MakeGenericType(modelType).IsAssignableFrom(dataConfigType))
            {
                Debug.LogErrorFormat("dataConfigType: {0} must inherited from DataConfigBase<{1}>!", dataConfigType, modelType);
                return null;
            }
            var dataConfig = ScriptableObject.CreateInstance(dataConfigType);
            (dataConfig as IDataConfig).Assign(CSVConverter.ConvertColumn(modelType, csvContent, headerIndex));
            
            CreateAsset(scriptObjectPath, dataConfig, refresh);
            return dataConfig;
        }
        
        public static ScriptableObject CreateColumnDataByContent(Type modelType, Type dataConfigType, string csvContent, string scriptObjectPath, string headerName, bool refresh = true)
        {
            if (!typeof(DataConfigBase<>).MakeGenericType(modelType).IsAssignableFrom(dataConfigType))
            {
                Debug.LogErrorFormat("dataConfigType: {0} must inherited from DataConfigBase<{1}>!", dataConfigType, modelType);
                return null;
            }
            var dataConfig = ScriptableObject.CreateInstance(dataConfigType);
            (dataConfig as IDataConfig).Assign(CSVConverter.ConvertColumn(modelType, csvContent, headerName));
            
            CreateAsset(scriptObjectPath, dataConfig, refresh);
            return dataConfig;
        }
        #endregion

        public static void CreateAsset(string assetPath, Object asset, bool refresh)
        {
            var oldConfig = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (oldConfig != null)
                AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);

            if (refresh)
                SaveAsset(asset);
            else
                EditorUtility.SetDirty(asset);
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