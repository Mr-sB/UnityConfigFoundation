#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Config.Loader
{
    /// <summary>
    /// 加载文本资源
    /// </summary>
    public class TextAssetLoader : MonoBehaviour
    {
        private static TextAssetLoader instance;

        private static TextAssetLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    //Find
                    instance = FindObjectOfType<TextAssetLoader>();
                    //Create
                    if (instance == null)
                    {
                        var go = new GameObject(nameof(TextAssetLoader));
                        instance = go.AddComponent<TextAssetLoader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
#if UNITY_EDITOR
        public static string LoadInEditor(string absolutePath)
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(absolutePath).text;
        }
#endif

        public static void LoadFromStreamingAssets(string relativePath, Action<string> onLoaded)
        {
            if (onLoaded == null) return;
            Instance.StartCoroutine(LoadFromStreamingAssetsInternal(relativePath, onLoaded));
        }
        
        private static IEnumerator LoadFromStreamingAssetsInternal(string relativePath, Action<string> onLoaded)
        {
            if (onLoaded == null) yield break;
            string path = Path.Combine(Application.streamingAssetsPath, relativePath);
            UnityWebRequest request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();
            if (!request.isDone)
            {
                Debug.LogError("LoadFromStreamingAssets field:" + path);
                onLoaded(string.Empty);
            }
            onLoaded(request.downloadHandler.text);
        }
        
        public static string LoadFromPersistentData(string relativePath)
        {
            string path = Path.Combine(Application.persistentDataPath, relativePath);
            if (!File.Exists(path))
            {
                Debug.LogError("File:" + path + "does not exist!");
                return string.Empty;
            }
            using (StreamReader reader = new StreamReader(path))
                return reader.ReadToEnd();
        }
        
        /// <summary>
        /// Resources下的文本文件必须以.bytes结尾。例如Test.csv.bytes
        /// </summary>
        public static string LoadFromResources(string relativePath)
        {
            string path = Path.Combine(Application.persistentDataPath, relativePath);
            if (!path.EndsWith(".bytes"))
                path += ".bytes";
            if (!File.Exists(path))
            {
                Debug.LogError("File:" + path + "does not exist!");
                return string.Empty;
            }
            return Resources.Load<TextAsset>(relativePath).text;
        }
    }
}