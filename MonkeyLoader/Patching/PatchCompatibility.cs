using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Patching
{
    /// <summary>
    /// Indicates how compatible a patch for a particular feature is with others.
    /// <para/>
    /// Impact of the patch decreases with higher values.
    /// </summary>
    public enum PatchCompatibility
    {
        /// <summary>
        /// Indicates that a patch replaces the original version entirely.<br/>
        /// <see cref="HarmonyPrefix"/>es may also fall into this category,
        /// if they always stop the original from running.
        /// </summary>
        Replacement,

        /// <summary>
        /// Indicates that a patch modifies the internal functionality of the original, affecting the observed result.
        /// </summary>
        Modification,

        /// <summary>
        /// Indicates that a patch adds behavior before the original functionality runs.<br/>
        /// This should be used if the parameters are modified or the observed result is otherwise affected.
        /// </summary>
        Prefix,

        /// <summary>
        /// Indicates that a patch adds behavior after the original functionality has run.<br/>
        /// This should be used if the observed result is affected.
        /// </summary>
        Postfix,

        /// <summary>
        /// Indicates that a patch only adds something that doesn't affect the observed result.<br/>
        /// Example: adding extra logging to a commonly misbehaving method.
        /// </summary>
        DebugOnly,

        /// <summary>
        /// Indicates that a patch only hooks the original to do something else,
        /// leaving the original functionality otherwise untouched.
        /// </summary>
        HookOnly
    }
}