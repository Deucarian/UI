namespace Deucarian.UI
{
    /// <summary>
    /// Reserved runtime UI Toolkit sorting layers. Product UI may use any
    /// lower values; package-owned transient surfaces use these shared layers
    /// so their cross-document ordering remains deterministic.
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
    }
}
