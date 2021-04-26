#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GameUtil.Config
{
    /// <summary>
    /// 加载文本资源
    /// </summary>
    public static class TextAssetLoader// : MonoBehaviour
    {
        // private static TextAssetLoader instance;
        //
        // private static TextAssetLoader Instance
        // {
        //     get
        //     {
        //         if (instance == null)
        //         {
        //             //Find
        //             instance = FindObjectOfType<TextAssetLoader>();
        //             //Create
        //             if (instance == null)
        //             {
        //                 var go = new GameObject(nameof(TextAssetLoader));
        //                 instance = go.AddComponent<TextAssetLoader>();
        //                 DontDestroyOnLoad(go);
        //             }
        //         }
        //         return instance;
        //     }
        // }
        
#if UNITY_EDITOR
        public static string LoadInEditor(string absolutePath)
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(absolutePath).text;
        }
#endif

        public static void LoadFromStreamingAssets(string relativePath, Action<string> loaded)
        {
            if (loaded == null) return;
            /*
            Android: Application.streamingAssetsPath + /asset
            Others(Include IOS): "file://" + Application.streamingAssetsPath + /asset
            */
            string path = Path.Combine(Application.streamingAssetsPath, relativePath);
#if !UNITY_ANDROID || UNITY_EDITOR
            path = "file://" + path;
#endif
            
            UnityWebRequest.Get(path).SendWebRequest().completed += asyncOperation =>
            {
                if (!TryGetUnityWebRequestFromAsyncOperation(asyncOperation, out var webRequest))
                {
                    loaded(string.Empty);
                    return;
                }

                string text = string.Empty;
#if UNITY_2020_1_OR_NEWER
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.InProgress:
                        Debug.LogError("WebRequest is not done yet!");
                        break;
                    case UnityWebRequest.Result.Success:
                        text = webRequest.downloadHandler != null ? webRequest.downloadHandler.text : string.Empty;
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(webRequest.error);
                        break;
                }
#else
                if (webRequest.isNetworkError || webRequest.isHttpError)
                    Debug.LogError(webRequest.error);
                else if (!webRequest.isDone)
                    Debug.LogError("WebRequest is not done yet!");
                else
                    text = webRequest.downloadHandler != null ? webRequest.downloadHandler.text : string.Empty;
#endif
                webRequest.Dispose();
                loaded(text);
            };
        }
        
        private static bool TryGetUnityWebRequestFromAsyncOperation(AsyncOperation asyncOperation, out UnityWebRequest webRequest)
        {
            webRequest = null;
            if (!(asyncOperation is UnityWebRequestAsyncOperation webRequestAsyncOperation))
            {
                Debug.LogError("AsyncOperation is not UnityWebRequestAsyncOperation!");
                return false;
            }

            webRequest = webRequestAsyncOperation.webRequest;
            if (webRequest == null)
            {
                Debug.LogError("UnityWebRequest is null!");
                return false;
            }
            return true;
        }
        
        public static string LoadFromPersistentData(string relativePath)
        {
            string path = Path.Combine(Application.persistentDataPath, relativePath);
            if (!File.Exists(path))
            {
                Debug.LogError("File:" + path + " does not exist!");
                return string.Empty;
            }
            return File.ReadAllText(path);
        }
        
        public static string LoadFromResources(string relativePath)
        {
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