namespace MajorInteractiveBot.Utilities
{
    public interface IResult<TResult>
    {
        bool IsSuccess { get; }
        TResult Result { get; }
        string ErrorReason { get; }
        System.Exception Exception { get; }
    }
}
