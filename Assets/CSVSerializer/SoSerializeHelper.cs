using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace DungeonShooter
{
    public static class SoSerializeHelper
    {
        private const char ListDelimiter = '/';

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
        
        public static AssetReferenceT<T> DeserializeAssetReference<T>(string key) where T : UnityEngine.Object
        {
            return new AssetReferenceT<T>(key);
        }

        public static AssetReferenceGameObject DeserializeAssetReferenceGameObject(string key)
        {
            return new AssetReferenceGameObject(key);
        }

        public static string SerializeStrings(IReadOnlyList<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return string.Empty;

            var list = new List<string>(strings.Count);
            foreach (var s in strings)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                list.Add(s.Trim());
            }

            return list.Count == 0 ? string.Empty : string.Join(ListDelimiter, list);
        }

        public static List<string> DeserializeStrings(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
                return new List<string>();

            var split = serialized.Split(ListDelimiter, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>(split.Length);
            foreach (var s in split)
                list.Add(s.Trim());
            return list;
        }

        public static string SerializeAssetReferences<T>(IReadOnlyList<AssetReferenceT<T>> refs) where T : UnityEngine.Object
        {
            if (refs == null || refs.Count == 0)
                return string.Empty;

            var keys = new List<string>(refs.Count);
            foreach (var r in refs)
            {
                if (r == null)
                    continue;
                var key = SerializeAssetReference(r);
                if (!string.IsNullOrWhiteSpace(key))
                    keys.Add(key);
            }

            return keys.Count == 0 ? string.Empty : string.Join(ListDelimiter, keys);
        }

        public static List<AssetReferenceT<T>> DeserializeAssetReferences<T>(string serializedKeys) where T : UnityEngine.Object
        {
            var keys = DeserializeStrings(serializedKeys);
            if (keys.Count == 0)
                return new List<AssetReferenceT<T>>();

            var list = new List<AssetReferenceT<T>>(keys.Count);
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                list.Add(DeserializeAssetReference<T>(key));
            }

            return list;
        }
    }
}

