namespace pluginExtension;

using Markdig;
using Registry = Dictionary<Handle, IPluginNode>;

public static class PluginRegistry {
  private static readonly Registry BlockRegister = [];
  private static readonly Registry InlineRegister = [];
  private static readonly Registry HolderRegister = [];
  private static readonly Registry ParserRegister = [];
  private static readonly List<IMarkdownExtension> Extensions = [];
  private static Registry GetRegistry(Handle.Type type) => type switch
  {
    Handle.Type.ContainerBlock or Handle.Type.LeafBlock or Handle.Type.RawTextBlock => BlockRegister,
    Handle.Type.ContainerInline or Handle.Type.LeafInline => InlineRegister,
    Handle.Type.Holder => HolderRegister,
    Handle.Type.Parser => ParserRegister,
    _ => throw new NotImplementedException($"Handle Type {type} does not have an implemented register"),
  };
  private static Registry GetRegistry(Handle handle) => GetRegistry(handle.GetType());
  public static Handle Register(Handle handle, IPluginNode node)
  {
    Registry registry = GetRegistry(handle);
    registry[handle] = node;
    return handle;
  }
  public static Handle Register(Handle.Type type, IPluginNode node) => Register(Handle.Create(type), node);

  public static IPluginNode Get(Handle handle)
  {
    Registry registry = GetRegistry(handle);
    if (registry.TryGetValue(handle, out var value))
      return value;
    throw new KeyNotFoundException($"Handle for {handle} not registered");
  }

  public static void RegisterExtension(IMarkdownExtension extension) => Extensions.Add(extension);
  public static MarkdownPipelineBuilder UsePluginExtensions(this MarkdownPipelineBuilder pipeline)
  {
    Extensions.ForEach(ext => pipeline.Extensions.AddIfNotAlready(ext));
    return pipeline;
  }

  public static bool Dispose(Handle handle)
  {
    Registry registry = GetRegistry(handle);
    return registry.Remove(handle);
  }

  //TODO Parser,
  //TODO RawTextBlock,
  //TODO LeafBlock,
  //TODO ContainerBlock,
  //TODO LeafInline,
  //TODO ContainerInline,
  //TODO Holder,
}
