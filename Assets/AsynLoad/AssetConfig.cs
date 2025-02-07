// ����һ�������洢��Դ��Ϣ�����ȼ�
using System.Collections.Generic;

[System.Serializable]
public class AssetInfo
{
    public string assetBundleName;
    public string assetName;
    public int priority;

    // ��д Equals �������ڶԱ�
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        AssetInfo other = (AssetInfo)obj;
        return assetBundleName == other.assetBundleName &&
               assetName == other.assetName &&
               priority == other.priority;
    }

    // ��д GetHashCode ����
    public override int GetHashCode()
    {
        return (assetBundleName != null ? assetBundleName.GetHashCode() : 0) ^
               (assetName != null ? assetName.GetHashCode() : 0) ^
               priority.GetHashCode();
    }
}

// ���ڴ洢 JSON ���õ���
[System.Serializable]
public class AssetConfig
{
    public List<AssetInfo> assets;
}