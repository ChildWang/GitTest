using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ExampleUsage1 : MonoBehaviour
{
    public AssetBundleManager manager;
    
    void Start()
    {
        manager.OnConfigLoaded += OnConfigLoaded;
        var catalogPath = "http://192.168.1.2:8000/catalog_2025.02.08.03.44.08.json";
        // ����catalog�����ڼ�������¼��ص��н�����Դ����
        Addressables.LoadContentCatalogAsync(catalogPath).Completed += (resLocatorAopHandler) =>
        {
            string configPath = Application.streamingAssetsPath + "/asset_config.json";
            StartCoroutine(manager.LoadConfig(configPath, DataSource.JSON));
        };
    }

    void OnConfigLoaded()
    {
        StartCoroutine(manager.StartLoading());
    }
}