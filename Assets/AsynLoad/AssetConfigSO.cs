// ����һ�� ScriptableObject �����洢����
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetConfigSO", menuName = "Asset Bundle/Asset Config ScriptableObject")]
public class AssetConfigSO : ScriptableObject
{
    public List<AssetInfo> assets;
}
