namespace VectraRuntime.Executor.Models;

internal abstract record ExecuteResult
{
    public sealed record Normal(StackValue Value) : ExecuteResult;
    public sealed record Unwinding(StackValue AbortedValue) : ExecuteResult;
}