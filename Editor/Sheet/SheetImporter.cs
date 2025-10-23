using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor.AssetImporters;

namespace HN.Framework.Editor
{
    [ScriptedImporter(1, "sheet")]
    public class SheetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var sheets = ScriptableObject.CreateInstance<Sheet>();
            sheets.Initialize(ctx.assetPath);
            ctx.AddObjectToAsset("main obj", sheets);
            ctx.SetMainObject(sheets);
        }
        

    }
}
