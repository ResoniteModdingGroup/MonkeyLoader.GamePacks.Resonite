using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using System.Reflection;

namespace MonkeyLoader.Resonite.UI
{
    /// <summary>
    /// Contains extension methods to setup <see cref="IButton"/>s with links to the Resonite Wiki, which are triggerable by anyone.
    /// </summary>
    public static class WikiLinkButtonExtensions
    {
        private static readonly Dictionary<string, string> _nameOverrides = new()
        {
            { "Engine.DynamicVariables.Input", "DynamicVariableInput" },
            { "Engine.DynamicVariables.InputWithEvents", "DynamicVariableInputWithEvents" },
        };

        internal static LocaleString ComponentLocale { get; set; }

        internal static LocaleString ProtoFluxLocale { get; set; }

        /// <summary>
        /// Sets up an <see cref="IButton"/> with a <see cref="Hyperlink"/> to the given wiki <paramref name="page"/>,
        /// optionally setting the <paramref name="reason"/> for opening it.
        /// </summary>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with a wiki link.</param>
        /// <param name="page">The wiki page to link to.</param>
        /// <param name="reason">The optional reason for opening the link.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithWikiLink<TButton>(this TButton button, string page, in LocaleString reason = default)
            where TButton : IButton
        {
            var hyperlink = button.Slot.AttachComponent<Hyperlink>();
            hyperlink.URL.Value = new Uri($"{button.Slot.Engine.PlatformProfile.Wiki}/{page}");
            hyperlink.Reason.AssignLocaleString(reason);

            return button;
        }

        //private static readonly Type _componentType = typeof(Component);
        //private static readonly Type _protoFluxNodeType = typeof(ProtoFluxNode);
        //public static TButton WithWikiLinkFor<TButton>(this TButton button, Type workerType)
        //    where TButton : IButton
        //{
        //    while (workerType.IsNested)
        //        workerType = workerType.DeclaringType!;

        //    var pagePrefix = workerType.GetCustomAttribute<WikiPrefixAttribute>(inherit: true)?.Prefix ?? "";

        //    string wikiPage;
        //    LocaleString reason;

        //    if (workerType.IsAssignableTo(_protoFluxNodeType))
        //    {
        //        var nodeName = node.NodeName;
        //        var overload = NodeMetadataHelper.GetMetadata(node.NodeType).Overload;

        //        if (!string.IsNullOrEmpty(overload))
        //        {
        //            if (_nameOverrides.TryGetValue(overload, out var overrideName))
        //            {
        //                nodeName = overrideName;
        //            }
        //            else
        //            {
        //                var dotIndex = overload.LastIndexOf('.');

        //                nodeName = dotIndex > 0 ? overload[(dotIndex + 1)..] : nodeName;
        //            }
        //        }

        //        wikiPage = $"{pagePrefix}{nodeName.Replace(' ', '_')}";
        //        reason = ProtoFluxLocale;
        //    }
        //    else
        //    {
        //        var workerName = worker.WorkerType.Name;

        //        // Don't need to remove the `1 on generics - they redirect and may actually be different
        //        wikiPage = $"{pagePrefix}{workerName}";
        //        reason = ComponentLocale;
        //    }

        //    return button
        //        .WithTooltip(reason)
        //        .WithWikiLink(wikiPage, reason);
        //}

        /// <summary>
        /// Sets up an <see cref="IButton"/> with a <see cref="Hyperlink"/> to the wiki page for the given <paramref name="worker"/>.
        /// </summary>
        /// <remarks>
        /// This method automatically sets a tooltip and reason for opening the link.
        /// </remarks>
        /// <typeparam name="TButton">The specific type of the button.</typeparam>
        /// <param name="button">The button to set up with a wiki link.</param>
        /// <param name="worker">The worker to the link the wiki page for.</param>
        /// <returns>The unchanged button.</returns>
        public static TButton WithWikiLinkFor<TButton>(this TButton button, IWorker worker)
            where TButton : IButton
        {
            if (worker is ProtoFluxEngineProxy proxy)
                worker = proxy.Node.Target ?? worker;

            var pagePrefix = worker.GetType().GetCustomAttribute<WikiPrefixAttribute>(inherit: true)?.Prefix ?? "";

            string wikiPage;
            LocaleString reason;

            if (worker is ProtoFluxNode node)
            {
                var nodeName = node.NodeName;
                var overload = NodeMetadataHelper.GetMetadata(node.NodeType).Overload;

                if (!string.IsNullOrEmpty(overload))
                {
                    if (_nameOverrides.TryGetValue(overload, out var overrideName))
                    {
                        nodeName = overrideName;
                    }
                    else
                    {
                        var dotIndex = overload.LastIndexOf('.');

                        nodeName = dotIndex > 0 ? overload[(dotIndex + 1)..] : nodeName;
                    }
                }

                wikiPage = $"{pagePrefix}{nodeName.Replace(' ', '_')}";
                reason = ProtoFluxLocale;
            }
            else
            {
                var workerName = worker.WorkerType.Name;

                // Don't need to remove the `1 on generics - they redirect and may actually be different
                wikiPage = $"{pagePrefix}{workerName}";
                reason = ComponentLocale;
            }

            return button
                .WithTooltip(reason)
                .WithWikiLink(wikiPage, reason);
        }
    }
}