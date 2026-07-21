#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using CoreDebugHub = HN.Framework.Core.Capability.Debug.DebugHub;
using HN.Framework.Core.Driver;
using HN.Framework.Core.Driver.Common.Debug;
using HN.Framework.Unity.Driver.Platform;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HN.Framework.Unity.Capability.Debug
{
    /// <summary>
    /// UGUI 运行时调试终端。`~` 键切换显示，支持命令输入、自动补全和历史。
    /// </summary>
    /// <remarks>
    /// 通过 <see cref="GameWorldDriver"/> 自动发现 <see cref="GameWorld.DebugHub"/>，
    /// 无需手动配置。控制台面板占据屏幕下半部分，上半部分为输出区域，底部为输入区域。
    /// </remarks>
    public class RuntimeDebugConsole : MonoBehaviour
    {
        [SerializeField] private int _maxHistory = 50;
        [SerializeField] private int _maxOutputLines = 200;
        [SerializeField] private Color _errorColor = Color.red;
        [SerializeField] private Color _outputColor = Color.white;

        private CoreDebugHub? _debugHub;
        private GameObject? _canvasObject;
        private TMP_InputField? _inputField;
        private TextMeshProUGUI? _outputText;
        private ScrollRect? _scrollRect;
        private readonly List<string> _commandHistory = new();
        private int _historyIndex = -1;
        private string _pendingCommand = "";
        private bool _isVisible;
        private readonly List<string> _tabCandidates = new();
        private int _tabIndex = -1;

        /// <summary>
        /// 获取或设置调试中枢。可在 Start() 之前手动注入以覆盖自动发现。
        /// </summary>
        public CoreDebugHub? DebugHub
        {
            get => _debugHub;
            set => _debugHub = value;
        }

        /// <summary>
        /// 获取控制台当前是否可见。
        /// </summary>
        public bool IsVisible => _isVisible;

        void Start()
        {
            // 自动发现 GameWorldDriver（Momus F3 fix - 不修改 GameWorldDriver.cs）
            if (_debugHub == null)
            {
                var driver = FindObjectOfType<GameWorldDriver>();
                if (driver != null && driver.World != null)
                {
                    _debugHub = driver.World.DebugHub;
                }
            }

            CreateUI();
            SetVisible(false);
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote))
            {
                Toggle();
            }

            if (!_isVisible || _inputField == null || !_inputField.isFocused)
                return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ExecuteInput();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                HandleTabComplete();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(-1);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(1);
            }
        }

        void OnDestroy()
        {
            if (_canvasObject != null)
                Destroy(_canvasObject);
        }

        private void CreateUI()
        {
            // === Canvas (Screen Space - Overlay, sort order 9999 确保最上层) ===
            _canvasObject = new GameObject("RuntimeDebugCanvas");
            _canvasObject.transform.SetParent(transform, false);

            var canvas = _canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var canvasScaler = _canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            _canvasObject.AddComponent<GraphicRaycaster>();

            // === 半透明黑色面板（屏幕下半部分 50%） ===
            var panelObj = new GameObject("ConsolePanel");
            panelObj.transform.SetParent(_canvasObject.transform, false);

            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);

            // === 输出区域 ScrollRect（面板上半部 80%，即顶部 80%） ===
            var scrollObj = new GameObject("OutputScroll");
            scrollObj.transform.SetParent(panelObj.transform, false);

            var scrollRectComp = scrollObj.AddComponent<ScrollRect>();
            _scrollRect = scrollRectComp;
            scrollRectComp.vertical = true;
            scrollRectComp.horizontal = false;
            scrollRectComp.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            var scrollRectTrans = scrollObj.GetComponent<RectTransform>();
            scrollRectTrans.anchorMin = new Vector2(0, 0.2f);
            scrollRectTrans.anchorMax = new Vector2(1, 1);
            scrollRectTrans.offsetMin = new Vector2(10, 0);
            scrollRectTrans.offsetMax = new Vector2(-10, 0);

            // Mask for ScrollRect
            var mask = scrollObj.AddComponent<Mask>();
            scrollObj.AddComponent<Image>().color = new Color(0, 0, 0, 0);

            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);

            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);

            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);

            // Content (holds output text)
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _outputText = content.AddComponent<TextMeshProUGUI>();
            _outputText.fontSize = 16;
            _outputText.color = _outputColor;
            _outputText.alignment = TextAlignmentOptions.BottomLeft;
            _outputText.enableWordWrapping = true;

            scrollRectComp.viewport = viewportRect;
            scrollRectComp.content = contentRect;

            // === 输入区域（面板底部 20%） ===
            var inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(panelObj.transform, false);

            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 0);
            inputRect.anchorMax = new Vector2(1, 0.2f);
            inputRect.offsetMin = new Vector2(10, 0);
            inputRect.offsetMax = new Vector2(-10, 0);

            var inputImage = inputObj.AddComponent<Image>();
            inputImage.color = new Color(0.15f, 0.15f, 0.15f, 1);

            _inputField = inputObj.AddComponent<TMP_InputField>();
            _inputField.text = "";

            // Input field text area
            var inputTextArea = new GameObject("TextArea");
            inputTextArea.transform.SetParent(inputObj.transform, false);

            var textAreaRect = inputTextArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);

            var inputText = inputTextArea.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 18;
            inputText.color = Color.white;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;

            _inputField.textComponent = inputText;
            _inputField.textViewport = textAreaRect;

            // Placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);

            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 5);
            placeholderRect.offsetMax = new Vector2(-10, -5);

            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "输入命令...";
            placeholderText.fontSize = 18;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholderText.fontStyle = FontStyles.Italic;
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

            _inputField.placeholder = placeholderText;
        }

        private void Toggle()
        {
            _isVisible = !_isVisible;
            SetVisible(_isVisible);
        }

        private void SetVisible(bool visible)
        {
            if (_canvasObject != null)
                _canvasObject.SetActive(visible);

            if (visible && _inputField != null)
            {
                _inputField.Select();
                _inputField.ActivateInputField();
                // 重置 Tab 补全状态
                _tabCandidates.Clear();
                _tabIndex = -1;
            }
        }

        private void ExecuteInput()
        {
            if (_inputField == null || _debugHub == null)
                return;

            var input = _inputField.text.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            // 加入历史记录
            _commandHistory.Add(input);
            if (_commandHistory.Count > _maxHistory)
                _commandHistory.RemoveAt(0);
            _historyIndex = _commandHistory.Count;

            // 回显输入
            AppendText($"> {input}\n", _outputColor);

            // 解析命令：第一个 token 是命令名，其余是参数
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmdName = parts[0];
            var args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

            // 执行命令
            if (!_debugHub.ExecuteCommand(cmdName, args))
            {
                AppendText($"[Error] 未知命令: {cmdName}\n", _errorColor);
            }

            _inputField.text = "";
            _inputField.Select();
            _inputField.ActivateInputField();

            // 重置 Tab 补全状态
            _tabCandidates.Clear();
            _tabIndex = -1;

            // 自动滚动到底部
            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = 0;
        }

        private void AppendText(string text, Color color)
        {
            if (_outputText == null)
                return;

            _outputText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

            // 按行数截断
            var lines = _outputText.text.Split('\n');
            if (lines.Length > _maxOutputLines)
            {
                _outputText.text = string.Join("\n", lines, lines.Length - _maxOutputLines, _maxOutputLines);
            }
        }

        private void HandleTabComplete()
        {
            if (_inputField == null || _debugHub == null)
                return;

            var input = _inputField.text.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            // 首次按 Tab，获取候选列表
            if (_tabCandidates.Count == 0)
            {
                _tabCandidates.Clear();
                _tabCandidates.AddRange(_debugHub.SearchCommands(input)
                    .Select(c => c.Name)
                    .Where(n => n.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    .ToList());
                _tabIndex = 0;
            }

            if (_tabCandidates.Count == 0)
                return;

            // 循环候选列表
            _inputField.text = _tabCandidates[_tabIndex];
            _inputField.caretPosition = _inputField.text.Length;

            _tabIndex = (_tabIndex + 1) % _tabCandidates.Count;
        }

        private void NavigateHistory(int direction)
        {
            if (_commandHistory.Count == 0 || _inputField == null)
                return;

            // 离开当前位置时保存待定命令
            if (_historyIndex >= 0 && _historyIndex < _commandHistory.Count)
            {
                _pendingCommand = _inputField.text;
            }

            _historyIndex += direction;

            if (_historyIndex < 0)
            {
                _historyIndex = 0;
                return;
            }

            if (_historyIndex >= _commandHistory.Count)
            {
                _historyIndex = _commandHistory.Count;
                _inputField.text = _pendingCommand;
                return;
            }

            _inputField.text = _commandHistory[_historyIndex];
            _inputField.caretPosition = _inputField.text.Length;
        }
    }
}
