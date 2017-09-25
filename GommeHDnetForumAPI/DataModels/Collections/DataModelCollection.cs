using System.Collections.ObjectModel;
using System.Linq;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class DataModelCollection<T> : Collection<T>
    {
        public override string ToString()
            => Items.Aggregate("", (s, t) => $"{s}{IndexOf(t)}: ({t})\n").TrimEnd('\n');
    }
}
