using System;
using System.Collections.Generic;

namespace InsightVisitorGroups.Impl
{
    public interface IMailing
    {
        IList<Tuple<long, string, string>> GetActiveMailings();
    }
}
