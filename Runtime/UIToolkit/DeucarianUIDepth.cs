using System;

namespace Deucarian.UI
{
    /// <summary>
    /// Concrete sorting values owned by the shared semantic screen-space UI
    /// policy. Consumers choose a <see cref="DeucarianUISurfaceRole"/> and
    /// configure through <see cref="DeucarianUIRuntime"/> rather than
    /// assigning raw values.
    /// </summary>
    public static class DeucarianUIDepth
    {
        public const int PrimaryControls = 1110;
        public const int ContextControls = 1111;
        public const int MediaControls = 1112;
        public const int Status = 1120;
        public const int Menu = 1121;
        public const int Modal = 1200;
        public const int Tooltip = 32760;

        /// <summary>
        /// Resolves the package-owned sorting depth for a semantic surface.
        /// Consumers should configure documents and canvases through
        /// <see cref="DeucarianUIRuntime"/> instead of assigning this value.
        /// </summary>
        public static int Resolve(DeucarianUISurfaceRole role)
        {
            switch (role)
            {
                case DeucarianUISurfaceRole.PrimaryControls:
                    return PrimaryControls;
                case DeucarianUISurfaceRole.ContextControls:
                    return ContextControls;
                case DeucarianUISurfaceRole.MediaControls:
                    return MediaControls;
                case DeucarianUISurfaceRole.Status:
                    return Status;
                case DeucarianUISurfaceRole.Menu:
                    return Menu;
                case DeucarianUISurfaceRole.Modal:
                    return Modal;
                case DeucarianUISurfaceRole.Tooltip:
                    return Tooltip;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role),
                        role,
                        "Unknown Deucarian UI surface role.");
            }
        }

        internal static bool IsTransient(DeucarianUISurfaceRole role) =>
            role == DeucarianUISurfaceRole.Menu ||
            role == DeucarianUISurfaceRole.Modal ||
            role == DeucarianUISurfaceRole.Tooltip;
    }
}
