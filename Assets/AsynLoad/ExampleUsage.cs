using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    private List<ABVersionCatalog> allVersionCatalogs;
    public AssetBundleManager manager;

    void Start()
    {
        manager.OnConfigLoaded += OnConfigLoaded;
        string configPath = Application.streamingAssetsPath + "/asset_config.json";        
        StartCoroutine(manager.LoadConfig(configPath, DataSource.JSON));
        
    }

    void OnConfigLoaded()
    {
        StartCoroutine(manager.StartLoading());
    }
}