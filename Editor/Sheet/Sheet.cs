using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;

namespace HN.Framework
{
    public class Sheet : ScriptableObject
    {
        /// <summary>从CSV文件路径初始化Sheet数据</summary>
        /// <param name="path">CSV文件路径</param>
        public void Initialize(string path)
        {
            this.assetPath = Path.GetFullPath(path);
            List<string[]> data = ReadCSV(assetPath);
            if (data.Count == 0)
                return;

            rowCount = 0;
            for (int dataLineId = 0; dataLineId < data.Count; dataLineId++)
            {
                if (dataLineId == 0)
                {
                    ReadTypes(data[dataLineId]);
                }
                else if (dataLineId == 1)
                {
                    ReadHeaders(data[dataLineId]);
                }
                else if(dataLineId == 2)
                {
                    ReadSummarys(data[dataLineId]);
                }
                else
                {
                    ReadSheetLine(data[dataLineId]);
                }

            }
        }

        /// <summary>在指定列索引处添加新列</summary>
        /// <param name="columnId">列索引</param>
        public void AddColumn(int columnId)
        {
            types.Insert(columnId, defaultTypeName);
            headers.Insert(columnId, defaultHeaderName);
            summaries.Insert(columnId, defaultSummary);
            for (int i = rowCount - 1; i >= 0; i--)
            {
                int index = i * columnCount + columnId;
                elements.Insert(index, defaultElement);
            }
            columnCount++;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>删除指定列索引处的列</summary>
        /// <param name="columnId">列索引</param>
        public void DeleteColumn(int columnId)
        {
            types.RemoveAt(columnId);
            headers.RemoveAt(columnId);
            summaries.RemoveAt(columnId);
            for (int i = rowCount - 1; i >= 0; i--)
            {
                int index = i * columnCount + columnId;
                elements.RemoveAt(index);
            }
            columnCount--;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>在指定行索引处添加新行</summary>
        /// <param name="rowId">行索引</param>
        public void AddRow(int rowId)
        {
            int index = rowId * columnCount;
            for (int i = 0; i < columnCount; i++)
            {
                elements.Insert(index, defaultElement);
            }
            rowCount++;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        /// <summary>删除指定行索引处的行</summary>
        /// <param name="rowId">行索引</param>
        public void DeleteRow(int rowId)
        {
            int index = rowId * columnCount;
            for (int i = 0; i < columnCount; i++)
            {
                elements.RemoveAt(index);
            }
            rowCount--;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>将Sheet数据保存回CSV文件</summary>
        public void SaveAsset()
        {
            //将SO数据写入到csv文件中
            string content = "";
            for (int i = 0; i < columnCount; i++)
            {
                content += types[i];
                if (i != columnCount - 1)
                    content += ",";
                else
                    content += "\n";
            }
            for (int i = 0; i < columnCount; i++)
            {
                content += headers[i];
                if (i != columnCount - 1)
                    content += ",";
                else
                    content += "\n";
            }
            for(int i = 0; i < columnCount; i++)
            {
                content += summaries[i];
                if (i != columnCount - 1)
                    content += ",";
                else
                    content += "\n";
            }
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    content += elements[i * columnCount + j];
                    if (j != columnCount - 1)
                        content += ",";
                    else
                        content += "\n";
                }
            }

            if (!File.Exists(assetPath))
            {
                string dirPath = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
            File.WriteAllText(assetPath, content, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
        
        /// <summary>导出Sheet数据</summary>
        public void Export()
        {
            
        }


        private List<string[]> ReadCSV(string filePath)
        {
            List<string[]> data = new List<string[]>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    data.Add(values);
                }
            }

            return data;
        }

        private void ReadTypes(string[] typeNames)
        {
            for (int i = 0; i < typeNames.Length; i++)
            {
                types.Add(typeNames[i]);
            }
            columnCount = typeNames.Length;
        }

        private void ReadHeaders(string[] headers)
        {
            if (columnCount == 0)
                return;

            for (int i = 0; i < columnCount; i++)
            {
                if (headers.Length > i)
                {
                    this.headers.Add(headers[i]);
                }
            }
        }
        
        private void ReadSummarys(string[] summaries)
        {
            if (columnCount == 0)
                return;

            for(int i = 0; i < columnCount; i++)
            {
                this.summaries.Add(summaries[i]);
            }
        }
        
        private void ReadSheetLine(string[] sheetLine)
        {
            if (columnCount == 0)
                return;

            for (int i = 0; i < sheetLine.Length; i++)
            {
                if (sheetLine.Length > i)
                {
                    elements.Add(sheetLine[i]);
                }
            }

            rowCount++;
        }

        // private Type ReflectionFindType(string typeName)
        // {
        //     var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //     foreach (var assembly in assemblies)
        //     {
        //         Type[] types;
        //         try
        //         {
        //             types = assembly.GetTypes();
        //         }
        //         catch (ReflectionTypeLoadException e)
        //         {
        //             types = e.Types.Where(t => t != null).ToArray();
        //         }
        //         catch
        //         {
        //             continue;
        //         }

        //         foreach (var type in types)
        //         {
        //             var attrs = type.GetCustomAttributes(typeof(SheetElementTypeAttribute), false);
        //             if(attrs.Length > 0)
        //             {
        //                 foreach(SheetElementTypeAttribute attr in attrs)
        //                 {
        //                     if(attr.typeName == typeName)
        //                     {
        //                         return type;
        //                     }
        //                 }
        //             }
        //         }
        //     }

        //     return null;
        // }


        [SerializeField]
        private string assetPath;
        public string AssetPath => assetPath;

        [SerializeField]
        private string sheetName;
        public string SheetName => sheetName;

        [SerializeField]
        private string sheetNameSummary;
        public string SheetNameSummary => sheetNameSummary;

        [SerializeField]
        private List<string> types = new List<string>();
        public List<string> Types => types;

        [SerializeField]
        private List<string> headers = new List<string>();
        public List<string> Headers => headers;

        [SerializeField]
        private List<string> summaries = new List<string>();
        public List<string> Summaries => summaries;

        [SerializeField]
        private List<string> elements = new List<string>();
        public List<string> Elements => elements;

        [SerializeField]
        private int columnCount;
        public int ColumnCount => columnCount;

        [SerializeField]
        private int rowCount;
        public int RowCount => rowCount;

        /// <summary>默认类型名称</summary>
        public const string defaultTypeName = "STRING";
        /// <summary>默认表头名称</summary>
        public const string defaultHeaderName = "[Undefined]";
        /// <summary>默认摘要</summary>
        public const string defaultSummary = "";
        /// <summary>默认元素值</summary>
        public const string defaultElement = "";
    }


}
