using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    public static class FieldExtensions
    {
        public static Action<IChangeable> SyncWithConfigKey<T>(this IField<T> field, IDefiningConfigKey<T> configKey, string? eventLabel = null, bool allowWriteBack = true)
        {
            field.Value = configKey.GetValue()!;
            eventLabel ??= $"SyncedField.WriteBack.{field.World.GetIdentifier()}";

            void Handler(IChangeable _)
            {
                if (allowWriteBack)
                    configKey.SetValue(field.Value, eventLabel);
                else
                    field.World.RunSynchronously(() => field.Value = configKey.GetValue()!);
            }

            field.Changed += Handler;
            return Handler;
        }
    }
}