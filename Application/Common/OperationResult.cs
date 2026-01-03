namespace Application.Common
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static OperationResult<T> Success(T data, string? message = null)
        {
            return new OperationResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static OperationResult<T> Failure(string message)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Message = message
            };
        }

        public static OperationResult<T> Failure(List<string> errors)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }

    public class OperationResult : OperationResult<object>
    {
        public static OperationResult Success(string? message = null)
        {
            return new OperationResult
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static new OperationResult Failure(string message)
        {
            return new OperationResult
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
