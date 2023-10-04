using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TTo> Castable<TFrom, TTo>(this IEnumerable<TFrom> source) where TTo : TFrom
        {
            foreach (var item in source)
            {
                if (item is TTo toItem)
                    yield return toItem;
            }
        }
    }
}