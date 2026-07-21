namespace Core
{
    public sealed class SearchCollection<T>
    {
        public readonly SearchEntry<T>[] Entries;
        public SearchCollection(SearchEntry<T>[] entries) => Entries = entries;
    }
}
