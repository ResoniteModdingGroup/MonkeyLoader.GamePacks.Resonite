using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Configuration
{
    public class RangedConfigKey<T> : ConfigKey<T>
    {
        public IComparer<T?> Comparer { get; }
        public T Maximum { get; }
        public T Minimum { get; }

        public RangedConfigKey(string name, string? description = null, Func<T>? computeDefault = null, bool internalAccessOnly = false, Predicate<T?>? valueValidator = null)
            : base(name, description, computeDefault, internalAccessOnly, rangeValidator)
        {
        }

        private bool rangeValidator(T? value)
            => Comparer.Compare(Minimum, value) <= 0 && Comparer.Compare(Maximum, value) >= 0
                     && (predicate?.Invoke(value) ?? true);
    }
}