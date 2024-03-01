using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityAddressableImporter.Helper;

public class AddressableImportSettingsList : ScriptableObject
{
    public const string kConfigObjectName = "addressableimportsettingslist";
    public const string kDefaultPath = "Assets/AddressableAssetsData/AddressableImportSettingsList.asset";
    public List<AddressableImportSettings> SettingList;
    public List<AddressableImportSettings> EnabledSettingsList => SettingList.Where((s) => s?.rulesEnabled == true).ToList();
    public bool ShowImportProgressBar = true;

    public static AddressableImportSettingsList Instance
    {
        get
        {
            AddressableImportSettingsList so;

            // Try to locate settings list from EditorBuildSettings
            EditorBuildSettings.TryGetConfigObject(kConfigObjectName, out so);
            if (so != null)
            {
                return so;
            }
            // Try to locate settings list from the default path
            so = AssetDatabase.LoadAssetAtPath<AddressableImportSettingsList>(kDefaultPath);
            if (so != null)
            {
                EditorBuildSettings.AddConfigObject(kConfigObjectName, so, true);
                return so;
            }
            // Try to locate settings list from AssetDatabase
            var guidList = AssetDatabase.FindAssets($"t:{nameof(AddressableImportSettingsList)}");
            if (guidList.Length > 0)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guidList[0]);
                so = AssetDatabase.LoadAssetAtPath<AddressableImportSettingsList>(assetPath);
                EditorBuildSettings.AddConfigObject(kConfigObjectName, so, true);
                return so;
            }

            // If AddressableImportSettingsList doesn't exist but AddressableImportSettings exists, create the list.
            var importSettingsGuidList = AssetDatabase.FindAssets($"t:{nameof(AddressableImportSettings)}");
            if (importSettingsGuidList.Length > 0)
            {
                var settingList = importSettingsGuidList.Select((guid) => AssetDatabase.LoadAssetAtPath<AddressableImportSettings>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
                var asset = ScriptableObject.CreateInstance<AddressableImportSettingsList>();
                asset.SettingList = settingList;
                var path = Path.Combine(Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(importSettingsGuidList[0])),
                                        nameof(AddressableImportSettingsList) + ".asset");
                Debug.LogFormat("Created AddressableImportSettingsList at path: {0}", path);
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                so = asset;
                EditorBuildSettings.AddConfigObject(kConfigObjectName, so, true);
                return so;
            }

            return null;
        }
    }

    public bool RemoveMissingImportSettings()
    {
        bool removedAnySettings = false;
        for (int i = SettingList.Count - 1; i >= 0; --i)
        {
            if (SettingList[i] == null)
            {
                SettingList.RemoveAt(i);
                removedAnySettings = true;
            }
        }

        return removedAnySettings;
    }

    [ButtonMethod]
    public void RebuildSettingsList()
    {
        var importSettingsGuidList = AssetDatabase.FindAssets($"t:{nameof(AddressableImportSettings)}");
        SettingList = importSettingsGuidList.Select((guid) => AssetDatabase.LoadAssetAtPath<AddressableImportSettings>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    [ButtonMethod]
    public void Documentation()
    {
        Application.OpenURL("https://github.com/favoyang/unity-addressable-importer/blob/master/Documentation~/AddressableImporter.md#multiple-addressableimportsettings-support");
    }
}
