using Elements.Core;
using FrooxEngine.UIX;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EnumerableToolkit;

namespace MonkeyLoader.Resonite.UI
{
    [HarmonyPatchCategory(nameof(SyncArrayEditor))]
    [HarmonyPatch(typeof(SyncMemberEditorBuilder), nameof(SyncMemberEditorBuilder.BuildArray))]
    internal sealed class SyncArrayEditor : ResoniteMonkey<SyncArrayEditor>
    {
        private static readonly MethodInfo _addLinearValueProxying = AccessTools.Method(typeof(SyncArrayEditor), nameof(AddLinearValueProxying));
        private static readonly MethodInfo _addListReferenceProxying = AccessTools.Method(typeof(SyncArrayEditor), nameof(AddListReferenceProxying));
        private static readonly MethodInfo _addListValueProxying = AccessTools.Method(typeof(SyncArrayEditor), nameof(AddListValueProxying));
        private static readonly Type _iWorldElementType = typeof(IWorldElement);
        private static readonly Type _particleBurstType = typeof(ParticleBurst);

        public override bool CanBeDisabled => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static void AddLinearValueProxying<T>(SyncArray<LinearKey<T>> array, SyncElementList<ValueGradientDriver<T>.Point> list)
            where T : IEquatable<T>
        {
            foreach (var key in array)
            {
                var point = list.Add();
                point.Position.Value = key.time;
                point.Value.Value = key.value;
            }

            AddUpdateProxies(array, list, list.Elements);

            list.ElementsAdded += (list, startIndex, count) =>
            {
                var addedElements = list.Elements.Skip(startIndex).Take(count).ToArray();
                var buffer = addedElements.Select(point => new LinearKey<T>(point.Position, point.Value)).ToArray();

                array.Insert(buffer, startIndex);
                AddUpdateProxies(array, list, addedElements);
            };

            list.ElementsRemoved += (list, startIndex, count) => array.Remove(startIndex, count);
        }

        private static void AddListReferenceProxying<T>(SyncArray<T> array, SyncElementList<SyncRef<T>> list)
            where T : class, IEquatable<T>, IWorldElement
        {
            foreach (var reference in array)
            {
                var syncRef = list.Add();
                syncRef.Target = reference;
            }

            AddUpdateProxies(array, list, list.Elements);

            list.ElementsAdded += (list, startIndex, count) =>
            {
                var addedElements = list.Elements.Skip(startIndex).Take(count).ToArray();
                var buffer = addedElements.Select(syncRef => syncRef.Target).ToArray();

                array.Insert(buffer, startIndex);
                AddUpdateProxies(array, list, addedElements);
            };

            list.ElementsRemoved += (list, startIndex, count) => array.Remove(startIndex, count);
        }

        private static void AddListValueProxying<T>(SyncArray<T> array, SyncElementList<Sync<T>> list)
            where T : IEquatable<T>
        {
            foreach (var value in array)
            {
                var sync = list.Add();
                sync.Value = value;
            }

            AddUpdateProxies(array, list, list.Elements);

            list.ElementsAdded += (list, startIndex, count) =>
            {
                var addedElements = list.Elements.Skip(startIndex).Take(count).ToArray();
                var buffer = addedElements.Select(sync => sync.Value).ToArray();

                array.Insert(buffer, startIndex);
                AddUpdateProxies(array, list, addedElements);
            };

            list.ElementsRemoved += (list, startIndex, count) => array.Remove(startIndex, count);
        }

        private static void AddParticleBurstListProxying(SyncArray<LinearKey<ParticleBurst>> array, SyncElementList<ValueGradientDriver<int2>.Point> list)
        {
            foreach (var burst in array)
            {
                var point = list.Add();
                point.Position.Value = burst.time;
                point.Value.Value = new int2(burst.value.minCount, burst.value.maxCount);
            }

            AddUpdateProxies(array, list, list.Elements);

            list.ElementsAdded += (list, startIndex, count) =>
            {
                var addedElements = list.Elements.Skip(startIndex).Take(count).ToArray();
                var buffer = addedElements.Select(point => new LinearKey<ParticleBurst>(point.Position, new ParticleBurst() { minCount = point.Value.Value.x, maxCount = point.Value.Value.y })).ToArray();

                array.Insert(buffer, startIndex);
                AddUpdateProxies(array, list, addedElements);
            };

            list.ElementsRemoved += (list, startIndex, count) => array.Remove(startIndex, count);
        }

        private static void AddUpdateProxies<T>(SyncArray<LinearKey<T>> array,
            SyncElementList<ValueGradientDriver<T>.Point> list, IEnumerable<ValueGradientDriver<T>.Point> elements)
                    where T : IEquatable<T>
        {
            foreach (var point in elements)
            {
                point.Changed += syncObject =>
                {
                    var index = list.IndexOfElement(point);
                    array[index] = new LinearKey<T>(point.Position, point.Value);
                };
            }
        }

        private static void AddUpdateProxies(SyncArray<LinearKey<ParticleBurst>> array,
            SyncElementList<ValueGradientDriver<int2>.Point> list, IEnumerable<ValueGradientDriver<int2>.Point> elements)
        {
            foreach (var point in elements)
            {
                point.Changed += field =>
                {
                    var index = list.IndexOfElement(point);
                    var key = new LinearKey<ParticleBurst>(point.Position, new ParticleBurst() { minCount = point.Value.Value.x, maxCount = point.Value.Value.y });
                    array[index] = key;
                };
            }
        }

        private static void AddUpdateProxies<T>(SyncArray<T> array, SyncElementList<Sync<T>> list, IEnumerable<Sync<T>> elements)
                    where T : IEquatable<T>
        {
            foreach (var sync in elements)
            {
                sync.OnValueChange += field =>
                {
                    var index = list.IndexOfElement(sync);
                    array[index] = sync.Value;
                };
            }
        }

        private static void AddUpdateProxies<T>(SyncArray<T> array, SyncElementList<SyncRef<T>> list, IEnumerable<SyncRef<T>> elements)
            where T : class, IEquatable<T>, IWorldElement
        {
            foreach (var sync in elements)
            {
                sync.OnValueChange += field =>
                {
                    var index = list.IndexOfElement(sync);
                    array[index] = sync.Target;
                };
            }
        }

        private static Component GetOrAttachComponent(Slot targetSlot, Type type, out bool attachedNew)
        {
            attachedNew = false;
            if (targetSlot.GetComponent(type) is not Component comp)
            {
                comp = targetSlot.AttachComponent(type);
                attachedNew = true;
            }
            return comp;
        }

        private static bool Prefix(ISyncArray array, string name, FieldInfo fieldInfo, UIBuilder ui)
        {
            if (!Enabled) return true;

            if (!TryGetGenericParameters(typeof(SyncArray<>), array.GetType(), out var genericParameters))
                return true;

            var isSyncLinear = TryGetGenericParameters(typeof(SyncLinear<>), array.GetType(), out var syncLinearGenericParameters);

            var arrayType = genericParameters!.Value.First();
            var syncLinearType = syncLinearGenericParameters?.First();

            var isParticleBurst = syncLinearType == _particleBurstType;

            if (isSyncLinear && isParticleBurst)
                syncLinearType = typeof(int2);

            var proxySlotName = $"{name}-{array.ReferenceID}-Proxy";
            var proxiesSlot = ui.World.AssetsSlot;
            if (proxiesSlot.FindChild(proxySlotName) is not Slot proxySlot)
            {
                proxySlot = proxiesSlot.AddSlot(proxySlotName);
                array.FindNearestParent<IDestroyable>().Destroyed += (IDestroyable _) => proxySlot.Destroy();
            }
            proxySlot.DestroyWhenLocalUserLeaves();

            ISyncList list;
            FieldInfo listField;

            if (isSyncLinear && SupportsLerp(syncLinearType!))
            {
                var gradientType = typeof(ValueGradientDriver<>).MakeGenericType(syncLinearType);
                var gradient = GetOrAttachComponent(proxySlot, gradientType, out bool attachedNew);

                list = (ISyncList)gradient.GetSyncMember(nameof(ValueGradientDriver<float>.Points));
                listField = gradient.GetSyncMemberFieldInfo(nameof(ValueGradientDriver<float>.Points));

                if (attachedNew)
                {
                    if (isParticleBurst)
                        AddParticleBurstListProxying((SyncArray<LinearKey<ParticleBurst>>)array, (SyncElementList<ValueGradientDriver<int2>.Point>)list);
                    else
                        _addLinearValueProxying.MakeGenericMethod(syncLinearType).Invoke(null, new object[] { array, list });
                }
            }
            else
            {
                if (Coder.IsEnginePrimitive(arrayType))
                {
                    var multiplexerType = typeof(ValueMultiplexer<>).MakeGenericType(arrayType);
                    var multiplexer = GetOrAttachComponent(proxySlot, multiplexerType, out bool attachedNew);
                    list = (ISyncList)multiplexer.GetSyncMember(nameof(ValueMultiplexer<float>.Values));
                    listField = multiplexer.GetSyncMemberFieldInfo(nameof(ValueMultiplexer<float>.Values));

                    if (attachedNew)
                        _addListValueProxying.MakeGenericMethod(arrayType).Invoke(null, new object[] { array, list });
                }
                else if (_iWorldElementType.IsAssignableFrom(arrayType))
                {
                    var multiplexerType = typeof(ReferenceMultiplexer<>).MakeGenericType(arrayType);
                    var multiplexer = GetOrAttachComponent(proxySlot, multiplexerType, out bool attachedNew);
                    list = (ISyncList)multiplexer.GetSyncMember(nameof(ReferenceMultiplexer<Slot>.References));
                    listField = multiplexer.GetSyncMemberFieldInfo(nameof(ReferenceMultiplexer<Slot>.References));

                    if (attachedNew)
                        _addListReferenceProxying.MakeGenericMethod(arrayType).Invoke(null, new object[] { array, list });
                }
                else
                {
                    proxySlot.Destroy();
                    return true;
                }
            }

            SyncMemberEditorBuilder.BuildList(list, name, listField, ui);
            ui.Current[ui.Current.ChildrenCount - 1].DestroyWhenLocalUserLeaves();

            return false;
        }

        private static bool SupportsLerp(Type type)
        {
            var coderType = typeof(Coder<>).MakeGenericType(type);
            return Traverse.Create(coderType).Property<bool>(nameof(Coder<float>.SupportsLerp)).Value;
        }

        private static bool TryGetGenericParameters(Type baseType, Type concreteType, [NotNullWhen(true)] out Sequence<Type>? genericParameters)
        {
            genericParameters = null;

            if (concreteType is null || baseType is null || !baseType.IsGenericType)
                return false;

            if (concreteType.IsGenericType && concreteType.GetGenericTypeDefinition() == baseType)
            {
                genericParameters = concreteType.GetGenericArguments();
                return true;
            }

            return TryGetGenericParameters(baseType, concreteType.BaseType, out genericParameters);
        }
    }
}