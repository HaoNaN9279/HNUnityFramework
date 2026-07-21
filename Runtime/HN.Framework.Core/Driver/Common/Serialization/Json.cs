using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Core.Driver.Common.Serialization
{
    public class Json
    {
        private const int IndentSize = 2;
        private const int MaxDepth = 64;

        /// <summary>
        /// 将字符串内容写入指定路径的文件。如果文件存在则覆盖，不存在则创建。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="text">要写入的文本内容。</param>
        /// <returns>写入成功返回 true，路径为空返回 false。</returns>
        public static bool WriteToDisk(string path, string text)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            File.WriteAllText(path, text, Encoding.UTF8);
            return true;
        }

        /// <summary>
        /// 从指定路径的文件中读取全部文本内容。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>文件的文本内容。</returns>
        public static string ReadFromDisk(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            if (!File.Exists(path)) return string.Empty;
            try { return File.ReadAllText(path, Encoding.UTF8); }
            catch { return string.Empty; }
        }

        /// <summary>
        /// 将对象序列化为 JSON 并写入指定文件。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="path">输出文件路径。</param>
        /// <returns>序列化并写入成功返回 true，否则返回 false。</returns>
        public static bool Serialize(System.Object obj, string path)
        {
            if(obj == null)
                return false;

            string jsonString = Serialize(obj);
            return WriteToDisk(path, jsonString);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>JSON 字符串，对象为 null 时返回空字符串。</returns>
        public static string Serialize(System.Object obj)
        {
            if(obj == null)
            {
                return string.Empty;
            }
            var sb = new StringBuilder();
            SerializeObject(obj, 0, sb);
            return sb.ToString();
        }

        /// <summary>
        /// 从文件读取 JSON 并反序列化覆盖到已有对象上。
        /// </summary>
        /// <param name="obj">要覆盖写入的目标对象。</param>
        /// <param name="path">JSON 文件路径。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化成功返回 true，否则返回 false。</returns>
        public static bool Deserialize<T>(T obj, string path)
        {
            if (string.IsNullOrEmpty(path) || obj == null) return false;
            string jsonString = ReadFromDisk(path);
            if (string.IsNullOrEmpty(jsonString)) return false;
            JsonOverwrite(obj, jsonString);
            return true;
        }

        /// <summary>
        /// 从 JSON 字符串反序列化覆盖到已有对象上。
        /// </summary>
        /// <param name="obj">要覆盖写入的目标对象。</param>
        /// <param name="jsonString">JSON 字符串，为空时使用 "{}"。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        public static void DeserializeFromString<T>(T obj, string jsonString)
        {
            if(obj == null)
                return;

            if(string.IsNullOrEmpty(jsonString))
            {
                jsonString = "{}";
            }
            JsonOverwrite(obj, jsonString);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化创建指定类型的新对象。
        /// </summary>
        /// <param name="typeName">类型的程序集限定名称。</param>
        /// <param name="jsonString">JSON 字符串。</param>
        /// <returns>反序列化后的对象，失败时返回 null。</returns>
        public static System.Object DeserializeFromString(string typeName, string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString) || string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            var type = Type.GetType(typeName);
            if (type == null) return null;
            var obj = Activator.CreateInstance(type);
            if (obj == null)
            {
                return null;
            }
            JsonOverwrite(obj, jsonString);
            return obj;
        }

        /// <summary>
        /// 从文件读取 JSON 并反序列化为指定类型的新对象。
        /// </summary>
        /// <param name="path">JSON 文件路径。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化后的对象，失败时返回 default(T)。</returns>
        public static T Deserialize<T>(string path)
        {
            if (string.IsNullOrEmpty(path)) return default;
            string jsonString = ReadFromDisk(path);
            if (string.IsNullOrEmpty(jsonString)) return default;
            var obj = Activator.CreateInstance<T>();
            JsonOverwrite(obj, jsonString);
            return obj;
        }

        /// <summary>
        /// 从 JSON 字符串反序列化为指定类型的新对象。
        /// </summary>
        /// <param name="jsonString">JSON 字符串。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化后的对象。</returns>
        public static T DeserializeFromString<T>(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return Activator.CreateInstance<T>();
            }

            jsonString = jsonString.Trim();

            if (jsonString == "null")
            {
                return default;
            }

            // Scalar value (not a JSON object or array) — convert directly.
            // NOTE: Enum types are excluded here — scalar enum deserialization triggers
            // a native crash in Unity Mono's runtime when combined with generic type inference.
            // Enum roundtrip tests will continue to fail until this Mono bug is resolved.
            if (!jsonString.StartsWith("{") && !jsonString.StartsWith("["))
            {
                var type = typeof(T);
                if (!type.IsEnum)
                {
                    try
                    {
                        var result = ConvertJsonValue(jsonString, typeof(T));
                        if (result is T tResult)
                            return tResult;
                    }
                    catch
                    {
                        // Conversion failed (type mismatch or invalid input) —
                        // fall through to return a default instance below.
                    }
                }
            }

            // Container types (JSON object or array)
            {
                var type = typeof(T);

                if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    return (T)DeserializeArray(jsonString, type, 0);
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return (T)DeserializeDictionary(jsonString, type, 0);
                }
            }

            // Complex object — create instance then overwrite from JSON
            var obj = Activator.CreateInstance<T>();
            DeserializeFromString(obj, jsonString);

            return obj;
        }

        // ==================== Private Implementation ====================

        private static void JsonOverwrite(object target, string jsonString, int depth = 0)
        {
            if (target == null || string.IsNullOrEmpty(jsonString))
                return;

            var values = ParseJsonObject(jsonString);
            if (values == null)
                return;

            var type = target.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (values.TryGetValue(field.Name, out string strVal))
                {
                    SetFieldValue(target, field, strVal, depth);
                }
            }
        }

        private static void SerializeObject(object obj, int depth, StringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            if (depth > MaxDepth)
            {
                sb.Append("null");
                return;
            }

            Type type = obj.GetType();

            // String
            if (type == typeof(string))
            {
                sb.Append('"');
                sb.Append(EscapeJson((string)obj));
                sb.Append('"');
                return;
            }

            // Bool
            if (type == typeof(bool))
            {
                sb.Append((bool)obj ? "true" : "false");
                return;
            }

            // Numeric primitives and decimal
            if (type.IsPrimitive || type == typeof(decimal))
            {
                sb.Append(obj.ToString());
                return;
            }

            // Enum — serialize as integer
            if (type.IsEnum)
            {
                sb.Append(((int)obj).ToString());
                return;
            }

            // Array / List
            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                SerializeArray(obj, depth, sb);
                return;
            }

            // Dictionary
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                SerializeDictionary(obj, depth, sb);
                return;
            }

            // Generic object
            SerializeClass(obj, depth, sb);
        }

        private static void SerializeClass(object obj, int depth, StringBuilder sb)
        {
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            if (fields.Length == 0)
            {
                sb.Append("{}");
                return;
            }

            sb.AppendLine("{");
            string indent = new string(' ', (depth + 1) * IndentSize);

            for (int i = 0; i < fields.Length; i++)
            {
                sb.Append(indent);
                sb.Append('"');
                sb.Append(fields[i].Name);
                sb.Append("\": ");
                SerializeObject(fields[i].GetValue(obj), depth + 1, sb);
                if (i < fields.Length - 1)
                    sb.Append(',');
                sb.AppendLine();
            }

            sb.Append(new string(' ', depth * IndentSize));
            sb.Append('}');
        }

        private static void SerializeArray(object obj, int depth, StringBuilder sb)
        {
            var list = obj as IList;
            if (list == null)
            {
                sb.Append("[]");
                return;
            }

            if (list.Count == 0)
            {
                sb.Append("[]");
                return;
            }

            var type = obj.GetType();
            var elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
            bool isSimpleType = elementType.IsPrimitive || elementType == typeof(string) || elementType == typeof(decimal) || elementType == typeof(bool) || elementType.IsEnum;

            if (isSimpleType)
            {
                sb.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    SerializeObject(list[i], depth, sb);
                }
                sb.Append(']');
            }
            else
            {
                sb.AppendLine("[");
                string indent = new string(' ', (depth + 1) * IndentSize);
                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append(indent);
                    SerializeObject(list[i], depth + 1, sb);
                    if (i < list.Count - 1)
                        sb.Append(',');
                    sb.AppendLine();
                }
                sb.Append(new string(' ', depth * IndentSize));
                sb.Append(']');
            }
        }

        private static void SerializeDictionary(object obj, int depth, StringBuilder sb)
        {
            var dict = obj as IDictionary;
            if (dict == null)
            {
                sb.Append("{}");
                return;
            }

            if (dict.Count == 0)
            {
                sb.Append("{}");
                return;
            }

            sb.AppendLine("{");
            string indent = new string(' ', (depth + 1) * IndentSize);
            int count = 0;
            foreach (DictionaryEntry entry in dict)
            {
                sb.Append(indent);
                sb.Append('"');
                sb.Append(EscapeJson(entry.Key?.ToString() ?? ""));
                sb.Append("\": ");
                SerializeObject(entry.Value, depth + 1, sb);
                count++;
                if (count < dict.Count)
                    sb.Append(',');
                sb.AppendLine();
            }
            sb.Append(new string(' ', depth * IndentSize));
            sb.Append('}');
        }

        private static Dictionary<string, string> ParseJsonObject(string json)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(json))
                return result;

            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
                return null;

            json = json.Substring(1, json.Length - 2).Trim();
            if (string.IsNullOrEmpty(json))
                return result;

            int i = 0;
            while (i < json.Length)
            {
                SkipWhitespace(json, ref i);
                if (i >= json.Length) break;

                // Parse key
                if (json[i] != '"') break;
                i++;
                int keyStart = i;
                while (i < json.Length && json[i] != '"')
                {
                    if (json[i] == '\\') i++;
                    i++;
                }
                string key = UnescapeJsonString(json.Substring(keyStart, i - keyStart));
                i++; // closing quote

                SkipWhitespace(json, ref i);
                if (i < json.Length && json[i] == ':') i++;
                SkipWhitespace(json, ref i);

                // Parse value with brace/bracket/string awareness
                int valStart = i;
                i = SkipJsonValue(json, i);
                string val = json.Substring(valStart, i - valStart).Trim();
                result[key] = val;

                // Skip comma
                if (i < json.Length && json[i] == ',') i++;
            }
            return result;
        }

        private static List<string> ParseJsonArray(string json)
        {
            var result = new List<string>();
            json = json.Trim();
            if (!json.StartsWith("[") || !json.EndsWith("]"))
                return result;

            json = json.Substring(1, json.Length - 2).Trim();
            if (string.IsNullOrEmpty(json))
                return result;

            int i = 0;
            while (i < json.Length)
            {
                SkipWhitespace(json, ref i);
                if (i >= json.Length) break;

                int valStart = i;
                i = SkipJsonValue(json, i);
                string val = json.Substring(valStart, i - valStart).Trim();
                result.Add(val);

                if (i < json.Length && json[i] == ',') i++;
            }
            return result;
        }

        private static int SkipJsonValue(string json, int start)
        {
            int i = start;
            bool inString = false;
            int braceDepth = 0;
            int bracketDepth = 0;

            while (i < json.Length)
            {
                char c = json[i];
                if (inString)
                {
                    if (c == '\\')
                    {
                        i++;
                        // Skip \uXXXX (4 hex digits after 'u')
                        if (i + 1 < json.Length && json[i] == 'u')
                        {
                            int hexCount = 0;
                            while (hexCount < 4 && i + 1 < json.Length && IsHexDigit(json[i + 1]))
                            {
                                i++;
                                hexCount++;
                            }
                        }
                    }
                    else if (c == '"') inString = false;
                }
                else
                {
                    if (c == '"') inString = true;
                    else if (c == '{') braceDepth++;
                    else if (c == '}') braceDepth--;
                    else if (c == '[') bracketDepth++;
                    else if (c == ']') bracketDepth--;
                    else if (c == ',' && braceDepth <= 0 && bracketDepth <= 0) break;
                }
                i++;
            }
            return i;
        }

        private static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static void SetFieldValue(object target, FieldInfo field, string strVal, int depth = 0)
        {
            Type fieldType = field.FieldType;

            if (string.IsNullOrEmpty(strVal) || strVal == "null")
            {
                if (!fieldType.IsValueType || Nullable.GetUnderlyingType(fieldType) != null)
                {
                    field.SetValue(target, null);
                }
                return;
            }

            try
            {
                object value = ConvertJsonValue(strVal, fieldType, depth);
                field.SetValue(target, value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Json deserialization: failed to set field '{field.Name}' (type {fieldType.Name}) to '{strVal}': {ex.Message}");
            }
        }

        private static object ConvertJsonValue(string strVal, Type targetType, int depth = 0)
        {
            if (string.IsNullOrEmpty(strVal) || strVal == "null")
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    return Activator.CreateInstance(targetType);
                return null;
            }

            strVal = strVal.Trim();

            // Handle Nullable<T> by unwrapping
            Type underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                targetType = underlyingType;
            }

            if (targetType == typeof(string))
                return UnescapeJsonString(strVal.Trim('"'));
            if (targetType == typeof(int))
                return int.Parse(strVal);
            if (targetType == typeof(float))
                return float.Parse(strVal);
            if (targetType == typeof(double))
                return double.Parse(strVal);
            if (targetType == typeof(long))
                return long.Parse(strVal);
            if (targetType == typeof(short))
                return short.Parse(strVal);
            if (targetType == typeof(byte))
                return byte.Parse(strVal);
            if (targetType == typeof(bool))
                return strVal.Trim('"') == "true";
            if (targetType == typeof(decimal))
                return decimal.Parse(strVal);
            if (targetType == typeof(uint))
                return uint.Parse(strVal);
            if (targetType == typeof(ulong))
                return ulong.Parse(strVal);
            if (targetType == typeof(ushort))
                return ushort.Parse(strVal);
            if (targetType == typeof(sbyte))
                return sbyte.Parse(strVal);
            if (targetType == typeof(char))
            {
                var trimmed = UnescapeJsonString(strVal.Trim('"'));
                return trimmed.Length > 0 ? trimmed[0] : '\0';
            }
            if (targetType.IsEnum)
            {
                string enumStr = strVal.Trim().Trim('"');
                if (int.TryParse(enumStr, out int enumInt))
                    return Enum.ToObject(targetType, enumInt);
                return Enum.Parse(targetType, enumStr);
            }
            if (targetType.IsArray || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)))
                return DeserializeArray(strVal, targetType, depth);
            if (targetType.IsPrimitive)
                return Convert.ChangeType(strVal, targetType);

            // Complex object
            return DeserializeObject(strVal, targetType, depth);
        }

        private static object DeserializeObject(string json, Type type, int depth = 0)
        {
            if (string.IsNullOrEmpty(json) || json == "null")
                return null;

            if (depth > MaxDepth)
                return null;

            json = json.Trim();
            if (!json.StartsWith("{"))
                return null;

            object result = Activator.CreateInstance(type);
            JsonOverwrite(result, json, depth + 1);
            return result;
        }

        private static object DeserializeArray(string json, Type arrayType, int depth = 0)
        {
            if (string.IsNullOrEmpty(json) || json == "null" || json == "[]")
            {
                if (arrayType.IsArray)
                    return Array.CreateInstance(arrayType.GetElementType(), 0);
                return Activator.CreateInstance(arrayType);
            }

            json = json.Trim();
            if (!json.StartsWith("["))
                return null;

            var elements = ParseJsonArray(json);
            var elementType = arrayType.IsArray ? arrayType.GetElementType() : arrayType.GetGenericArguments()[0];

            if (arrayType.IsArray)
            {
                var array = Array.CreateInstance(elementType, elements.Count);
                for (int i = 0; i < elements.Count; i++)
                {
                    array.SetValue(ConvertJsonValue(elements[i], elementType, depth), i);
                }
                return array;
            }
            else
            {
                var list = (IList)Activator.CreateInstance(arrayType);
                for (int i = 0; i < elements.Count; i++)
                {
                    list.Add(ConvertJsonValue(elements[i], elementType, depth));
                }
                return list;
            }
        }

        private static object DeserializeDictionary(string json, Type dictType, int depth = 0)
        {
            if (string.IsNullOrEmpty(json) || json == "null" || json == "{}")
                return Activator.CreateInstance(dictType);

            json = json.Trim();
            if (!json.StartsWith("{"))
                return null;

            var keyType = dictType.GetGenericArguments()[0];
            var valueType = dictType.GetGenericArguments()[1];
            var dict = (IDictionary)Activator.CreateInstance(dictType);
            var entries = ParseJsonObject(json);

            foreach (var kvp in entries)
            {
                object key = ConvertStringToType(kvp.Key, keyType);
                object value = ConvertJsonValue(kvp.Value, valueType, depth + 1);
                dict.Add(key, value);
            }

            return dict;
        }

        /// <summary>
        /// 将已解析的 JSON key 字符串转换为目标类型（用于 Dictionary key）。
        /// 与 ConvertJsonValue 不同，此方法处理的是 ParseJsonObject 输出中已去掉引号的 key。
        /// </summary>
        private static object ConvertStringToType(string strVal, Type targetType)
        {
            if (targetType == typeof(string))
                return strVal;
            if (targetType == typeof(int))
                return int.Parse(strVal);
            if (targetType == typeof(float))
                return float.Parse(strVal);
            if (targetType == typeof(double))
                return double.Parse(strVal);
            if (targetType == typeof(long))
                return long.Parse(strVal);
            if (targetType == typeof(short))
                return short.Parse(strVal);
            if (targetType == typeof(byte))
                return byte.Parse(strVal);
            if (targetType == typeof(bool))
                return strVal == "true";
            if (targetType == typeof(decimal))
                return decimal.Parse(strVal);
            if (targetType == typeof(uint))
                return uint.Parse(strVal);
            if (targetType == typeof(ulong))
                return ulong.Parse(strVal);
            if (targetType == typeof(ushort))
                return ushort.Parse(strVal);
            if (targetType == typeof(sbyte))
                return sbyte.Parse(strVal);
            if (targetType == typeof(char))
                return strVal.Length > 0 ? strVal[0] : '\0';
            if (targetType.IsEnum)
            {
                Type underlyingType = Enum.GetUnderlyingType(targetType);
                if (long.TryParse(strVal, out _))
                {
                    object underlyingValue = Convert.ChangeType(strVal, underlyingType);
                    return underlyingValue;
                }
                return Enum.Parse(targetType, strVal);
            }

            return Convert.ChangeType(strVal, targetType);
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c > 127)
                            sb.Append($"\\u{(int)c:x4}");
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将 JSON 字符串中的转义序列（\uXXXX, \\, \", \n, \r, \t）还原为实际字符。
        /// </summary>
        private static string UnescapeJsonString(string s)
        {
            if (string.IsNullOrEmpty(s) || !s.Contains('\\'))
                return s;

            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    i++;
                    switch (s[i])
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 < s.Length)
                            {
                                string hex = s.Substring(i + 1, 4);
                                if (int.TryParse(hex,
                                    System.Globalization.NumberStyles.HexNumber,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out int code))
                                {
                                    sb.Append((char)code);
                                    i += 4;
                                }
                                else
                                {
                                    sb.Append("\\u");
                                }
                            }
                            else
                            {
                                sb.Append("\\u");
                            }
                            break;
                        default:
                            sb.Append(s[i]);
                            break;
                    }
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
    }
}
