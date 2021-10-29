using Alura.WebAPI.Api.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alura.WebAPI.WebApp.Filtros
{
    public class ErrorResponseFilter: IExceptionFilter
    {
        //quando aparecer uma exceção esse código será executado
        public void OnException(ExceptionContext context)
        {
            //pegando a execeção e embrulhando a ela dentro do objetO errorResponse
            var errorResponse = ErrorResponse.From(context.Exception);

            context.Result = new ObjectResult(errorResponse) { StatusCode = 500};
        }
    }
}
