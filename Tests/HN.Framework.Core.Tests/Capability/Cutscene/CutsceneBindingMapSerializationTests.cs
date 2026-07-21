using System.Collections.Generic;
using HN.Framework.Core.Capability.Cutscene;
using NUnit.Framework;

namespace HN.Framework.Core.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutsceneBindingMapSerializationTests
    {
        [Test]
        public void BindingMap_DefaultConstructor_EmptyBindings()
        {
            var map = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            Assert.That(map.Bindings, Is.Not.Null);
            Assert.That(map.Bindings.Count, Is.EqualTo(0));
            Assert.That(map.ResolveMode, Is.EqualTo(BindingResolveMode.ScenePath));
        }

        [Test]
        public void BindingMap_AddBinding_StoresCorrectly()
        {
            var map = new CutsceneBindingMap(BindingResolveMode.Tag);
            map.AddBinding("Hero", "Player");
            map.AddBinding("NPC1", "Merchant");

            Assert.That(map.Bindings.Count, Is.EqualTo(2));
            Assert.That(map.TryGetBinding("Hero", out var target), Is.True);
            Assert.That(target, Is.EqualTo("Player"));
        }

        [Test]
        public void BindingMap_TryGetBinding_NonExistent_ReturnsFalse()
        {
            var map = new CutsceneBindingMap(BindingResolveMode.ScenePath);
            Assert.That(map.TryGetBinding("NonExistent", out _), Is.False);
        }

        [Test]
        public void BindingMap_ConstructorWithBindings_PreservesAll()
        {
            var bindings = new Dictionary<string, string>
            {
                { "Hero", "/Game/Hero" },
                { "Villain", "/Game/Villain" },
                { "Chest", "/Game/Chest" }
            };
            var map = new CutsceneBindingMap(BindingResolveMode.ScenePath, bindings);

            Assert.That(map.Bindings.Count, Is.EqualTo(3));
            Assert.That(map.ResolveMode, Is.EqualTo(BindingResolveMode.ScenePath));
        }

        [Test]
        public void BindingMap_ResolveMode_AllValues()
        {
            Assert.That((int)BindingResolveMode.ScenePath, Is.EqualTo(0));
            Assert.That((int)BindingResolveMode.Tag, Is.EqualTo(1));
            Assert.That((int)BindingResolveMode.EntityId, Is.EqualTo(2));
            Assert.That((int)BindingResolveMode.ActorComponent, Is.EqualTo(3));
        }

        [Test]
        public void CutsceneRole_DefaultValues_AreEmpty()
        {
            CutsceneRole role = default;
            Assert.That(role.RoleName, Is.Null);
            Assert.That(role.DisplayName, Is.Null);
            Assert.That(role.DefaultBindingKey, Is.Null);
        }

        [Test]
        public void CutsceneRole_Constructor_SetsProperties()
        {
            var role = new CutsceneRole("hero", "英雄", "/Game/Hero");
            Assert.That(role.RoleName, Is.EqualTo("hero"));
            Assert.That(role.DisplayName, Is.EqualTo("英雄"));
            Assert.That(role.DefaultBindingKey, Is.EqualTo("/Game/Hero"));
        }

        [Test]
        public void CutsceneRole_Constructor_NullStrings_NotThrow()
        {
            var role = new CutsceneRole(null!, "Display", null!);
            Assert.That(role.RoleName, Is.EqualTo(string.Empty));
            Assert.That(role.DisplayName, Is.EqualTo("Display"));
            Assert.That(role.DefaultBindingKey, Is.EqualTo(string.Empty));
        }

        [Test]
        public void CutsceneAssetRef_DefaultConstructor_EmptyKey()
        {
            CutsceneAssetRef assetRef = default;
            Assert.That(assetRef.AddressablesKey, Is.Null);
        }

        [Test]
        public void CutsceneAssetRef_Constructor_SetsProperties()
        {
            var bindings = new Dictionary<string, string> { { "Hero", "Player" } };
            var assetRef = new CutsceneAssetRef("Assets/Cutscenes/Intro", bindings);

            Assert.That(assetRef.AddressablesKey, Is.EqualTo("Assets/Cutscenes/Intro"));
            Assert.That(assetRef.DefaultBindings.Count, Is.EqualTo(1));
            Assert.That(assetRef.DefaultBindings["Hero"], Is.EqualTo("Player"));
        }

        [Test]
        public void CutsceneAssetRef_NullBindings_BecomesEmpty()
        {
            var assetRef = new CutsceneAssetRef("key", null);
            Assert.That(assetRef.DefaultBindings, Is.Not.Null);
            Assert.That(assetRef.DefaultBindings.Count, Is.EqualTo(0));
        }
    }
}
