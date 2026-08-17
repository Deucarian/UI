using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.UI
{
    /// <summary>Canonical Resources keys for shared runtime UI assets.</summary>
    public static class DeucarianUIRuntimeAssets
    {
        public const string Root = "Deucarian/UI/";
        public const string ControlIslandStyleSheet =
            Root + "DeucarianControlIsland";
        public const string RuntimePanelSettings =
            Root + "DeucarianRuntimePanelSettings";
        public const string ControlIslandStyle =
            ControlIslandStyleSheet;
        public const string PanelSettings =
            RuntimePanelSettings;

        public static StyleSheet LoadControlIslandStyleSheet() =>
            Resources.Load<StyleSheet>(ControlIslandStyleSheet);

        public static PanelSettings LoadRuntimePanelSettings() =>
            Resources.Load<PanelSettings>(RuntimePanelSettings);
    }
}
