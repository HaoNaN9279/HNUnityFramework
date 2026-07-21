using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HN.Framework.Editor.Scripting
{
    /// <summary>
    /// HybridCLR 原生库管理器，负责验证安装状态和拷贝原生库到构建输出。
    /// </summary>
    public static class HybridCLRNativeLibManager
    {
        /// <summary>
        /// 验证 HybridCLR 是否已正确安装（il2cpp_plus 已 patch）。
        /// </summary>
        /// <returns>安装状态正常返回 true；异常返回 false。</returns>
        public static bool ValidateHybridCLRInstallation()
        {
            try
            {
                // 检查 HybridCLR.Installer 是否报告安装状态
                var installerType = Type.GetType(
                    "HybridCLR.Editor.Installer.InstallerController, HybridCLR.Editor");
                if (installerType == null)
                {
                    UnityEngine.Debug.LogWarning(
                        "[HybridCLR] InstallerController type not found. Is the HybridCLR package installed?");
                    return false;
                }

                // 尝试获取 Status 属性
                var statusProp = installerType.GetProperty(
                    "Status",
                    BindingFlags.Static | BindingFlags.Public);
                if (statusProp != null)
                {
                    var status = statusProp.GetValue(null);
                    UnityEngine.Debug.Log(
                        $"[HybridCLR] HybridCLR installation status: {status}");
                    return status != null;
                }

                // 无 Status 属性时，尝试调用 HasInstalledHybridCLR 方法
                var hasInstalledMethod = installerType.GetMethod(
                    "HasInstalledHybridCLR",
                    BindingFlags.Static | BindingFlags.Public);
                if (hasInstalledMethod != null)
                {
                    var result = hasInstalledMethod.Invoke(null, null);
                    var isInstalled = result is bool b && b;
                    UnityEngine.Debug.Log(
                        $"[HybridCLR] HybridCLR installation check: {(isInstalled ? "Installed" : "Not installed")}");
                    return isInstalled;
                }

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[HybridCLR] Failed to validate HybridCLR installation: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 复制 HybridCLR 原生库到构建输出目录。
        /// </summary>
        /// <remarks>
        /// HybridCLR 的原生库（il2cpp + hybridclr 解释器）由 HybridCLR 包的内置构建处理器管理
        /// （如 AddLil2cppSourceCodeToXcodeproj2022OrNewer）。此方法作为自定义原生库管理的扩展点。
        /// </remarks>
        /// <param name="buildOutputPath">构建输出目录路径。</param>
        public static void CopyNativeLibsToBuild(string buildOutputPath)
        {
            if (string.IsNullOrEmpty(buildOutputPath))
            {
                return;
            }

            var hybridCLRData = Path.Combine(
                Directory.GetCurrentDirectory(),
                "HybridCLRData");

            if (!Directory.Exists(hybridCLRData))
            {
                UnityEngine.Debug.LogWarning(
                    $"[HybridCLR] HybridCLRData directory not found at '{hybridCLRData}'. " +
                    "Native lib copy skipped.");
                return;
            }

            // 原生库集成由 HybridCLR 内置构建处理器完成
            // 此方法记录构建意图供后续自定义扩展使用
            UnityEngine.Debug.Log(
                $"[HybridCLR] HybridCLR native libs integration handled by HybridCLR built-in processors. " +
                $"Build output: {buildOutputPath}");
        }
    }
}
