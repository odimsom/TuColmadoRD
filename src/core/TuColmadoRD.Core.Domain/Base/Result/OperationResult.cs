using System;

namespace TuColmadoRD.Core.Domain.Base.Result
{
    public class OperationResult<TResult, TError>
    {
        private readonly TResult? _result;
        private readonly TError? _error;

        public bool IsGood { get; }

        public TResult? Result =>
            IsGood
                ? _result
                : throw new InvalidOperationException("The result of a bad operation cannot be accessed");

        public TError? Error =>
            !IsGood
                ? _error
                : throw new InvalidOperationException("Cannot access the error of a good operation");

        private OperationResult(bool isGood, TResult? result, TError? error)
        {
            IsGood = isGood;
            _result = result;
            _error = error;
        }

        public static OperationResult<TResult, TError> Good(TResult result)
        {
            return new OperationResult<TResult, TError>(true, result, default);
        }

        public static OperationResult<TResult, TError> Bad(TError error)
        {
            return new OperationResult<TResult, TError>(false, default, error);
        }
    }
}