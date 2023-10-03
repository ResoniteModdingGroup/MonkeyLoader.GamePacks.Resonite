using MonkeyLoader.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.Features
{
    /// <summary>
    /// The Exit screen of the dash menu.
    /// </summary>
    public class DashExitScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Exit screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Exit Screen";
    }

    /// <summary>
    /// The File Browser screen of the dash menu.
    /// </summary>
    public class DashFileBrowserScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The File Browser screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash File Browser screen";
    }

    /// <summary>
    /// The Home screen of the dash menu.
    /// </summary>
    public class DashHomeScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Home screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Home Screen";
    }

    /// <summary>
    /// The Inventory screen of the dash menu.
    /// </summary>
    public class DashInventoryScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Inventory screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Inventory Screen";
    }

    /// <summary>
    /// Anything on the dash menu.
    /// </summary>
    public class DashMenu : GameFeature
    {
        /// <inheritdoc/>
        public override string Description { get; } = "Anything on the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Menu";
    }

    /// <summary>
    /// The Migration screen of the dash menu.
    /// </summary>
    public class DashMigrationScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Migration screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Migration Screen";
    }

    /// <summary>
    /// The Session screen of the dash menu.
    /// </summary>
    public class DashSessionScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Session screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Session Screen";
    }

    /// <summary>
    /// The Settings screen of the dash menu.
    /// </summary>
    public class DashSettingsScreen : DashMenu
    {
        /// <inheritdoc/>
        public override string Description { get; } = "The Settings screen of the dash menu.";

        /// <inheritdoc/>
        public override string Name { get; } = "Dash Settings Screen";
    }
}