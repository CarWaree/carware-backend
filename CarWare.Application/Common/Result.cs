using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWare.Application.Common
{
    public class Result<T>
    {
        public bool Success { get; }
        public string? Error { get; }
        public T? Data { get; }

        private Result(bool success, T? data, string? error)
        {
            Success = success;
            Error = error;
            Data = data;
        }

        public static Result<T> Ok(T data) => new(true, data, null);
        public static Result<T> Fail(string error) => new(false, default, error);
    }
}
