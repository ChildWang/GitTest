// 定义一个类来存储资源信息和优先级
using System.Collections.Generic;

[System.Serializable]
public class AssetInfo
{
    public string assetBundleName;
    public string assetName;
    public int priority;

    // 重写 Equals 方法用于对比
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        AssetInfo other = (AssetInfo)obj;
        return assetBundleName == other.assetBundleName &&
               assetName == other.assetName &&
               priority == other.priority;
    }

    // 重写 GetHashCode 方法
    public override int GetHashCode()
    {
        return (assetBundleName != null ? assetBundleName.GetHashCode() : 0) ^
               (assetName != null ? assetName.GetHashCode() : 0) ^
               priority.GetHashCode();
    }
}

// 用于存储 JSON 配置的类
[System.Serializable]
public class AssetConfig
{
    public List<AssetInfo> assets;
}