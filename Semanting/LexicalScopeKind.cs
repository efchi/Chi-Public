namespace Chi.Semanting
{
    public enum LexicalScopeKind 
    {
        Singleton,  // Used for <global>, <module>, ...
        Instance    // Used for <def>, ...
    }
}
