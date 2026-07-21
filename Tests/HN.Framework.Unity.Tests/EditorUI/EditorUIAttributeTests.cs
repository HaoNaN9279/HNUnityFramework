using HN.Framework.Unity.EditorUI.Attributes;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.EditorUI
{
    [TestFixture]
    public class EditorUIAttributeTests
    {
        // ─── ReadOnly ──────────────────────────────────────────────

        [Test]
        public void ReadOnlyAttribute_CanCreate()
        {
            var attr = new ReadOnlyAttribute();
            Assert.IsNotNull(attr);
            Assert.IsInstanceOf<UnityEngine.PropertyAttribute>(attr);
        }

        // ─── Button ────────────────────────────────────────────────

        [Test]
        public void ButtonAttribute_DefaultValues()
        {
            var attr = new ButtonAttribute();
            Assert.IsNull(attr.ButtonName);
            Assert.AreEqual(22f, attr.ButtonHeight);
            Assert.IsTrue(attr.DirtyOnClick);
        }

        [Test]
        public void ButtonAttribute_CustomName()
        {
            var attr = new ButtonAttribute("My Button");
            Assert.AreEqual("My Button", attr.ButtonName);
        }

        [Test]
        public void ButtonAttribute_CustomHeight()
        {
            var attr = new ButtonAttribute { ButtonHeight = 30f };
            Assert.AreEqual(30f, attr.ButtonHeight);
        }

        // ─── ShowIf ────────────────────────────────────────────────

        [Test]
        public void ShowIfAttribute_HasCondition()
        {
            var attr = new ShowIfAttribute("isVisible");
            Assert.AreEqual("isVisible", attr.Condition);
        }

        [Test]
        public void ShowIfAttribute_EmptyCondition()
        {
            var attr = new ShowIfAttribute("");
            Assert.AreEqual("", attr.Condition);
        }

        // ─── HideIf ────────────────────────────────────────────────

        [Test]
        public void HideIfAttribute_HasCondition()
        {
            var attr = new HideIfAttribute("isHidden");
            Assert.AreEqual("isHidden", attr.Condition);
        }

        // ─── EnableIf ──────────────────────────────────────────────

        [Test]
        public void EnableIfAttribute_HasCondition()
        {
            var attr = new EnableIfAttribute("canEdit");
            Assert.AreEqual("canEdit", attr.Condition);
        }

        // ─── DisableIf ─────────────────────────────────────────────

        [Test]
        public void DisableIfAttribute_HasCondition()
        {
            var attr = new DisableIfAttribute("isDisabled");
            Assert.AreEqual("isDisabled", attr.Condition);
        }

        // ─── ShowInInspector ───────────────────────────────────────

        [Test]
        public void ShowInInspectorAttribute_CanCreate()
        {
            var attr = new ShowInInspectorAttribute();
            Assert.IsNotNull(attr);
        }

        // ─── HideInInspector ───────────────────────────────────────

        [Test]
        public void HideInInspectorAttribute_CanCreate()
        {
            var attr = new HideInInspectorAttribute();
            Assert.IsNotNull(attr);
        }

        // ─── Required ──────────────────────────────────────────────

        [Test]
        public void RequiredAttribute_DefaultErrorMessage()
        {
            var attr = new RequiredAttribute();
            Assert.IsNull(attr.ErrorMessage);
        }

        [Test]
        public void RequiredAttribute_CustomErrorMessage()
        {
            var attr = new RequiredAttribute("Field is required!");
            Assert.AreEqual("Field is required!", attr.ErrorMessage);
        }

        [Test]
        public void RequiredAttribute_EmptyErrorMessage()
        {
            var attr = new RequiredAttribute("");
            Assert.AreEqual("", attr.ErrorMessage);
        }

        // ─── ValidateInput ─────────────────────────────────────────

        [Test]
        public void ValidateInputAttribute_HasMethodName()
        {
            var attr = new ValidateInputAttribute("ValidateMethod");
            Assert.AreEqual("ValidateMethod", attr.MethodName);
        }

        [Test]
        public void ValidateInputAttribute_DefaultMessageIsNull()
        {
            var attr = new ValidateInputAttribute("ValidateMethod");
            Assert.IsNull(attr.Message);
        }

        [Test]
        public void ValidateInputAttribute_CustomMessage()
        {
            var attr = new ValidateInputAttribute("ValidateMethod") { Message = "Invalid value" };
            Assert.AreEqual("Invalid value", attr.Message);
        }

        // ─── OnValueChanged ────────────────────────────────────────

        [Test]
        public void OnValueChangedAttribute_HasMethodName()
        {
            var attr = new OnValueChangedAttribute("OnChanged");
            Assert.AreEqual("OnChanged", attr.MethodName);
        }

        // ─── Title ─────────────────────────────────────────────────

        [Test]
        public void TitleAttribute_HasTitle()
        {
            var attr = new TitleAttribute("Section Title");
            Assert.AreEqual("Section Title", attr.Title);
        }

        [Test]
        public void TitleAttribute_Defaults()
        {
            var attr = new TitleAttribute("Title");
            Assert.IsTrue(attr.Bold);
            Assert.IsTrue(attr.HorizontalLine);
            Assert.IsNull(attr.Subtitle);
        }

        [Test]
        public void TitleAttribute_CustomSubtitle()
        {
            var attr = new TitleAttribute("Title") { Subtitle = "Sub" };
            Assert.AreEqual("Sub", attr.Subtitle);
        }

        [Test]
        public void TitleAttribute_DisableBold()
        {
            var attr = new TitleAttribute("Title") { Bold = false };
            Assert.IsFalse(attr.Bold);
        }

        // ─── InfoBox ───────────────────────────────────────────────

        [Test]
        public void InfoBoxAttribute_HasMessage()
        {
            var attr = new InfoBoxAttribute("Info message");
            Assert.AreEqual("Info message", attr.Message);
        }

        [Test]
        public void InfoBoxAttribute_DefaultTypeIsInfo()
        {
            var attr = new InfoBoxAttribute("Info");
            Assert.AreEqual(InfoMessageType.Info, attr.Type);
        }

        [Test]
        public void InfoBoxAttribute_CustomType()
        {
            var attr = new InfoBoxAttribute("Warning") { Type = InfoMessageType.Warning };
            Assert.AreEqual(InfoMessageType.Warning, attr.Type);
        }

        [Test]
        public void InfoBoxAttribute_ErrorType()
        {
            var attr = new InfoBoxAttribute("Error") { Type = InfoMessageType.Error };
            Assert.AreEqual(InfoMessageType.Error, attr.Type);
        }

        [Test]
        public void InfoBoxAttribute_WithVisibleIf()
        {
            var attr = new InfoBoxAttribute("Conditional") { VisibleIf = "showInfo" };
            Assert.AreEqual("showInfo", attr.VisibleIf);
        }

        // ─── PropertyOrder ─────────────────────────────────────────

        [Test]
        public void PropertyOrderAttribute_HasOrder()
        {
            var attr = new PropertyOrderAttribute(5);
            Assert.AreEqual(5, attr.Order);
        }

        [Test]
        public void PropertyOrderAttribute_NegativeOrder()
        {
            var attr = new PropertyOrderAttribute(-10);
            Assert.AreEqual(-10, attr.Order);
        }

        [Test]
        public void PropertyOrderAttribute_ZeroOrder()
        {
            var attr = new PropertyOrderAttribute(0);
            Assert.AreEqual(0, attr.Order);
        }

        // ─── FoldoutGroup ──────────────────────────────────────────

        [Test]
        public void FoldoutGroupAttribute_HasPath()
        {
            var attr = new FoldoutGroupAttribute("Stats/Base");
            Assert.AreEqual("Stats/Base", attr.Path);
        }

        [Test]
        public void FoldoutGroupAttribute_SingleLevelPath()
        {
            var attr = new FoldoutGroupAttribute("Settings");
            Assert.AreEqual("Settings", attr.Path);
        }

        // ─── BoxGroup ──────────────────────────────────────────────

        [Test]
        public void BoxGroupAttribute_HasPath()
        {
            var attr = new BoxGroupAttribute("Stats");
            Assert.AreEqual("Stats", attr.Path);
        }

        // ─── TabGroup ──────────────────────────────────────────────

        [Test]
        public void TabGroupAttribute_HasPath()
        {
            var attr = new TabGroupAttribute("Stats/Base");
            Assert.AreEqual("Stats/Base", attr.Path);
        }

        // ─── HorizontalGroup ───────────────────────────────────────

        [Test]
        public void HorizontalGroupAttribute_HasPath()
        {
            var attr = new HorizontalGroupAttribute("Row1");
            Assert.AreEqual("Row1", attr.Path);
        }

        // ─── VerticalGroup ─────────────────────────────────────────

        [Test]
        public void VerticalGroupAttribute_HasPath()
        {
            var attr = new VerticalGroupAttribute("Column1");
            Assert.AreEqual("Column1", attr.Path);
        }

        // ─── InfoMessageType ───────────────────────────────────────

        [Test]
        public void InfoMessageType_HasAllValues()
        {
            Assert.AreEqual(0, (int)InfoMessageType.None);
            Assert.AreEqual(1, (int)InfoMessageType.Info);
            Assert.AreEqual(2, (int)InfoMessageType.Warning);
            Assert.AreEqual(3, (int)InfoMessageType.Error);
        }
    }
}
