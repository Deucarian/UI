using Deucarian.Theming;
using Object = UnityEngine.Object;

namespace Deucarian.UI
{
    /// <summary>
    /// Generic stacking slots for viewer control islands. Consumers attach
    /// their own feature meaning to a slot instead of exporting app semantics.
    /// </summary>
    public enum DeucarianControlIslandRow
    {
        Primary = 0,
        Secondary = 1,
        Tertiary = 2
    }

    /// <summary>
    /// Fully resolved control-island presentation shared by every consumer.
    /// </summary>
    public readonly struct DeucarianControlIslandPresentation
    {
        public DeucarianControlIslandPresentation(
            DeucarianTheme theme,
            DeucarianThemeStyle style,
            DeucarianControlIslandProfile profile)
        {
            Theme = theme;
            Style = style;
            Profile = profile;
        }

        public DeucarianTheme Theme { get; }
        public DeucarianThemeStyle Style { get; }
        public DeucarianControlIslandProfile Profile { get; }
        public DeucarianPanelChrome CompactPanel =>
            Profile.CreatePanelChrome();
        public DeucarianIconButtonChrome CompactIconButton =>
            Profile.CreateIconButtonChrome();

        /// <summary>
        /// Button chrome whose icons occupy one absolute centered slot. This is
        /// the canonical profile for cross-faded or swapped overlay icons.
        /// </summary>
        public DeucarianIconButtonChrome CenteredOverlayIconButton =>
            Profile.CreateIconButtonChrome(true);

        public static DeucarianControlIslandPresentation Resolve(
            DeucarianTheme theme = null,
            Object context = null)
        {
            DeucarianTheme resolvedTheme =
                DeucarianGlassPanelStyle.ResolveTheme(theme, context);
            DeucarianThemeStyle style =
                DeucarianGlassPanelStyle.ResolveStyle(
                    resolvedTheme,
                    context);
            return new DeucarianControlIslandPresentation(
                resolvedTheme,
                style,
                DeucarianControlIslandProfiles.Resolve(style));
        }
    }
}
