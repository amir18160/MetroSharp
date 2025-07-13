using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Core
{
    public class AppException
    {
        public int StatusCore { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public AppException(int statusCore, string message, string details = null)
        {
            Details = details;
            Message = message;
            StatusCore = statusCore;
        }
    }
}