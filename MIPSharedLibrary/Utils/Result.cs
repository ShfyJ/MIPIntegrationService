using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSharedLibrary.Utils
{
    public class Result<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static Result<T> SuccessResponse(T data)
        {
            return new Result<T> { Data = data, Success = true };
        }

        public static Result<T> ErrorResponse(string message)
        {
            return new Result<T> { Success = false, ErrorMessage = message };
        }
    }
}
