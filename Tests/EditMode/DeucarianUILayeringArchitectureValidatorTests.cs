using System;
using System.IO;
using System.Linq;
using Deucarian.UI.Editor;
using NUnit.Framework;

namespace Deucarian.UI.Tests
{
    public sealed class DeucarianUILayeringArchitectureValidatorTests
    {
        private string runtimeRoot;

        [SetUp]
        public void SetUp()
        {
            runtimeRoot = Path.Combine(
                Path.GetTempPath(),
                "DeucarianUILayeringArchitectureValidatorTests",
                Guid.NewGuid().ToString("N"),
                "Runtime");
            Directory.CreateDirectory(runtimeRoot);
        }

        [TearDown]
        public void TearDown()
        {
            string fixtureRoot = Directory.GetParent(runtimeRoot)?.FullName;
            if (!string.IsNullOrEmpty(fixtureRoot) &&
                Directory.Exists(fixtureRoot))
            {
                Directory.Delete(fixtureRoot, true);
            }
        }

        [Test]
        public void CleanRuntimeIgnoresCommentsAndStringLiterals()
        {
            WriteSource(
                "CleanSurface.cs",
                "// PanelSettings document.sortingOrder = 9;\n" +
                "public sealed class CleanSurface\n" +
                "{\n" +
                "    private const string Example = " +
                "\"canvas.overrideSorting = true;\";\n" +
                "    private const string Verbatim = " +
                "@\"PanelSettings\";\n" +
                "    private const string Raw = \"\"\"" +
                "document.sortingOrder = 4;\"\"\";\n" +
                "    /* document.panelSettings = settings; */\n" +
                "}\n");

            var violations =
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(runtimeRoot);

            Assert.That(violations, Is.Empty);
        }

        [Test]
        public void SourceViolationsReportRulePathAndExactLine()
        {
            WriteSource(
                "UI/OwnedSurface.cs",
                "public sealed class OwnedSurface\n" +
                "{\n" +
                "    private PanelSettings settings;\n" +
                "    void Load() => DeucarianUIRuntimeAssets" +
                ".LoadRuntimePanelSettings();\n" +
                "    void Bind() => document.panelSettings = settings;\n" +
                "    void Sort() => document.sortingOrder = 500;\n" +
                "    void Override() => canvas.overrideSorting = true;\n" +
                "}\n");

            var violations =
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(runtimeRoot);

            Assert.That(violations.Count, Is.EqualTo(5));
            AssertViolation(
                violations,
                DeucarianUILayeringArchitectureRule.PanelSettingsReference,
                3);
            AssertViolation(
                violations,
                DeucarianUILayeringArchitectureRule
                    .DirectPanelSettingsLoad,
                4);
            AssertViolation(
                violations,
                DeucarianUILayeringArchitectureRule
                    .DirectPanelSettingsAssignment,
                5);
            AssertViolation(
                violations,
                DeucarianUILayeringArchitectureRule
                    .DirectSortingOrderAssignment,
                6);
            AssertViolation(
                violations,
                DeucarianUILayeringArchitectureRule
                    .DirectOverrideSortingAssignment,
                7);
            Assert.That(
                violations.All(violation =>
                    violation.RelativePath == "UI/OwnedSurface.cs" &&
                    violation.Column > 0 &&
                    !string.IsNullOrWhiteSpace(violation.Message)),
                Is.True);
            StringAssert.Contains(
                "UI/OwnedSurface.cs:3:",
                violations[0].ToString());
        }

        [Test]
        public void PanelSettingsAssetSignatureIsDetectedByContent()
        {
            WriteAsset(
                "Resources/Neutral.asset",
                "%YAML 1.1\n" +
                "MonoBehaviour:\n" +
                "  m_Script: {fileID: 19101, guid: " +
                "0000000000000000e000000000000000, type: 0}\n" +
                "  m_Name: Neutral\n");

            var violations =
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(runtimeRoot);

            Assert.That(violations.Count, Is.EqualTo(1));
            Assert.That(
                violations[0].Rule,
                Is.EqualTo(
                    DeucarianUILayeringArchitectureRule.PanelSettingsAsset));
            Assert.That(violations[0].Line, Is.EqualTo(3));
            Assert.That(
                violations[0].RelativePath,
                Is.EqualTo("Resources/Neutral.asset"));
        }

        [Test]
        public void PanelSettingsAssetNameIsDetectedWithoutYamlSignature()
        {
            WriteAsset(
                "Resources/LocalPanelSettings.asset",
                "Local package-owned panel settings placeholder");

            var violations =
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(runtimeRoot);

            Assert.That(violations.Count, Is.EqualTo(1));
            Assert.That(
                violations[0].Rule,
                Is.EqualTo(
                    DeucarianUILayeringArchitectureRule.PanelSettingsAsset));
        }

        [Test]
        public void OtherBuiltinAssetsAreNotPanelSettings()
        {
            WriteAsset(
                "Resources/OtherBuiltin.asset",
                "%YAML 1.1\n" +
                "MonoBehaviour:\n" +
                "  m_Script: {fileID: 19102, guid: " +
                "0000000000000000e000000000000000, type: 0}\n");

            var violations =
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(runtimeRoot);

            Assert.That(violations, Is.Empty);
        }

        [Test]
        public void SortingAllowlistIsExactAndSuppressesNoOtherRule()
        {
            const string allowedPath =
                "Reports/Attachments/ViewerMediaOverlayVisualStyle.cs";
            WriteSource(
                allowedPath,
                "renderer.sortingOrder = sortingOrder;\n" +
                "document.panelSettings = settings;\n");
            WriteSource(
                "Reports/Attachments/OtherVisualStyle.cs",
                "renderer.sortingOrder = sortingOrder;\n");

            var violations =
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(
                        runtimeRoot,
                        new[] { allowedPath });

            Assert.That(violations.Count, Is.EqualTo(2));
            Assert.That(
                violations.Any(violation =>
                    violation.RelativePath == allowedPath &&
                    violation.Rule ==
                    DeucarianUILayeringArchitectureRule
                        .DirectPanelSettingsAssignment),
                Is.True);
            Assert.That(
                violations.Any(violation =>
                    violation.RelativePath.EndsWith(
                        "OtherVisualStyle.cs",
                        StringComparison.Ordinal) &&
                    violation.Rule ==
                    DeucarianUILayeringArchitectureRule
                        .DirectSortingOrderAssignment),
                Is.True);
        }

        [Test]
        public void SortingAllowlistRejectsBroadOrEscapingPaths()
        {
            Assert.Throws<ArgumentException>(() =>
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(
                        runtimeRoot,
                        new[] { runtimeRoot }));
            Assert.Throws<ArgumentException>(() =>
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(
                        runtimeRoot,
                        new[] { "../Outside.cs" }));
            Assert.Throws<ArgumentException>(() =>
                DeucarianUILayeringArchitectureValidator
                    .ValidateRuntimeRoot(
                        runtimeRoot,
                        new[] { "Reports/Attachments" }));
        }

        [Test]
        public void MissingRuntimeRootFailsWithActionablePath()
        {
            string missingRoot = Path.Combine(runtimeRoot, "Missing");

            DirectoryNotFoundException exception =
                Assert.Throws<DirectoryNotFoundException>(() =>
                    DeucarianUILayeringArchitectureValidator
                        .ValidateRuntimeRoot(missingRoot));

            StringAssert.Contains(
                Path.GetFullPath(missingRoot),
                exception.Message);
        }

        private void WriteSource(string relativePath, string source) =>
            WriteFile(relativePath, source);

        private void WriteAsset(string relativePath, string content) =>
            WriteFile(relativePath, content);

        private void WriteFile(string relativePath, string content)
        {
            string path = Path.Combine(
                runtimeRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
        }

        private static void AssertViolation(
            System.Collections.Generic.IEnumerable<
                DeucarianUILayeringArchitectureViolation> violations,
            DeucarianUILayeringArchitectureRule rule,
            int line)
        {
            DeucarianUILayeringArchitectureViolation violation =
                violations.Single(candidate => candidate.Rule == rule);
            Assert.That(violation.Line, Is.EqualTo(line));
        }
    }
}
