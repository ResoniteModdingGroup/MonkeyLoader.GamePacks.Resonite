using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite.DataFeeds
{
    public abstract class DataFeedBuilderMonkey<TMonkey> : ResoniteMonkey<TMonkey>
        where TMonkey : DataFeedBuilderMonkey<TMonkey>, new()
    {
    }
}