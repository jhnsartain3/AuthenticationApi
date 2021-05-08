using System;
using AutoWrapper.Extensions;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedModels;

namespace PublicEndpoints.Filters
{
    public class UnauthorizedAccessExceptionHandlerAttribute : IExceptionFilter
    {
        public void OnException(ExceptionContext exceptionContext)
        {
            if (exceptionContext.ExceptionHandled) return;
            if (exceptionContext.Exception is not UnauthorizedAccessException exception) return;
            if (!exception.Message.Equals("Username or Password is invalid")) throw new ApiException(exception);

            var modelState = exceptionContext.ModelState;

            modelState.AddModelError(nameof(UserModel.Username), exception.Message);
            modelState.AddModelError(nameof(UserModel.Password), exception.Message);
            throw new ApiException(modelState.AllErrors());
        }
    }
}