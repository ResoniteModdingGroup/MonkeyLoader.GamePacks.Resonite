﻿using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using MonkeyLoader.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds.Settings
{
    /// <summary>
    /// Stores data about the <see cref="FrooxEngine.SettingsDataFeed"/>
    /// <see cref="FrooxEngine.RootCategoryView">Views</see> in Userspace - there can be any number of these.
    /// </summary>
    public sealed class SettingsViewData
    {
        private const string LegacyInjectedColorXTemplateName = "Injected DataFeedValueField<colorX>";
        private bool _legacyColorXTemplateCleanupDone;
        private RootCategoryView? _rootCategoryView = null;
        private Slider<float>? _scrollSlider = null;

        /// <summary>
        /// Gets whether this settings view data has a <see cref="Mapper">Mapper</see>.
        /// </summary>
        /// <value><c>true</c> if <see cref="Mapper">Mapper</see> is not <c>null</c>; otherwise, <c>false</c>.</value>
        [MemberNotNullWhen(true, nameof(Mapper))]
        public bool HasMapper => Mapper is not null;

        /// <summary>
        /// Gets whether this settings view data has a <see cref="RootCategoryView">RootCategoryView</see>.
        /// </summary>
        /// <value><c>true</c> if <see cref="RootCategoryView">RootCategoryView</see> is not <c>null</c>; otherwise, <c>false</c>.</value>
        [MemberNotNullWhen(true, nameof(RootCategoryView))]
        public bool HasRootCategoryView => RootCategoryView is not null;

        /// <summary>
        /// Gets the <see cref="DataFeedItemMapper"/> used by the <see cref="RootCategoryView">RootCategoryView</see>
        /// that's displaying the <see cref="SettingsDataFeed">SettingsDataFeed</see> this view data belongs to.
        /// </summary>
        public DataFeedItemMapper? Mapper => RootCategoryView?.ItemsManager.TemplateMapper.Target.FilterWorldElement();

        /// <summary>
        /// Gets the <see cref="RootCategoryView">RootCategoryView</see> that's displaying
        /// the <see cref="SettingsDataFeed">SettingsDataFeed</see> this view data belongs to.
        /// </summary>
        public RootCategoryView? RootCategoryView
        {
            get => _rootCategoryView = _rootCategoryView!.FilterWorldElement();
            internal set
            {
                if (ReferenceEquals(_rootCategoryView, value))
                    return;

                CleanupView();
                _rootCategoryView = value;
                SetupView();
            }
        }

        /// <summary>
        /// Gets the <see cref="FrooxEngine.SettingsDataFeed"/> that this view data belongs to.
        /// </summary>
        public SettingsDataFeed SettingsDataFeed { get; }

        /// <summary>
        /// Gets whether this view data has a cached <see cref="ScrollSlider">ScrollSlider</see>.
        /// </summary>
        /// <value><c>true</c> if <see cref="ScrollSlider">ScrollSlider</see> is not <c>null</c>; otherwise, <c>false</c>.</value>
        [MemberNotNullWhen(true, nameof(ScrollSlider))]
        internal bool HasScrollSlider => ScrollSlider is not null;

        /// <summary>
        /// Gets the tracked scroll amounts for the previously displayed pages.
        /// </summary>
        internal Stack<float> ScrollAmounts { get; } = new();

        /// <summary>
        /// Gets the cached scroll slider used by the <see cref="RootCategoryView">RootCategoryView</see>
        /// that's displaying the <see cref="SettingsDataFeed">SettingsDataFeed</see> this view data belongs to.
        /// </summary>
        internal Slider<float>? ScrollSlider
        {
            get => _scrollSlider = _scrollSlider!.FilterWorldElement();
            set => _scrollSlider = value;
        }

        private static Logger Logger => MonkeyLoaderRootCategorySettingsItems.Logger;

        internal SettingsViewData(SettingsDataFeed dataFeed)
        {
            SettingsDataFeed = dataFeed;

            if (!dataFeed.World.IsUserspace())
                return;

            RootCategoryView = dataFeed.Slot.GetComponent<RootCategoryView>();

            var settingsListSlot = dataFeed.Slot.FindChild(s => s.Name == "Settings List", maxDepth: 2);
            if (settingsListSlot is null)
                return;

            var scrollBarSlot = settingsListSlot.FindChild(s => s.Name == "Scroll Bar", maxDepth: 2);
            if (scrollBarSlot is null)
                return;

            var slider = scrollBarSlot.GetComponentInChildren<Slider<float>>();
            if (slider is null)
                return;

            ScrollSlider = slider;
            Logger.Debug(() => "Cached settings scroll slider.");
        }

        /// <summary>
        /// Ensures a <see cref="DataFeedValueField{T}"/> template exists for the given <see cref="Type"/>
        /// in this <see cref="SettingsViewData">SettingsViewData's</see> <see cref="DataFeedItemMapper"/>
        /// </summary>
        public void EnsureDataFeedValueFieldTemplate(Type typeToInject)
        {
            if (!HasMapper)
            {
                Logger.Error(() => "DataFeedItemMapper is null in EnsureDataFeedValueFieldTemplate!");
                return;
            }

            if (!typeToInject.IsInjectableEditorType())
            {
                Logger.Error(() => $"Attempted to inject unsupported editor type {typeToInject.CompactDescription()} in EnsureDataFeedValueFieldTemplate!");
                return;
            }

            // Cleanup previously injected colorX templates that were accidentally made persistent and may have been saved with the dash
            if (typeToInject == typeof(colorX) && !_legacyColorXTemplateCleanupDone)
            {
                Logger.Info(() => "Looking for previously injected colorX templates.");

                foreach (var mapping in Mapper.Mappings.Where(mapping => mapping.MatchingType == typeof(DataFeedValueField<colorX>) && mapping.Template.Target?.Slot.Name == LegacyInjectedColorXTemplateName).ToArray())
                {
                    mapping.Template.Target.Slot.Destroy();
                    Mapper.Mappings.Remove(mapping);
                    Logger.Info(() => "Cleaned up a previously injected colorX template.");
                }

                _legacyColorXTemplateCleanupDone = true;
            }

            var dataFeedValueFieldType = typeof(DataFeedValueField<>).MakeGenericType(typeToInject);
            if (!Mapper.Mappings.Any(mapping => mapping.MatchingType == dataFeedValueFieldType && mapping.Template.Target != null))
            {
                var templatesRoot = Mapper.Slot.Parent?.FindChild("Templates");
                if (templatesRoot != null)
                {
                    var changeIndex = false;
                    var mapping = Mapper.Mappings.FirstOrDefault(mapping => mapping.MatchingType == dataFeedValueFieldType && mapping.Template.Target == null);

                    if (mapping == null)
                    {
                        mapping = Mapper.Mappings.Add();
                        mapping.MatchingType.Value = dataFeedValueFieldType;
                        changeIndex = true;
                    }

                    var template = templatesRoot.AddSlot($"Injected DataFeedValueField<{typeToInject.Name}>");
                    template.ActiveSelf = false;
                    template.PersistentSelf = false;
                    template.AttachComponent<LayoutElement>();

                    var ui = new UIBuilder(template);
                    RadiantUI_Constants.SetupEditorStyle(ui);

                    ui.ForceNext = template.AttachComponent<RectTransform>();
                    ui.HorizontalLayout(11.78908f, 11.78908f).ForceExpandWidth.Value = false;

                    ui.PushStyle();
                    ui.Style.FlexibleWidth = 1f;
                    var text = ui.Text("Label");
                    ui.PopStyle();

                    text.Size.Value = 24f;
                    text.HorizontalAlign.Value = TextHorizontalAlignment.Left;

                    Component component;
                    ISyncMember member;
                    FieldInfo fieldInfo;

                    if (typeToInject == typeof(Type))
                    {
                        component = template.AttachComponent<TypeField>();
                        member = ((TypeField)component).Type;
                        fieldInfo = component.GetSyncMemberFieldInfo("Type");
                    }
                    else
                    {
                        component = template.AttachComponent(typeof(ValueField<>).MakeGenericType(typeToInject));
                        member = component.GetSyncMember("Value");

                        if (member == null)
                        {
                            Logger.Error(() => $"Could not get Value sync member from attached ValueField<{typeToInject.Name}> component!");
                            return;
                        }

                        fieldInfo = component.GetSyncMemberFieldInfo("Value");
                    }

                    ui.PushStyle();
                    ui.Style.MinWidth = 521.36f;
                    SyncMemberEditorBuilder.Build(member, null!, fieldInfo, ui, 0f);
                    ui.PopStyle();

                    var memberActions = ui.Root?.GetComponentInChildren<InspectorMemberActions>()?.Slot;
                    if (memberActions != null)
                        memberActions.ActiveSelf = false;

                    var feedValueFieldInterface = template.AttachComponent(typeof(FeedValueFieldInterface<>).MakeGenericType(typeToInject));

                    ((FeedItemInterface)feedValueFieldInterface).ItemName.Target = text.Content;

                    if (feedValueFieldInterface.GetSyncMember("Value") is not ISyncRef valueField)
                        Logger.Error(() => "Could not get Value sync member from attached FeedValueFieldInterface component!");
                    else
                        valueField.Target = member;

                    var innerInterfaceSlot = templatesRoot.FindChild("InnerContainerItem");
                    if (innerInterfaceSlot != null)
                    {
                        var innerInterface = innerInterfaceSlot.GetComponent<FeedItemInterface>();

                        ((FeedItemInterface)feedValueFieldInterface).ParentContainer.Target = innerInterface;
                    }
                    else
                    {
                        Logger.Error(() => "InnerContainerItem slot is null in EnsureDataFeedValueFieldTemplate!");
                    }

                    mapping.Template.Target = (FeedItemInterface)feedValueFieldInterface;

                    if (changeIndex)
                    {
                        // Move the new mapping above the previous last element (default DataFeedItem mapping) in the list
                        Mapper.Mappings.MoveToIndex(Mapper.Mappings.Count() - 1, Mapper.Mappings.Count() - 2);
                    }

                    Logger.Info(() => $"Injected DataFeedValueField<{typeToInject.Name}> template");
                }
                else
                {
                    Logger.Error(() => "Could not find Templates slot in EnsureDataFeedValueFieldTemplate!");
                }
            }
            else
            {
                // This could cause some log spam
                //Logger.Trace(() => $"Existing DataFeedValueField<{typeToInject.Name}> template found.");
            }
        }

        internal void MoveUpFromCategory(string category)
        {
            if (!HasRootCategoryView || RootCategoryView.Path[^1] != category)
                return;

            Logger.Debug(() => $"Moving up from category: {category}");
            RootCategoryView.MoveUpInCategory();
        }

        private void CleanupView()
        {
            if (!HasRootCategoryView)
                return;

            RootCategoryView.Path.ElementsAdded -= OnElementsAdded;
            RootCategoryView.Path.ElementsRemoved -= OnElementsRemoved;

            Logger.Debug(() => "Unsubscribed from RootCategoryView events.");
        }

        private void OnElementsAdded(SyncElementList<Sync<string>> list, int start, int count)
        {
            Logger.Trace(() => $"OnElementsAdded. start: {start} count: {count}");

            // we don't need to store the value if we are at the root
            if (start == 0 && count == 1)
                return;

            var rootCategoryView = list.FindNearestParent<RootCategoryView>();
            if (rootCategoryView?.Feed.Target is not SettingsDataFeed settingsDataFeed)
            {
                RootCategoryView = null;
                return;
            }

            if (HasScrollSlider)
            {
                var value = ScrollSlider.Value.Value;
                ScrollAmounts.Push(value);

                Logger.Trace(() => $"Pushed value {value}. _scrollAmounts count: {ScrollAmounts.Count}");
            }
        }

        private void OnElementsRemoved(SyncElementList<Sync<string>> list, int start, int count)
        {
            Logger.Trace(() => $"OnElementsRemoved. start: {start} count: {count}");

            var rootCategoryView = list.FindNearestParent<RootCategoryView>();
            if (rootCategoryView?.Feed.Target is not SettingsDataFeed settingsDataFeed)
            {
                RootCategoryView = null;
                return;
            }

            if (start == 0)
            {
                ScrollAmounts.Clear();
                Logger.Trace(() => $"Cleared _scrollAmounts.");
                return;
            }

            var poppedValue = 0f;

            for (var i = 0; i < count; ++i)
            {
                if (ScrollAmounts.Count > 0)
                {
                    poppedValue = ScrollAmounts.Pop();
                    Logger.Trace(() => $"Popped value {poppedValue}. _scrollAmounts count: {ScrollAmounts.Count}");
                }
            }

            if (!HasScrollSlider)
                return;

            ScrollSlider!.RunInUpdates(3, () =>
            {
                if (!HasScrollSlider)
                    return;

                ScrollSlider.Value.Value = poppedValue;
                Logger.Debug(() => $"Set scroll slider to value {poppedValue}");
            });
        }

        private void SetupView()
        {
            if (!HasRootCategoryView)
                return;

            RootCategoryView.Path.ElementsAdded += OnElementsAdded;
            RootCategoryView.Path.ElementsRemoved += OnElementsRemoved;

            Logger.Debug(() => "Cached RootCategoryView and subscribed to events.");
        }
    }
}