using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains extension method versions of <see cref="RadiantUI_Constants"/> methods.
    /// </summary>
    [TypeForwardedFrom("MonkeyLoader.Resonite.Integration")]
    public static class RadiantUIStyleExtensions
    {
        /// <summary>
        /// Initialized this <see cref="UIBuilder"/> with the <see cref="RadiantUI_Constants.SetupBaseStyle">base style</see>.
        /// </summary>
        /// <inheritdoc cref="WithDefaultStyle"/>
        public static UIBuilder WithBaseStyle(this UIBuilder ui)
        {
            RadiantUI_Constants.SetupBaseStyle(ui);
            return ui;
        }

        /// <summary>
        /// Initialized this <see cref="UIBuilder"/> with the <see cref="RadiantUI_Constants.SetupButtonStyle">button style</see>.
        /// </summary>
        /// <inheritdoc cref="WithDefaultStyle"/>
        public static UIBuilder WithButtonStyle(this UIBuilder ui)
        {
            RadiantUI_Constants.SetupButtonStyle(ui);
            return ui;
        }

        /// <summary>
        /// Initialized this <see cref="UIBuilder"/> with the <see cref="RadiantUI_Constants.SetupDefaultStyle">default style</see>.
        /// </summary>
        /// <param name="ui">The UI Builder to initialize.</param>
        /// <param name="extraPadding">Whether to add extra padding around text / icons.</param>
        /// <returns>The initialized UI Builder.</returns>
        public static UIBuilder WithDefaultStyle(this UIBuilder ui, bool extraPadding = false)
        {
            RadiantUI_Constants.SetupDefaultStyle(ui, extraPadding);
            return ui;
        }

        /// <summary>
        /// Initialized this <see cref="UIBuilder"/> with the <see cref="RadiantUI_Constants.SetupEditorStyle(UIBuilder, bool)">editor style</see>.
        /// </summary>
        /// <inheritdoc cref="WithDefaultStyle"/>
        public static UIBuilder WithEditorStyle(this UIBuilder ui, bool extraPadding = false)
        {
            RadiantUI_Constants.SetupEditorStyle(ui, extraPadding);
            return ui;
        }
    }
}