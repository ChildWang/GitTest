// 定义一个 ScriptableObject 类来存储配置
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetConfigSO", menuName = "Asset Bundle/Asset Config ScriptableObject")]
public class AssetConfigSO : ScriptableObject
{
    public List<AssetInfo> assets;
}
