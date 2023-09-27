// Adapted from the NeosModLoader project.

namespace MonkeyLoader.Config
{
    /// <summary>
    /// Defines options for the handling of incompatible configuration versions.
    /// </summary>
    public enum IncompatibleConfigHandling
    {
        /// <summary>
        /// Fail to read the config, and block saving over the config on disk.
        /// </summary>
        Error,

        /// <summary>
        /// Destroy the saved config and start over from scratch.
        /// </summary>
        Clobber,

        /// <summary>
        /// Ignore the version number and attempt to load the config from disk.
        /// </summary>
        ForceLoad,
    }
}