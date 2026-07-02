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

        /// <summary>
        /// 将字符串内容写入指定路径的文件。如果文件存在则覆盖，不存在则创建。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="text">要写入的文本内容。</param>
        /// <returns>写入成功返回 true，路径为空返回 false。</returns>
        public static bool WriteToDisk(string path, string text)
        {
            if(string.IsNullOrEmpty(path))
                return false;

            FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            file.Seek(0, SeekOrigin.Begin);
            file.SetLength(0);
            file.Close();
            StreamWriter sw;
            sw = File.CreateText(path);
            sw.Write(text);
            sw.Close();
            sw.Dispose();
            return true;
        }

        /// <summary>
        /// 从指定路径的文件中读取全部文本内容。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>文件的文本内容。</returns>
        public static string ReadFromDisk(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
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
            if(string.IsNullOrEmpty(path) || obj == null)
            {
                return false;
            }
            string jsonString = ReadFromDisk(path);
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
            if(string.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            var type = Type.GetType(typeName);
            var obj = Activator.CreateInstance(type);
            if(obj == null)
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
            var obj = Activator.CreateInstance<T>();
            if(Deserialize(obj, path))
            {
                return obj;
            }
            return default;
        }

        /// <summary>
        /// 从 JSON 字符串反序列化为指定类型的新对象。
        /// </summary>
        /// <param name="jsonString">JSON 字符串。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化后的对象。</returns>
        public static T DeserializeFromString<T>(string jsonString)
        {
            var obj = Activator.CreateInstance<T>();
            DeserializeFromString(obj, jsonString);

            return obj;
        }

        // ==================== Private Implementation ====================

        private static void JsonOverwrite(object target, string jsonString)
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
                    SetFieldValue(target, field, strVal);
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

            // Enum
            if (type.IsEnum)
            {
                sb.Append('"');
                sb.Append(EscapeJson(obj.ToString()));
                sb.Append('"');
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
                string key = json.Substring(keyStart, i - keyStart);
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
                    if (c == '\\') i++;
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

        private static void SetFieldValue(object target, FieldInfo field, string strVal)
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
                object value = ConvertJsonValue(strVal, fieldType);
                field.SetValue(target, value);
            }
            catch
            {
                // Silently ignore conversion errors for compatibility
            }
        }

        private static object ConvertJsonValue(string strVal, Type targetType)
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
                return strVal.Trim('"');
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
                var trimmed = strVal.Trim('"');
                return trimmed.Length > 0 ? trimmed[0] : '\0';
            }
            if (targetType.IsEnum)
                return Enum.Parse(targetType, strVal.Trim('"'));
            if (targetType.IsArray || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>)))
                return DeserializeArray(strVal, targetType);
            if (targetType.IsPrimitive)
                return Convert.ChangeType(strVal, targetType);

            // Complex object
            return DeserializeObject(strVal, targetType);
        }

        private static object DeserializeObject(string json, Type type)
        {
            if (string.IsNullOrEmpty(json) || json == "null")
                return null;

            json = json.Trim();
            if (!json.StartsWith("{"))
                return null;

            object result = Activator.CreateInstance(type);
            JsonOverwrite(result, json);
            return result;
        }

        private static object DeserializeArray(string json, Type arrayType)
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
                    array.SetValue(ConvertJsonValue(elements[i], elementType), i);
                }
                return array;
            }
            else
            {
                var list = (IList)Activator.CreateInstance(arrayType);
                for (int i = 0; i < elements.Count; i++)
                {
                    list.Add(ConvertJsonValue(elements[i], elementType));
                }
                return list;
            }
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
                    default:   sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
    }
}
