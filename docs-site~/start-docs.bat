@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo  HNUnityFramework 文档服务
echo  本地预览地址: http://localhost:3000
echo ========================================
echo.

:: 优先使用 bun，fallback 到 npm
where bun >nul 2>&1
if %errorlevel% equ 0 (
    echo [INFO] 使用 bun 启动...
    bun run start
) else (
    echo [INFO] bun 未安装，使用 npm 启动...
    npm start
)

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] 启动失败！请确保已执行 bun install^|npm install
    pause
)