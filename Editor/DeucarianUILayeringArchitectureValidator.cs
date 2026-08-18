using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Deucarian.UI.Editor
{
    /// <summary>
    /// Package-boundary rules enforced by the UI layering architecture
    /// validator.
    /// </summary>
    public enum DeucarianUILayeringArchitectureRule
    {
        PanelSettingsReference,
        PanelSettingsAsset,
        DirectPanelSettingsLoad,
        DirectPanelSettingsAssignment,
        DirectSortingOrderAssignment,
        DirectOverrideSortingAssignment
    }

    /// <summary>
    /// A source or asset location that bypasses the package-owned layering
    /// contract.
    /// </summary>
    public sealed class DeucarianUILayeringArchitectureViolation
    {
        internal DeucarianUILayeringArchitectureViolation(
            DeucarianUILayeringArchitectureRule rule,
            string relativePath,
            int line,
            int column,
            string message)
        {
            Rule = rule;
            RelativePath = relativePath;
            Line = line;
            Column = column;
            Message = message;
        }

        public DeucarianUILayeringArchitectureRule Rule { get; }

        public string RelativePath { get; }

        public int Line { get; }

        public int Column { get; }

        public string Message { get; }

        public override string ToString() =>
            RelativePath + ":" + Line + ":" + Column + " [" + Rule +
            "] " + Message;
    }

    /// <summary>
    /// Validates that a consumer runtime delegates screen-space panel and
    /// depth ownership to <c>com.deucarian.ui</c>.
    /// </summary>
    public static class DeucarianUILayeringArchitectureValidator
    {
        private static readonly Regex PanelSettingsAssetSignature = new Regex(
            @"^\s*m_Script:\s*\{(?=[^}\r\n]*\bfileID:\s*19101\b)" +
            @"(?=[^}\r\n]*\bguid:\s*" +
            @"0000000000000000e000000000000000\b)[^}\r\n]*\}",
            RegexOptions.CultureInvariant | RegexOptions.Multiline);

        private static readonly SourceRule[] SourceRules =
        {
            new SourceRule(
                DeucarianUILayeringArchitectureRule.PanelSettingsReference,
                @"\bPanelSettings\b",
                "Consumer runtime references PanelSettings directly; use " +
                "DeucarianUIRuntime or DeucarianUIOverlayHost."),
            new SourceRule(
                DeucarianUILayeringArchitectureRule
                    .DirectPanelSettingsLoad,
                @"\bLoadRuntimePanelSettings\s*\(",
                "Consumer runtime loads the canonical PanelSettings directly; " +
                "use DeucarianUIRuntime.Configure."),
            new SourceRule(
                DeucarianUILayeringArchitectureRule
                    .DirectPanelSettingsAssignment,
                @"\.\s*panelSettings\s*=(?!=)",
                "Consumer runtime assigns panelSettings directly; choose a " +
                "semantic role through DeucarianUIRuntime."),
            new SourceRule(
                DeucarianUILayeringArchitectureRule
                    .DirectSortingOrderAssignment,
                @"\.\s*sortingOrder\s*=(?!=)",
                "Consumer runtime assigns sortingOrder directly; depth is " +
                "owned by DeucarianUISurfaceRole."),
            new SourceRule(
                DeucarianUILayeringArchitectureRule
                    .DirectOverrideSortingAssignment,
                @"\.\s*overrideSorting\s*=(?!=)",
                "Consumer runtime assigns overrideSorting directly; configure " +
                "screen-space UI through DeucarianUIRuntime.")
        };

        /// <summary>
        /// Scans C# and Unity asset files beneath a consumer Runtime root.
        /// Exact relative source paths may be allowed only for non-UI
        /// <c>sortingOrder</c> assignments, such as SpriteRenderer ordering.
        /// </summary>
        /// <param name="runtimeRoot">
        /// Absolute or relative path to the consumer's Runtime directory.
        /// </param>
        /// <param name="allowedNonUiSortingOrderPaths">
        /// Exact Runtime-relative C# paths whose sortingOrder assignments are
        /// known to target a non-UI renderer. No other rule is suppressed.
        /// </param>
        public static IReadOnlyList<
            DeucarianUILayeringArchitectureViolation> ValidateRuntimeRoot(
                string runtimeRoot,
                IEnumerable<string> allowedNonUiSortingOrderPaths = null)
        {
            string normalizedRoot = NormalizeRuntimeRoot(runtimeRoot);
            HashSet<string> sortingAllowlist = NormalizeSortingAllowlist(
                normalizedRoot,
                allowedNonUiSortingOrderPaths);
            var violations =
                new List<DeucarianUILayeringArchitectureViolation>();

            string[] sourcePaths = Directory.GetFiles(
                normalizedRoot,
                "*.cs",
                SearchOption.AllDirectories);
            Array.Sort(sourcePaths, StringComparer.OrdinalIgnoreCase);
            for (int sourceIndex = 0;
                 sourceIndex < sourcePaths.Length;
                 sourceIndex++)
            {
                ValidateSource(
                    normalizedRoot,
                    sourcePaths[sourceIndex],
                    sortingAllowlist,
                    violations);
            }

            string[] assetPaths = Directory.GetFiles(
                normalizedRoot,
                "*.asset",
                SearchOption.AllDirectories);
            Array.Sort(assetPaths, StringComparer.OrdinalIgnoreCase);
            for (int assetIndex = 0;
                 assetIndex < assetPaths.Length;
                 assetIndex++)
            {
                ValidateAsset(
                    normalizedRoot,
                    assetPaths[assetIndex],
                    violations);
            }

            violations.Sort(CompareViolations);
            return violations.AsReadOnly();
        }

        private static string NormalizeRuntimeRoot(string runtimeRoot)
        {
            if (string.IsNullOrWhiteSpace(runtimeRoot))
            {
                throw new ArgumentException(
                    "A consumer Runtime root is required.",
                    nameof(runtimeRoot));
            }

            string normalizedRoot = Path.GetFullPath(runtimeRoot);
            if (!Directory.Exists(normalizedRoot))
            {
                throw new DirectoryNotFoundException(
                    "Consumer Runtime root was not found: " +
                    normalizedRoot);
            }

            return normalizedRoot.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        }

        private static HashSet<string> NormalizeSortingAllowlist(
            string normalizedRoot,
            IEnumerable<string> allowedPaths)
        {
            var normalizedPaths = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);
            if (allowedPaths == null)
            {
                return normalizedPaths;
            }

            foreach (string path in allowedPaths)
            {
                if (string.IsNullOrWhiteSpace(path) ||
                    Path.IsPathRooted(path))
                {
                    throw new ArgumentException(
                        "Non-UI sorting allowances must be exact, " +
                        "Runtime-relative C# paths.",
                        nameof(allowedPaths));
                }

                string normalizedPath = NormalizeRelativePath(path);
                string absolutePath = Path.GetFullPath(
                    Path.Combine(
                        normalizedRoot,
                        normalizedPath.Replace(
                            '/',
                            Path.DirectorySeparatorChar)));
                string relativePath = ToRelativePath(
                    normalizedRoot,
                    absolutePath);
                if (!string.Equals(
                        relativePath,
                        normalizedPath,
                        StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(
                        Path.GetExtension(relativePath),
                        ".cs",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Non-UI sorting allowances must remain inside the " +
                        "Runtime root and target one exact C# file: " + path,
                        nameof(allowedPaths));
                }

                normalizedPaths.Add(relativePath);
            }

            return normalizedPaths;
        }

        private static void ValidateSource(
            string normalizedRoot,
            string sourcePath,
            HashSet<string> sortingAllowlist,
            ICollection<DeucarianUILayeringArchitectureViolation> violations)
        {
            string relativePath = ToRelativePath(
                normalizedRoot,
                sourcePath);
            string source = File.ReadAllText(sourcePath);
            string code = MaskCommentsAndLiterals(source);

            for (int ruleIndex = 0;
                 ruleIndex < SourceRules.Length;
                 ruleIndex++)
            {
                SourceRule rule = SourceRules[ruleIndex];
                if (rule.Rule == DeucarianUILayeringArchitectureRule
                        .DirectSortingOrderAssignment &&
                    sortingAllowlist.Contains(relativePath))
                {
                    continue;
                }

                MatchCollection matches = rule.Pattern.Matches(code);
                for (int matchIndex = 0;
                     matchIndex < matches.Count;
                     matchIndex++)
                {
                    AddViolation(
                        violations,
                        rule.Rule,
                        relativePath,
                        source,
                        matches[matchIndex].Index,
                        rule.Message);
                }
            }
        }

        private static void ValidateAsset(
            string normalizedRoot,
            string assetPath,
            ICollection<DeucarianUILayeringArchitectureViolation> violations)
        {
            string content = File.ReadAllText(assetPath);
            Match signature = PanelSettingsAssetSignature.Match(content);
            bool namedAsPanelSettings = Path.GetFileName(assetPath)
                .IndexOf(
                    "PanelSettings",
                    StringComparison.OrdinalIgnoreCase) >= 0;
            if (!signature.Success && !namedAsPanelSettings)
            {
                return;
            }

            AddViolation(
                violations,
                DeucarianUILayeringArchitectureRule.PanelSettingsAsset,
                ToRelativePath(normalizedRoot, assetPath),
                content,
                signature.Success ? signature.Index : 0,
                "Consumer runtime ships a local PanelSettings asset; use the " +
                "canonical com.deucarian.ui resource.");
        }

        private static void AddViolation(
            ICollection<DeucarianUILayeringArchitectureViolation> violations,
            DeucarianUILayeringArchitectureRule rule,
            string relativePath,
            string source,
            int index,
            string message)
        {
            ResolveLocation(source, index, out int line, out int column);
            violations.Add(
                new DeucarianUILayeringArchitectureViolation(
                    rule,
                    relativePath,
                    line,
                    column,
                    message));
        }

        private static int CompareViolations(
            DeucarianUILayeringArchitectureViolation left,
            DeucarianUILayeringArchitectureViolation right)
        {
            int pathComparison = StringComparer.OrdinalIgnoreCase.Compare(
                left.RelativePath,
                right.RelativePath);
            if (pathComparison != 0)
            {
                return pathComparison;
            }

            int lineComparison = left.Line.CompareTo(right.Line);
            if (lineComparison != 0)
            {
                return lineComparison;
            }

            int columnComparison = left.Column.CompareTo(right.Column);
            return columnComparison != 0
                ? columnComparison
                : left.Rule.CompareTo(right.Rule);
        }

        private static string ToRelativePath(
            string normalizedRoot,
            string absolutePath)
        {
            string prefix = normalizedRoot + Path.DirectorySeparatorChar;
            string normalizedPath = Path.GetFullPath(absolutePath);
            if (!normalizedPath.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeRelativePath(normalizedPath);
            }

            return NormalizeRelativePath(
                normalizedPath.Substring(prefix.Length));
        }

        private static string NormalizeRelativePath(string path) =>
            path.Replace('\\', '/')
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/')
                .TrimStart('/');

        private static void ResolveLocation(
            string source,
            int index,
            out int line,
            out int column)
        {
            line = 1;
            column = 1;
            int limit = Math.Min(index, source.Length);
            for (int characterIndex = 0;
                 characterIndex < limit;
                 characterIndex++)
            {
                if (source[characterIndex] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
        }

        private static string MaskCommentsAndLiterals(string source)
        {
            char[] code = new char[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                char character = source[index];
                code[index] = character == '\r' || character == '\n'
                    ? character
                    : ' ';
            }

            int cursor = 0;
            while (cursor < source.Length)
            {
                if (StartsWith(source, cursor, "//"))
                {
                    cursor = SkipLineComment(source, cursor + 2);
                    continue;
                }

                if (StartsWith(source, cursor, "/*"))
                {
                    cursor = SkipBlockComment(source, cursor + 2);
                    continue;
                }

                int rawPrefixLength = CountRun(source, cursor, '$');
                int rawQuoteStart = cursor + rawPrefixLength;
                int rawQuoteCount = CountRun(
                    source,
                    rawQuoteStart,
                    '"');
                if (rawQuoteCount >= 3)
                {
                    cursor = SkipRawString(
                        source,
                        rawQuoteStart + rawQuoteCount,
                        rawQuoteCount);
                    continue;
                }

                if (StartsWith(source, cursor, "$@\"") ||
                    StartsWith(source, cursor, "@$\""))
                {
                    cursor = SkipVerbatimString(source, cursor + 3);
                    continue;
                }

                if (StartsWith(source, cursor, "@\""))
                {
                    cursor = SkipVerbatimString(source, cursor + 2);
                    continue;
                }

                if (StartsWith(source, cursor, "$\""))
                {
                    cursor = SkipRegularLiteral(source, cursor + 2, '"');
                    continue;
                }

                if (source[cursor] == '"' || source[cursor] == '\'')
                {
                    char delimiter = source[cursor];
                    cursor = SkipRegularLiteral(
                        source,
                        cursor + 1,
                        delimiter);
                    continue;
                }

                code[cursor] = source[cursor];
                cursor++;
            }

            return new string(code);
        }

        private static int SkipLineComment(string source, int cursor)
        {
            while (cursor < source.Length && source[cursor] != '\n')
            {
                cursor++;
            }

            return cursor;
        }

        private static int SkipBlockComment(string source, int cursor)
        {
            while (cursor < source.Length)
            {
                if (StartsWith(source, cursor, "*/"))
                {
                    return cursor + 2;
                }

                cursor++;
            }

            return cursor;
        }

        private static int SkipRegularLiteral(
            string source,
            int cursor,
            char delimiter)
        {
            while (cursor < source.Length)
            {
                if (source[cursor] == '\\')
                {
                    cursor += Math.Min(2, source.Length - cursor);
                    continue;
                }

                if (source[cursor] == delimiter)
                {
                    return cursor + 1;
                }

                cursor++;
            }

            return cursor;
        }

        private static int SkipVerbatimString(string source, int cursor)
        {
            while (cursor < source.Length)
            {
                if (source[cursor] != '"')
                {
                    cursor++;
                    continue;
                }

                if (cursor + 1 < source.Length &&
                    source[cursor + 1] == '"')
                {
                    cursor += 2;
                    continue;
                }

                return cursor + 1;
            }

            return cursor;
        }

        private static int SkipRawString(
            string source,
            int cursor,
            int quoteCount)
        {
            while (cursor < source.Length)
            {
                if (CountRun(source, cursor, '"') >= quoteCount)
                {
                    return cursor + quoteCount;
                }

                cursor++;
            }

            return cursor;
        }

        private static int CountRun(
            string source,
            int cursor,
            char character)
        {
            int count = 0;
            while (cursor + count < source.Length &&
                   source[cursor + count] == character)
            {
                count++;
            }

            return count;
        }

        private static bool StartsWith(
            string source,
            int cursor,
            string value)
        {
            if (cursor + value.Length > source.Length)
            {
                return false;
            }

            for (int valueIndex = 0;
                 valueIndex < value.Length;
                 valueIndex++)
            {
                if (source[cursor + valueIndex] != value[valueIndex])
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class SourceRule
        {
            internal SourceRule(
                DeucarianUILayeringArchitectureRule rule,
                string pattern,
                string message)
            {
                Rule = rule;
                Pattern = new Regex(
                    pattern,
                    RegexOptions.CultureInvariant);
                Message = message;
            }

            internal DeucarianUILayeringArchitectureRule Rule { get; }

            internal Regex Pattern { get; }

            internal string Message { get; }
        }
    }
}
