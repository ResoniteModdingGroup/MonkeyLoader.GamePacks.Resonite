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
    /// <summary>
    /// Defines an element of the <see cref="WorkerInspector">inspector</see> header text block.
    /// </summary>
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

        /// <summary>
        /// Creates a new text block element with the given locale string and optional overridden height.
        /// </summary>
        /// <param name="text">The header text that should be shown.</param>
        /// <param name="minHeight">The minimum height in <see cref="Canvas"/> units that should be used for the header text.</param>
        public InspectorHeaderText(LocaleString text, int minHeight = 100)
        {
            Text = text;
            MinHeight = minHeight;
        }
    }

    /// <summary>
    /// Represents the event fired to get the <see cref="InspectorHeaderText">text elements</see>
    /// for the <see cref="WorkerInspector">inspector</see> header text block of workers.
    /// </summary>
    public sealed class ResolveInspectorHeaderTextEvent : SortedItemsEvent<InspectorHeaderText>
    {
        /// <summary>
        /// Gets the vanilla <see cref="InspectorHeaderAttribute"/> of the
        /// <see cref="FrooxEngine.Worker"/> for which an inspector is being build, if present.
        /// </summary>
        public InspectorHeaderAttribute? DefaultHeader { get; }

        /// <summary>
        /// Gets or sets whether an item for the <see cref="DefaultHeader">DefaultHeader</see> was already added.
        /// </summary>
        /// <remarks>
        /// The <see cref="DefaultInspectorHeaderTextResolver"/> does this with
        /// <see cref="HarmonyLib.Priority.Normal">normal priority</see>.
        /// </remarks>
        public bool DefaultHeaderWasAdded { get; set; }

        /// <summary>
        /// Gets whether the <see cref="FrooxEngine.Worker"/> for which an inspector
        /// is being build has a <see cref="DefaultHeader">DefaultHeader</see>
        /// <see cref="InspectorHeaderAttribute">attribute</see>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(DefaultHeader))]
        public bool HasDefaultHeader => DefaultHeader is not null;

        /// <summary>
        /// Gets the <see cref="FrooxEngine.Worker"/> for which an inspector is being build.
        /// </summary>
        public Worker Worker { get; }

        /// <summary>
        /// Allows adding <see cref="InspectorHeaderText">text elements</see>
        /// to the <see cref="WorkerInspector">inspector</see> header text block of workers.
        /// </summary>
        /// <param name="worker">The <see cref="FrooxEngine.Worker"/> for which an inspector is being build.</param>
        internal ResolveInspectorHeaderTextEvent(Worker worker)
        {
            Worker = worker;
            DefaultHeader = worker.GetType().GetCustomAttribute<InspectorHeaderAttribute>();
        }
    }
}