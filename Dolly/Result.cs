using System;

namespace Dolly;

internal sealed record Result<TValue>(TValue? Value, DiagnosticInfo? Error)
    where TValue : class, IEquatable<TValue>
{

    public static implicit operator Result<TValue>(TValue source) =>
        new Result<TValue>(source, null);


    public static implicit operator Result<TValue>(DiagnosticInfo error) =>
        new Result<TValue>(null, error);


    public void Handle(Action<TValue> handleSuccess, Action<DiagnosticInfo> handleFailure)
    {
        if (Value != null)
        {
            handleSuccess(Value);
        }
        else if (Error != null)
        {
            handleFailure(Error);
        }
        else
        {
            throw new ArgumentOutOfRangeException("Both 'Value' and 'Error' are null!!!!!");
        }
    }
}
