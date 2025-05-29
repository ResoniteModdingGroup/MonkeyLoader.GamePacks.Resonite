using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Resonite.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace MonkeyLoader.Resonite.UI.Inspectors
{
    public sealed class InspectorHeaderText
    {
        /// <summary>
        /// Gets the minimum height that should be used for the header text.
        /// </summary>
        /// <value>In <see cref="Canvas"/> units.</value>
        public int MinHeight { get; }

        /// <summary>
        /// Gets the header text that should be shown.
        /// </summary>
        public LocaleString Text { get; }

        public InspectorHeaderText(LocaleString text, int minHeight = 100)
        {
            Text = text;
            MinHeight = minHeight;
        }
    }

    /// <summary>
    /// Represents the event fired to get the .
    /// </summary>
    public sealed class ResolveInspectorHeaderTextEvent : SortedItemsEvent<InspectorHeaderText>
    {
        public InspectorHeaderAttribute? DefaultHeader { get; }

        public bool DefaultHeaderWasAdded { get; set; }

        [MemberNotNullWhen(true, nameof(DefaultHeader))]
        public bool HasDefaultHeader => DefaultHeader is not null;

        /// <summary>
        /// Gets the <see cref="FrooxEngine.Worker"/> for which an inspector is being build.
        /// </summary>
        public Worker Worker { get; }

        /// <summary>
        /// Allows
        /// </summary>
        /// <param name="worker"></param>
        internal ResolveInspectorHeaderTextEvent(Worker worker)
        {
            Worker = worker;
            DefaultHeader = worker.GetType().GetCustomAttribute<InspectorHeaderAttribute>();
        }
    }
}