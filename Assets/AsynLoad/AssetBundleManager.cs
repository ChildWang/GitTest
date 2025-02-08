using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR_WIN
using UnityEditor;
#endif



// 异步加载事件委托定义
public delegate void LoadingStartedHandler();
public delegate void LoadingUpdatedHandler(float progress);
public delegate void LoadingCompletedHandler();
// 新增配置加载完成事件委托
public delegate void ConfigLoadedHandler();

// 新增枚举类型表示数据源
public enum DataSource
{
    JSON,
    ScriptableObject
}

public class AssetBundleManager : MonoBehaviour
{
    // 用于存储待加载的资源信息的队列
    private List<AssetInfo> assetQueue = new List<AssetInfo>();

    // 事件声明
    public event LoadingStartedHandler OnLoadingStarted;
    public event LoadingUpdatedHandler OnLoadingUpdated;
    public event LoadingCompletedHandler OnLoadingCompleted;
    // 新增配置加载完成事件
    public event ConfigLoadedHandler OnConfigLoaded;

    // 根据平台和数据源选择加载方式，并对比更新
    public IEnumerator LoadConfig(string urlOrPath, DataSource source)
    {
        AssetConfig jsonConfig = null;
        AssetConfigSO soConfig = null;
        if (source == DataSource.JSON)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // WebGL 平台从网页加载
                using (UnityWebRequest uwr = UnityWebRequest.Get(new Uri(urlOrPath)))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result == UnityWebRequest.Result.Success)
                    {
                        string json = uwr.downloadHandler.text;
                        jsonConfig = JsonUtility.FromJson<AssetConfig>(json);
                    }
                    else
                    {
                        Debug.LogError($"Error loading config from {urlOrPath}: {uwr.error}");
                    }
                }
            }
            else
            {
                // 其他平台从文件加载
                if (File.Exists(urlOrPath))
                {
                    string json = File.ReadAllText(urlOrPath);
                    jsonConfig = JsonUtility.FromJson<AssetConfig>(json);
                }
                else
                {
                    Debug.LogError($"Config file {urlOrPath} not found.");
                }
            }

            soConfig = Resources.Load<AssetConfigSO>("AssetConfigSO");
            if (soConfig != null)
            {
#if UNITY_EDITOR_WIN                
                if (!File.Exists(urlOrPath))
                {
        
                    // Convert ScriptableObject config to JSON and save it
                    jsonConfig = new AssetConfig { assets = soConfig.assets };
                    string jsonContent = JsonUtility.ToJson(jsonConfig, true);
                    File.WriteAllText(urlOrPath, jsonContent);
                }
#endif
                if (!IsConfigsEqual(jsonConfig.assets, soConfig.assets))
                {
                    UpdateScriptableObjectConfig(soConfig, jsonConfig.assets);
                }
            }
            else
            {
#if UNITY_EDITOR_WIN
                soConfig = ScriptableObject.CreateInstance<AssetConfigSO>();
                soConfig.assets = jsonConfig.assets;
                AssetDatabase.CreateAsset(soConfig, "Assets/Resources/AssetConfigSO.asset");
                AssetDatabase.SaveAssets();
#endif
            }

            foreach (var info in jsonConfig.assets)
            {
                AddToQueue(info);
            }
        }
        else if (source == DataSource.ScriptableObject)
        {
            string soConfigname = Path.GetFileNameWithoutExtension(urlOrPath);
            Debug.Log(soConfigname);
            soConfig = Resources.Load<AssetConfigSO>("AssetConfigSO");
            if (soConfig != null)
            {
                foreach (var info in soConfig.assets)
                {
                    AddToQueue(info);
                }
            }
            else
            {
                Debug.LogError($"ScriptableObject {urlOrPath} not found.");
            }
        }
        yield return null;
       // 触发配置加载完成事件
       OnConfigLoaded?.Invoke();
    }

    // 对比两个配置列表是否一致
    private bool IsConfigsEqual(List<AssetInfo> list1, List<AssetInfo> list2)
    {
        if (list1.Count != list2.Count)
            return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (!list1[i].Equals(list2[i]))
                return false;
        }

        return true;
    }

    // 更新 ScriptableObject 配置
    private void UpdateScriptableObjectConfig(AssetConfigSO soConfig, List<AssetInfo> newAssets)
    {
#if UNITY_EDITOR_WIN
        soConfig.assets = newAssets;
        EditorUtility.SetDirty(soConfig);
        AssetDatabase.SaveAssets();
#endif
    }

    // 向队列中添加资源信息
    public void AddToQueue(AssetInfo info)
    {
        assetQueue.Add(info);
        // 每次添加后按优先级排序
        assetQueue = assetQueue.OrderBy(item => item.priority).ToList();
    }

    // 开始加载队列中的资源
    public IEnumerator StartLoading()
    {
        if (assetQueue.Count == 0)
        {
            Debug.LogError("No configuration data loaded. Please load the configuration file first.");
            yield break;
        }

        // 触发开始事件
        OnLoadingStarted?.Invoke();

        int totalAssets = assetQueue.Count;
        int loadedAssets = 0;
        
        foreach (var info in assetQueue)
        {
            string key = info.assetBundleName;
            AsyncOperationHandle<GameObject> assetRequest = Addressables.LoadAssetAsync<GameObject>(key);
            while (!assetRequest.IsDone)
            {
                // 触发更新事件
                float progress = (loadedAssets + assetRequest.PercentComplete) / totalAssets;
                OnLoadingUpdated?.Invoke(progress);
                yield return null;
            }
            if (assetRequest.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject asset = assetRequest.Result as GameObject;
                if (asset != null)
                {
                    Instantiate(asset);
                }

            }
            else
                Addressables.Release(assetRequest);

            loadedAssets++;
        }

        // 触发完成事件
        OnLoadingCompleted?.Invoke();

        // 加载完成后清空队列
        assetQueue.Clear();
    }

    string GetAssetBundleURL(string bundleName)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WebGLPlayer:
                return Application.streamingAssetsPath + "/AssetBundles/" + bundleName;
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.LinuxPlayer:
                return Application.dataPath + "/StreamingAssets/AssetBundles/" + bundleName;
            default:
                Debug.LogError("Unsupported platform for AssetBundle loading.");
                return "";
        }
    }
}