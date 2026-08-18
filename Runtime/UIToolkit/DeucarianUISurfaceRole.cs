namespace Deucarian.UI
{
    /// <summary>
    /// Semantic screen-space UI roles ordered by the shared Deucarian
    /// presentation policy. Consumers choose a role; com.deucarian.ui owns
    /// the concrete sorting values.
    /// </summary>
    public enum DeucarianUISurfaceRole
    {
        PrimaryControls = 0,
        ContextControls = 1,
        MediaControls = 2,
        Status = 3,
        Menu = 4,
        Modal = 5,
        Tooltip = 6
    }
}
