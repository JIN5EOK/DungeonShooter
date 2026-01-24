using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

namespace DungeonShooter
{
    /// <summary>
    /// CSV 파일을 파싱하여 테이블 엔트리로 변환하는 파서
    /// </summary>
    public static class CSVTableParser
    {
        /// <summary>
        /// CSV TextAsset을 파싱하여 테이블 엔트리 리스트로 변환합니다.
        /// </summary>
        /// <typeparam name="T">테이블 엔트리 타입</typeparam>
        /// <param name="csvTextAsset">CSV 파일 TextAsset</param>
        /// <returns>파싱된 테이블 엔트리 리스트</returns>
        public static List<T> Parse<T>(TextAsset csvTextAsset) where T : class, new()
        {
            if (csvTextAsset == null)
            {
                LogHandler.LogError(nameof(CSVTableParser), "CSV TextAsset이 null입니다.");
                return new List<T>();
            }

            var lines = csvTextAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length < 2)
            {
                LogHandler.LogError(nameof(CSVTableParser), "CSV 파일에 헤더 또는 데이터가 없습니다.");
                return new List<T>();
            }

            var headerLine = lines[0];
            var headers = headerLine.Split(',').Select(h => h.Trim()).ToArray();

            var entries = new List<T>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var values = lines[i].Split(',');
                if (values.Length != headers.Length)
                {
                    LogHandler.LogWarning(nameof(CSVTableParser),$"라인 {i + 1}: 헤더와 값의 개수가 맞지 않습니다. 스킵합니다.");
                    continue;
                }

                try
                {
                    var entry = ParseLine<T>(headers, values);
                    if (entry != null)
                        entries.Add(entry);
                }
                catch (Exception ex)
                {
                    LogHandler.LogWarning(nameof(CSVTableParser),$"라인 {i + 1} 파싱 실패: {ex.Message}");
                }
            }

            return entries;
        }

        /// <summary>
        /// CSV 라인을 파싱하여 테이블 엔트리로 변환합니다.
        /// </summary>
        private static T ParseLine<T>(string[] headers, string[] values) where T : class, new()
        {
            var entry = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < headers.Length; i++)
            {
                var headerName = headers[i].Trim();
                var value = values[i].Trim();

                if (string.IsNullOrEmpty(value))
                    continue;

                var property = properties.FirstOrDefault(p => p.Name == headerName && p.CanWrite);
                if (property == null)
                    continue;

                try
                {
                    SetPropertyValue(entry, property, value);
                }
                catch (Exception ex)
                {
                    LogHandler.LogWarning(nameof(CSVTableParser),$"속성 '{headerName}' 설정 실패: {ex.Message}");
                }
            }

            return entry;
        }

        /// <summary>
        /// 속성에 값을 설정합니다.
        /// </summary>
        private static void SetPropertyValue<T>(T entry, PropertyInfo property, string value) where T : class
        {
            var propertyType = property.PropertyType;

            // Dictionary<string, float> 처리 (SkillTableEntry.FloatAmounts)
            if (propertyType == typeof(Dictionary<string, float>))
            {
                var floatAmountsDict = ParseAmountsDictionary<float>(value);
                property.SetValue(entry, floatAmountsDict);
                return;
            }

            // Dictionary<string, int> 처리 (SkillTableEntry.IntAmounts)
            if (propertyType == typeof(Dictionary<string, int>))
            {
                var intAmountsDict = ParseAmountsDictionary<int>(value);
                property.SetValue(entry, intAmountsDict);
                return;
            }

            // enum 타입 처리
            if (propertyType.IsEnum)
            {
                var enumValue = Enum.Parse(propertyType, value, ignoreCase: true);
                property.SetValue(entry, enumValue);
                return;
            }

            // 기본 타입 처리
            var convertedValue = Convert.ChangeType(value, propertyType);
            property.SetValue(entry, convertedValue);
        }

        /// <summary>
        /// JSON 형식의 Amounts 문자열을 Dictionary<string, T>로 파싱합니다.
        /// 예: {"damage":30,"heal":10} 또는 {"range":5.0,"speed":2.5}
        /// </summary>
        private static Dictionary<string, T> ParseAmountsDictionary<T>(string json) where T : struct
        {
            var amountsDict = new Dictionary<string, T>();

            if (string.IsNullOrEmpty(json))
                return amountsDict;

            try
            {
                // Newtonsoft.Json으로 Dictionary 직접 파싱
                var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
                
                if (jsonDict != null)
                {
                    foreach (var kvp in jsonDict)
                    {
                        amountsDict[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                var typeName = typeof(T).Name;
                LogHandler.LogError(nameof(CSVTableParser), $"{typeName} JSON 파싱 실패: {json}, 에러: {ex.Message}");
            }

            return amountsDict;
        }
    }
}
