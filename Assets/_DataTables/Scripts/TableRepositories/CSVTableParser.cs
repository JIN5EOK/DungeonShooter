using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// CSV 파일을 파싱하여 테이블 엔트리로 변환하는 파서
    /// 큰따옴표(")로 감싼 필드 안의 쉼표는 셀 구분자로 사용하지 않으며, ""는 하나의 "로 이스케이프됩니다.
    /// 클래스·구조체는 셀에 "이름:값/이름:값" 형식으로 기입합니다 (public 프로퍼티·필드, 값에 ':'가 있으면 첫 ':'만 구분자).
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
            var headers = SplitCsvLine(headerLine).Select(h => h.Trim()).ToArray();

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

                var values = SplitCsvLine(lines[i]);
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
        /// CSV 한 줄을 셀 단위로 분리합니다. 큰따옴표(")로 감싼 부분 안의 쉼표는 구분자로 사용하지 않으며,
        /// ""는 하나의 "로 치환됩니다.
        /// </summary>
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        inQuotes = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(current.ToString().Trim());
                        current.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        /// <summary>
        /// CSV 라인을 파싱하여 테이블 엔트리로 변환합니다.
        /// </summary>
        private static T ParseLine<T>(string[] headers, string[] values) where T : class, new()
        {
            var entry = new T();
            var entryType = typeof(T);
            var properties = entryType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = entryType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < headers.Length; i++)
            {
                var headerName = headers[i].Trim();
                var value = values[i].Trim();

                if (string.IsNullOrEmpty(value))
                    continue;

                // 이름과 일치하는 프로퍼티 찾기 시도
                var property = properties.FirstOrDefault(p => p.Name == headerName && p.CanWrite);
                if (property != null)
                {
                    try
                    {
                        SetPropertyValue(entry, property, value);
                    }
                    catch (Exception ex)
                    {
                        LogHandler.LogWarning(nameof(CSVTableParser), $"속성 '{headerName}' 설정 실패: {ex.Message}");
                    }

                    continue;
                }

                // 이름과 일치하는 필드 찾기 시도
                var field = fields.FirstOrDefault(f => f.Name == headerName && !f.IsInitOnly);
                if (field != null)
                {
                    try
                    {
                        SetFieldValue(entry, field, value);
                    }
                    catch (Exception ex)
                    {
                        LogHandler.LogWarning(nameof(CSVTableParser), $"필드 '{headerName}' 설정 실패: {ex.Message}");
                    }
                }
            }

            return entry;
        }

        /// <summary>
        /// 속성에 값을 설정합니다.
        /// </summary>
        private static void SetPropertyValue(object entry, PropertyInfo property, string value)
        {
            AssignParsedValue(property.PropertyType, value, v => property.SetValue(entry, v));
        }

        /// <summary>
        /// 필드에 값을 설정합니다.
        /// </summary>
        private static void SetFieldValue(object entry, FieldInfo field, string value)
        {
            AssignParsedValue(field.FieldType, value, v => field.SetValue(entry, v));
        }

        /// <summary>
        /// 문자열을 파싱한 뒤 대상 멤버 타입에 맞게 할당합니다.
        /// </summary>
        private static void AssignParsedValue(Type memberType, string value, Action<object> assign)
        {
            // Dictionary<K,V> 일괄 처리 (예: "key:value/key:value")
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var args = memberType.GetGenericArguments();
                assign(ParseDictionary(value, args[0], args[1]));
                return;
            }

            // List<T> 일괄 처리 (예: "18000000/18000001" 또는 "a/b/c")
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = memberType.GetGenericArguments()[0];
                assign(ParseList(value, elementType));
                return;
            }

            // enum 타입 처리
            if (memberType.IsEnum)
            {
                assign(Enum.Parse(memberType, value, ignoreCase: true));
                return;
            }

            // 클래스·구조체 (예: "MaxHp:100/Attack:10/MoveSpeed:5") — 이름은 CSV·복합 셀과 동일하게 대소문자 구분
            if (IsCompositePropertyType(memberType))
            {
                assign(ParseCompositeObject(value, memberType));
                return;
            }

            // 기본 타입 처리
            assign(Convert.ChangeType(value, memberType));
        }

        /// <summary>
        /// CSV 셀 하나로 묶어 파싱할 수 있는 단순 타입이면 true (문자열·숫자·날짜 등).
        /// </summary>
        private static bool IsSimpleScalarType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (underlying == typeof(string))
                return true;
            if (underlying.IsPrimitive)
                return true;
            if (underlying == typeof(decimal))
                return true;
            if (underlying == typeof(DateTime))
                return true;
            if (underlying == typeof(TimeSpan))
                return true;
            if (underlying == typeof(Guid))
                return true;
            return false;
        }

        /// <summary>
        /// public 프로퍼티·필드를 "이름:값/이름:값" 형식으로 채울 수 있는 복합 타입인지 여부입니다.
        /// </summary>
        private static bool IsCompositePropertyType(Type type)
        {
            if (type.IsEnum)
                return false;
            if (IsSimpleScalarType(type))
                return false;
            return true;
        }

        /// <summary>
        /// "이름:값/이름:값" 형식 문자열을 대상 타입 인스턴스로 파싱합니다.
        /// 값에 ':'가 포함되면 첫 번째 ':'만 구분자로 사용합니다.
        /// </summary>
        private static object ParseCompositeObject(string data, Type targetType)
        {
            var instance = Activator.CreateInstance(targetType);
            if (string.IsNullOrEmpty(data))
                return instance;

            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            try
            {
                var segments = data.Split('/');
                foreach (var segment in segments)
                {
                    var trimmed = segment.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    var colonIndex = trimmed.IndexOf(':');
                    if (colonIndex < 0)
                        continue;

                    var name = trimmed.Substring(0, colonIndex).Trim();
                    var fieldValue = trimmed.Substring(colonIndex + 1);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    var valueTrimmed = fieldValue.Trim();

                    var prop = properties.FirstOrDefault(p => p.Name == name && p.CanWrite);
                    if (prop != null)
                    {
                        try
                        {
                            SetPropertyValue(instance, prop, valueTrimmed);
                        }
                        catch (Exception ex)
                        {
                            LogHandler.LogWarning(nameof(CSVTableParser), $"복합 타입 '{targetType.Name}' 속성 '{name}' 설정 실패: {ex.Message}");
                        }

                        continue;
                    }

                    var fld = fields.FirstOrDefault(f => f.Name == name && !f.IsInitOnly);
                    if (fld == null)
                        continue;

                    try
                    {
                        SetFieldValue(instance, fld, valueTrimmed);
                    }
                    catch (Exception ex)
                    {
                        LogHandler.LogWarning(nameof(CSVTableParser), $"복합 타입 '{targetType.Name}' 필드 '{name}' 설정 실패: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError(nameof(CSVTableParser), $"복합 타입 파싱 실패: {data}, 타입: {targetType.Name}, 에러: {ex.Message}");
            }

            return instance;
        }

        /// <summary>
        /// "key:value/key:value" 형식 문자열을 Dictionary로 파싱합니다.
        /// 예: "damage:30/heal:10", "1:10/2:20/3:30"
        /// </summary>
        /// <param name="data">파싱할 문자열 ('/'로 쌍 구분, ':'로 키·값 구분)</param>
        /// <param name="keyType">딕셔너리 키 타입 (string, int 등)</param>
        /// <param name="valueType">딕셔너리 값 타입</param>
        private static object ParseDictionary(string data, Type keyType, Type valueType)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var result = Activator.CreateInstance(dictType);
            var dict = (System.Collections.IDictionary)result;

            if (string.IsNullOrEmpty(data))
                return result;

            try
            {
                var pairs = data.Split('/');
                foreach (var pair in pairs)
                {
                    var trimmedPair = pair.Trim();
                    if (string.IsNullOrEmpty(trimmedPair))
                        continue;

                    var parts = trimmedPair.Split(':');
                    if (parts.Length != 2)
                        continue;

                    var keyString = parts[0].Trim();
                    var valueString = parts[1].Trim();
                    if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(valueString))
                        continue;

                    var key = keyType == typeof(string) ? keyString : Convert.ChangeType(keyString, keyType);
                    var value = Convert.ChangeType(valueString, valueType);
                    dict.Add(key, value);
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError(nameof(CSVTableParser), $"Dictionary 파싱 실패: {data}, 에러: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// '/' 구분 문자열을 List로 파싱합니다.
        /// 예: "18000000/18000001/18000002", "a/b/c"
        /// </summary>
        /// <param name="data">파싱할 문자열 ('/'로 요소 구분)</param>
        /// <param name="elementType">리스트 요소 타입</param>
        private static object ParseList(string data, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var result = Activator.CreateInstance(listType);
            var list = (System.Collections.IList)result;

            if (string.IsNullOrEmpty(data))
                return result;

            try
            {
                var parts = data.Split('/');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    var item = elementType == typeof(string) ? trimmed : Convert.ChangeType(trimmed, elementType);
                    list.Add(item);
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError(nameof(CSVTableParser), $"List 파싱 실패: {data}, 에러: {ex.Message}");
            }

            return result;
        }
    }
}
