using System;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建清单，记录每次构建的元数据。
    /// </summary>
    [Serializable]
    public class BuildManifest
    {
        /// <summary>构建时间</summary>
        public string BuildTime;

        /// <summary>版本号</summary>
        public string Version;

        /// <summary>Git 分支</summary>
        public string Branch;

        /// <summary>Git Commit Hash</summary>
        public string CommitHash;

        /// <summary>构建平台</summary>
        public string Platform;

        /// <summary>包体大小（字节）</summary>
        public long BuildSize;

        /// <summary>构建输出路径</summary>
        public string OutputPath;

        /// <summary>构建编号</summary>
        public int BuildNumber;
    }
}
