#nullable enable

using System;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Capability.Input
{
    /// <summary>
    /// 输入设备检测器，负责检测当前连接的输入设备并报告控制方案。
    /// 通过 InputSystem 事件驱动，无需轮询。
    /// </summary>
    public sealed class DeviceDetector : IDisposable
    {
        private string _currentControlScheme = "Unknown";

        /// <summary>
        /// 当前控制方案。可能的值："KeyboardMouse"、"Gamepad"、"Touch"、"Unknown"。
        /// </summary>
        public string CurrentControlScheme => _currentControlScheme;

        /// <summary>
        /// 当控制方案发生变化时触发。
        /// 参数为新的控制方案名称。
        /// </summary>
        public event Action<string>? OnControlSchemeChanged;

        /// <summary>
        /// 初始化设备检测器，立即执行设备检测并订阅 InputSystem 设备变更事件。
        /// </summary>
        public DeviceDetector()
        {
            Detect();
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        /// <summary>
        /// 检测当前连接的输入设备，按优先级确定控制方案。
        /// 优先级：Touchscreen > Gamepad > Keyboard/Mouse > Unknown。
        /// 若方案发生变化且 <see cref="OnControlSchemeChanged"/> 不为 null，则触发事件。
        /// </summary>
        /// <returns>检测到的控制方案名称。</returns>
        public string Detect()
        {
            var devices = InputSystem.devices;
            string newScheme = "Unknown";

            foreach (var device in devices)
            {
                if (device is Touchscreen)
                {
                    newScheme = "Touch";
                    break;
                }

                if (device is Gamepad && newScheme != "Touch")
                {
                    newScheme = "Gamepad";
                }

                if ((device is Keyboard || device is Mouse) && newScheme == "Unknown")
                {
                    newScheme = "KeyboardMouse";
                }
            }

            if (newScheme != _currentControlScheme)
            {
                string previous = _currentControlScheme;
                _currentControlScheme = newScheme;
                OnControlSchemeChanged?.Invoke(newScheme);
            }

            return _currentControlScheme;
        }

        /// <summary>
        /// 释放资源，取消 InputSystem 设备变更事件的订阅。
        /// </summary>
        public void Dispose()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            // 设备添加或移除后重新检测控制方案
            if (change == InputDeviceChange.Added ||
                change == InputDeviceChange.Removed)
            {
                Detect();
            }
        }
    }
}
