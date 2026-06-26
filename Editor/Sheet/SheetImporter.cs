using UnityEngine;
using UnityEditor.AssetImporters;

namespace HN.Framework.Editor
{
    /// <summary>
    /// .sheet 文件的 ScriptedImporter，导入时创建 Sheet 实例。
    /// </summary>
    [ScriptedImporter(1, "sheet")]
    public class SheetImporter : ScriptedImporter
    {
        /// <summary>
        /// 导入资源时调用，创建 Sheet 并将其添加到导入上下文中。
        /// </summary>
        /// <param name="ctx">资源导入上下文。</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var sheets = ScriptableObject.CreateInstance<Sheet>();
            sheets.Initialize(ctx.assetPath);
            ctx.AddObjectToAsset("main obj", sheets);
            ctx.SetMainObject(sheets);
        }
        

    }
}
