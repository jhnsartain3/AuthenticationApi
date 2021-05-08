using System;
using System.Collections.Generic;
using System.Linq;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using PublicEndpoints.Filters;
using SharedModels;

namespace PublicEndpointsTests
{
    public class UnauthorizedAccessExceptionHandlerAttributeTests
    {
        private ActionContext _actionContext;
        private Mock<ArgumentException> _argumentExceptionMock;
        private ExceptionContext _exceptionContext;
        private UnauthorizedAccessExceptionHandlerAttribute _systemUnderTest;

        [SetUp]
        public void Setup()
        {
            _actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            _argumentExceptionMock = new Mock<ArgumentException>();

            _argumentExceptionMock.Setup(e => e.Message).Returns("Reason for exception");
            _argumentExceptionMock.Setup(e => e.ParamName).Returns("NameOfProperty");

            _exceptionContext = new ExceptionContext(_actionContext, new List<IFilterMetadata>())
                {Exception = _argumentExceptionMock.Object};

            _systemUnderTest = new UnauthorizedAccessExceptionHandlerAttribute();
        }

        [Test]
        public void OnException_DoNothing_If_ExceptionIsAlreadyHandled()
        {
            _exceptionContext.ExceptionHandled = true;

            _systemUnderTest.OnException(_exceptionContext);
        }

        [Test]
        public void OnException_DoNothing_If_Exception_IsNot_UnauthorizedAccessException()
        {
            _exceptionContext.ExceptionHandled = false;
            _exceptionContext.Exception = new InvalidCastException();

            _systemUnderTest.OnException(_exceptionContext);
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionMessageIsNotExpected()
        {
            _exceptionContext.ExceptionHandled = false;
            _exceptionContext.Exception = new UnauthorizedAccessException("Username or Password is invalid");

            Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionMessage_IsExpected()
        {
            var originalException = new UnauthorizedAccessException("Test Exception");

            _exceptionContext.ExceptionHandled = false;
            _exceptionContext.Exception = originalException;

            var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

            Assert.AreEqual(originalException.Message, exception.Message);
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectErrorCount()
        {
            _exceptionContext.ExceptionHandled = false;
            _exceptionContext.Exception = new UnauthorizedAccessException("Username or Password is invalid");

            var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

            Assert.AreEqual(2, exception.Errors.Count());
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectNameError()
        {
            _exceptionContext.Exception = new UnauthorizedAccessException("Username or Password is invalid");

            var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

            Assert.AreEqual(nameof(UserModel.Password), exception.Errors.First().Name);
            Assert.AreEqual(nameof(UserModel.Username), exception.Errors.ToList()[1].Name);
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectReasonError()
        {
            _exceptionContext.Exception = new UnauthorizedAccessException("Username or Password is invalid");

            var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

            Assert.AreEqual("Username or Password is invalid", exception.Errors.First().Reason);
            Assert.AreEqual("Username or Password is invalid", exception.Errors.ToList()[1].Reason);
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureCorrectStatusCode()
        {
            _exceptionContext.Exception = new UnauthorizedAccessException("Username or Password is invalid");

            var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

            Assert.AreEqual(400, exception.StatusCode);
        }

        [Test]
        public void OnException_ThrowApiException_If_ExceptionParameterName_IsNot_Null_EnsureIsValidationError()
        {
            _exceptionContext.Exception = new UnauthorizedAccessException("Username or Password is invalid");

            var exception = Assert.Throws<ApiException>(() => _systemUnderTest.OnException(_exceptionContext));

            Assert.AreEqual(true, exception.IsModelValidatonError);
        }
    }
}