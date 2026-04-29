using EnumerableToolkit;

namespace MonkeyLoader.Resonite
{
    internal static class SubgroupDefinitions
    {
        public static Sequence<string> ContextMenu { get; } = new string[] { "UI", "ContextMenu" };
        public static Sequence<string> Development { get; } = new string[] { "Development" };
        public static Sequence<string> Facets { get; } = new string[] { "UI", "Facets" };
        public static Sequence<string> GamePack { get; } = new string[] { "GamePack" };
        public static Sequence<string> InspectorDefaults { get; } = new string[] { "UI", "Inspectors", "Defaults" };
        public static Sequence<string> Inspectors { get; } = new string[] { "UI", "Inspectors" };
        public static Sequence<string> Locale { get; } = new string[] { "Locale" };
        public static Sequence<string> LocaleFallback { get; } = new string[] { "Locale", "Fallback" };
        public static Sequence<string> Settings { get; } = new string[] { "Settings" };
        public static Sequence<string> Tooltips { get; } = new string[] { "UI", "Tooltips" };
        public static Sequence<string> UI { get; } = new string[] { "UI" };
    }
}