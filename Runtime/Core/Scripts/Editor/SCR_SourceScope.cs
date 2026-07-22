namespace Core.Editor
{
    public readonly ref struct SourceScope
    {
        private readonly SourceGenerator writer;

        internal SourceScope(SourceGenerator writer) => this.writer = writer;

        public void Dispose() => writer.EndScope();
    }
}
