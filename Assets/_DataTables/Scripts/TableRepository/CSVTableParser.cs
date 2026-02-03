using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// CSV 파일을 파싱하여 테이블 엔트리로 변환하는 파서
    /// </summary>
    public static class CSVTableParser
    {
        /// <summary>
        /// 파싱에서 제외되는 메모 열의 헤더 이름. 첫 번째 열이 이 이름이면 실제 파싱 대상에서 제외됩니다.
        /// </summary>
        private const string MemoColumnName = "Memo";

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

            var parseHeaders = headers;
            if (headers.Length > 0 && string.Equals(headers[0], MemoColumnName, StringComparison.Ordinal))
            {
                parseHeaders = headers.Skip(1).ToArray();
            }

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

                var parseValues = values;
                if (parseHeaders.Length < headers.Length)
                {
                    parseValues = values.Skip(1).ToArray();
                }

                try
                {
                    var entry = ParseLine<T>(parseHeaders, parseValues);
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
        /// 커스텀 형식의 Amounts 문자열을 Dictionary<string, T>로 파싱합니다.
        /// 예: "damage:30/heal:10" 또는 "range:5.0/speed:2.5"
        /// </summary>
        private static Dictionary<string, T> ParseAmountsDictionary<T>(string data) where T : struct
        {
            var result = new Dictionary<string, T>();
            
            if (string.IsNullOrEmpty(data))
                return result;

            try
            {
                // '/' 구분자로 각 키-값 쌍 분리
                var pairs = data.Split('/');
                
                foreach (var pair in pairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;
                    
                    // ':' 구분자로 키와 값 분리
                    var parts = trimmedPair.Split(':');
                    if (parts.Length != 2)
                        continue;
                    
                    var key = parts[0].Trim();
                    var valueString = parts[1].Trim();
                    
                    if (string.IsNullOrEmpty(key))
                        continue;
                    
                    // 타입 변환
                    var value = (T)Convert.ChangeType(valueString, typeof(T));
                    result[key] = value;
                }
            }
            catch (Exception ex)
            {
                var typeName = typeof(T).Name;
                LogHandler.LogError(nameof(CSVTableParser), $"{typeName} 커스텀 파싱 실패: {data}, 에러: {ex.Message}");
            }

            return result;
        }
    }
}
