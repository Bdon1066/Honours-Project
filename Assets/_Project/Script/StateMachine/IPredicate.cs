using System;

/// <summary>
/// Predicates are functions that test a condition and return a boolean value
/// </summary>
public interface IPredicate
{
    bool Evaluate();
}

public class FuncPredicate : IPredicate
{
    readonly Func<bool> func;
    public FuncPredicate(Func<bool> func)
    {
        this.func = func;
    }

    public bool Evaluate() => func.Invoke();
}