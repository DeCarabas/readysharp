namespace ReadyGo {
  using System;
  using System.Runtime.CompilerServices;

  public interface IBenchmark {
    string Name {
      get;
    }
    void Setup();
    void Cleanup();
    void Go();
  }

  public abstract class BenchmarkBase : IBenchmark {
    public abstract string Name {
      get;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public virtual void Setup() {
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public virtual void Cleanup() {
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public abstract void Go();
  }

  public sealed class DelegateBenchmark : IBenchmark {
    readonly string name;
    readonly Action cleanupAction;
    readonly Action setupAction;
    readonly Action goAction;

    static readonly Action Nothing = () => { };

    public DelegateBenchmark(
      string name,
      Action goAction,
      Action setupAction = null,
      Action cleanupAction = null) {
      if (String.IsNullOrWhiteSpace(name)) {
        throw new ArgumentNullException(nameof(name));
      }
      if (goAction == null) {
        throw new ArgumentNullException(nameof(goAction));
      }

      this.name = name;
      this.goAction = goAction;
      this.setupAction = setupAction ?? Nothing;
      this.cleanupAction = cleanupAction ?? Nothing;
    }

    public string Name => this.name;
    public void Cleanup() {
      this.cleanupAction();
    }
    public void Setup() {
      this.setupAction();
    }
    public void Go() {
      this.goAction();
    }
  }
}