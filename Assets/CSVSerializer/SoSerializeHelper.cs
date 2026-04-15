using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace DungeonShooter
{
    public static class SoSerializeHelper
    {
        public static string SerializeLocalizedString(LocalizedString localizedString)
        {
            return $"{localizedString.TableReference.TableCollectionName}/{localizedString.TableEntryReference.KeyId}";
        }
        
        public static LocalizedString DeserializeLocalizedString(string serializedLocalizedString)
        {
            var split = serializedLocalizedString.Split('/');
            return new(split[0], long.Parse(split[1]));
        }

        public static string SerializeAssetReference(AssetReference assetRef)
        {
            return assetRef.AssetGUID.ToString();
        }
        
        public static AssetReferenceT<T> DeserializeAssetReference<T>(string key) where T : Object
        {
            return new AssetReferenceT<T>(key);
        }
    }
}

