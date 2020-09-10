#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GameUtil.Config
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
            
            /*
            Android平台：
            Application.streamingAssetsPath + 你的资源
            IOS平台：
            "file://” + Application.streamingAssetsPath + 你的资源
            其余平台以及下：
            "file:///" + Application.streamingAssetsPath + 你的资源
            */
            string path = Path.Combine(Application.streamingAssetsPath, relativePath);
#if UNITY_EDITOR
            path = "file:///" + path;
#elif UNITY_IOS
            path = "file://" + path;
#elif !UNITY_ANDROID
            path = "file:///" + path;
#endif
            
            UnityWebRequest request = UnityWebRequest.Get(path);
            request.SendWebRequest();
            yield return request;
            if (request.isNetworkError)
            {
                Debug.LogError(request.error);
                onLoaded(string.Empty);
                yield break;
            }
            if (!request.isDone)
            {
                Debug.LogError("File:" + path + " does not exist!");
                onLoaded(string.Empty);
                yield break;
            }
            onLoaded(request.downloadHandler != null ? request.downloadHandler.text : string.Empty);
        }
        
        public static string LoadFromPersistentData(string relativePath)
        {
            string path = Path.Combine(Application.persistentDataPath, relativePath);
            if (!File.Exists(path))
            {
                Debug.LogError("File:" + path + " does not exist!");
                return string.Empty;
            }
            using (StreamReader reader = new StreamReader(path))
                return reader.ReadToEnd();
        }
        
        /// <summary>
        /// Resources下的二进制文件必须以.bytes结尾。例如Test.xxx.bytes
        /// Please notice that files with the .txt and .bytes extension will be treated as text and binary files, respectively.
        /// Do not attempt to store a binary file using the .txt extension, as this will create unexpected behaviour when attempting to read data from it.
        /// </summary>
        public static string LoadFromResources(string relativePath)
        {
            //Resources下的文本文件不能加.txt后缀
            if (relativePath.EndsWith(".txt"))
                relativePath = relativePath.Substring(0, relativePath.Length - 4);
            var textAsset = Resources.Load<TextAsset>(relativePath);
            if (textAsset == null)
            {
                Debug.LogError("File:" + relativePath + " does not exist!");
                return string.Empty;
            }
            return textAsset.text;
        }
    }
}