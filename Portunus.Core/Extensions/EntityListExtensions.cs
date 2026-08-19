using Portunus.Core.Models;

namespace Portunus.Core.Extensions
{
    internal static class EntityListExtensions
    {
        public static void Upsert<T>(this List<T> list, T item) where T : IEntity
        {
            var i = list.FindIndex(x => x.Id == item.Id);
            if (i >= 0) list[i] = item; else list.Add(item);
        }

        public static bool RemoveById<T>(this List<T> list, Guid id) where T : IEntity
            => list.RemoveAll(x => x.Id == id) > 0;

        public static T? FindById<T>(this List<T> list, Guid id) where T : IEntity
            => list.FirstOrDefault(x => x.Id == id);
    }
}
