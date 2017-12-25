namespace ReadyGo {
  using System.Runtime.CompilerServices;
  
  public abstract class Benchmark {
    public abstract string Name {
      get;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public abstract void Setup();
    [MethodImpl(MethodImplOptions.NoInlining)]
    public abstract void Cleanup();
    [MethodImpl(MethodImplOptions.NoInlining)]
    public abstract void Go();
  }
}