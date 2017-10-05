using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GommeHDnetForumAPI.DataModels.Collections
{
    public class DataModelCollection<T> : Collection<T>
    {
        public DataModelCollection() {}

        public DataModelCollection(DataModelCollection<T> items) {
            AddRange(items);
        }

        public DataModelCollection(IEnumerable<T> items) {
            AddRange(items);
        }

        public override string ToString()
            => Items.Aggregate("", (s, t) => $"{s}{IndexOf(t)}: ({t}), ").TrimEnd(',', ' ');

        public void AddRange(DataModelCollection<T> items) {
            foreach (var item in items) {
                Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items) {
            foreach (var item in items) {
                Add(item);
            }
        }
    }
}
