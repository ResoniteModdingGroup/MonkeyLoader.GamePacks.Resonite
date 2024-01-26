using HarmonyLib;
using MonkeyLoader.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Contains comparers for <see cref="IMonkey"/>s / derived <see cref="MonkeyBase{TMonkey}"/> instances.
    /// </summary>
    public static class Monkey
    {
        /// <summary>
        /// Gets an <see cref="IMonkey"/>-comparer, that sorts patches with lower impact first.
        /// </summary>
        public static IComparer<IMonkey> AscendingComparer { get; } = new MonkeyComparer();

        /// <summary>
        /// Gets an <see cref="IMonkey"/>-comparer, that sorts patches with higher impact first.
        /// </summary>
        public static IComparer<IMonkey> DescendingComparer { get; } = new MonkeyComparer(false);

        private sealed class MonkeyComparer : IComparer<IMonkey>
        {
            private readonly int _factor;

            public MonkeyComparer(bool ascending = true)
            {
                _factor = ascending ? 1 : -1;
            }

            /// <inheritdoc/>
            public int Compare(IMonkey x, IMonkey y)
            {
                // If one of the mods has to come before the other,
                // all its patchers have to come before as well
                var modComparison = x.Mod.CompareTo(y.Mod);
                if (modComparison != 0)
                    return _factor * modComparison;

                // Only need the first as they're the highest impact ones.
                var biggestX = x.FeaturePatches.FirstOrDefault();
                var biggestY = y.FeaturePatches.FirstOrDefault();

                // Better declare features if you want to sort high
                if (biggestX is null)
                    return biggestY is null ? 0 : (-1 * _factor);

                if (biggestY is null)
                    return _factor;

                var impactComparison = _factor * biggestX.CompareTo(biggestY);
                if (impactComparison != 0)
                    return _factor * impactComparison;

                // Fall back to type name comparison just to avoid false ==
                return _factor * x.GetType().FullName.CompareTo(y.GetType().FullName);
            }
        }
    }

    /// <summary>
    /// Represents the base class for patchers that run after a game's assemblies have been loaded.<br/>
    /// All mod defined derivatives must derive from <see cref="Monkey{TMonkey}"/>,
    /// <see cref="ConfiguredMonkey{TMonkey, TConfigSection}"/>, or another class derived from it.
    /// </summary>
    /// <remarks>
    /// Game assemblies and their types can be directly referenced from these.
    /// </remarks>
    /// <inheritdoc/>
    public abstract class Monkey<TMonkey> : MonkeyBase<TMonkey> where TMonkey : Monkey<TMonkey>, new()
    {
        /// <summary>
        /// Allows creating only a single <typeparamref name="TMonkey"/> instance.
        /// </summary>
        protected Monkey()
        { }

        /// <inheritdoc/>
        public override sealed bool Run()
        {
            ThrowIfRan();
            Ran = true;

            Debug(() => "Running OnLoaded!");

            try
            {
                if (!OnLoaded())
                {
                    Failed = true;
                    Warn(() => "OnLoaded failed!");
                }
            }
            catch (Exception ex)
            {
                Failed = true;
                Error(() => ex.Format("OnLoaded threw an Exception:"));
            }

            return !Failed;
        }

        /// <summary>
        /// Called right after the game tooling packs and all the game's assemblies have been loaded.<br/>
        /// Use this to apply any patching and return <c>true</c> if it was successful.
        /// </summary>
        /// <remarks>
        /// <i>By default:</i> Applies the <see cref="Harmony"/> patches of the
        /// <see cref="Harmony.PatchCategory(string)">category</see> with this patcher's type's name.<br/>
        /// Easy to apply by using <c>[<see cref="HarmonyPatchCategory"/>(nameof(MyPatcher))]</c> attribute.
        /// </remarks>
        /// <returns>Whether the patching was successful.</returns>
        protected virtual bool OnLoaded()
        {
            var type = GetType();
            Harmony.PatchCategory(type.Assembly, type.Name);

            return true;
        }
    }
}