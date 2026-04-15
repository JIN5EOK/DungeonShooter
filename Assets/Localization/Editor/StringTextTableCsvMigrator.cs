using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace DungeonShooter.Localization.Editor
{
    public static class StringTextTableCsvMigrator
    {
        private const string SourceCsvAssetPath = "Assets/_DataTables/Tables/StringTextTable.csv";
        private const string TargetCollectionName = "StringTable";
        private const string TargetLocaleCode = "ko";

        [MenuItem("Tools/Localization/Migrate StringTextTable.csv -> StringTable (ko)")]
        public static void Migrate()
        {
            var csv = AssetDatabase.LoadAssetAtPath<TextAsset>(SourceCsvAssetPath);
            if (csv == null)
            {
                Debug.LogError($"CSV를 찾지 못했습니다. path={SourceCsvAssetPath}");
                return;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(TargetCollectionName);
            if (collection == null)
            {
                Debug.LogError($"String Table Collection을 찾지 못했습니다. name={TargetCollectionName}");
                return;
            }

            var locale = LocalizationEditorSettings.GetLocale(TargetLocaleCode);
            if (locale == null)
            {
                Debug.LogError($"Locale을 찾지 못했습니다. code={TargetLocaleCode}");
                return;
            }

            var table = collection.GetTable(locale.Identifier) as StringTable;
            if (table == null)
            {
                Debug.LogError($"StringTable을 찾지 못했습니다. collection={TargetCollectionName}, locale={TargetLocaleCode}");
                return;
            }

            var rows = ParseCsv(csv.text);
            if (rows.Count <= 1)
            {
                Debug.LogWarning($"CSV에 데이터가 없습니다. path={SourceCsvAssetPath}");
                return;
            }

            Undo.RecordObjects(new UnityEngine.Object[] { collection.SharedData, table }, "Migrate StringTextTable.csv");

            var addedOrUpdated = 0;
            var skipped = 0;

            // Header: Memo,Id,Text
            for (var i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Count < 3)
                {
                    skipped++;
                    continue;
                }

                var id = row[1]?.Trim();
                if (string.IsNullOrEmpty(id))
                {
                    skipped++;
                    continue;
                }

                var value = row[2] ?? string.Empty;

                var sharedEntry = collection.SharedData.GetEntry(id) ?? collection.SharedData.AddKey(id);
                var tableEntry = table.GetEntry(sharedEntry.Id) ?? table.AddEntry(sharedEntry.Id, value);

                if (!string.Equals(tableEntry.Value, value, StringComparison.Ordinal))
                {
                    tableEntry.Value = value;
                }

                addedOrUpdated++;
            }

            EditorUtility.SetDirty(collection.SharedData);
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(collection.SharedData), ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(table), ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            Debug.Log(
                $"CSV 이관 완료: collection={TargetCollectionName}, locale={TargetLocaleCode}, rows={rows.Count - 1}, applied={addedOrUpdated}, skipped={skipped}");
        }

        private static List<List<string>> ParseCsv(string csvText)
        {
            var result = new List<List<string>>();
            using var reader = new StringReader(csvText);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                result.Add(ParseCsvLine(line));
            }

            return result;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();

            var inQuotes = false;
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                sb.Append(c);
            }

            fields.Add(sb.ToString());
            return fields;
        }
    }
}

