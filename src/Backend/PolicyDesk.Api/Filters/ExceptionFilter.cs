using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PolicyDesk.Api.Errors;
using PolicyDesk.Api.Logging;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Api.Filters;

public class ExceptionFilter(ProblemDetailsFactory problemDetailsFactory, ILogger<ExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ErrorOnValidationException errorOnValidationException:
                SetErrorResult(context, StatusCodes.Status400BadRequest, errorOnValidationException.GetErrors());
                break;
            case DomainRuleException domainRuleException:
                SetErrorResult(context, StatusCodes.Status400BadRequest, [domainRuleException.Message]);
                break;
            case NotFoundException notFoundException:
                SetErrorResult(context, StatusCodes.Status404NotFound, [notFoundException.Message]);
                break;
            case ConflictException conflictException:
                SetErrorResult(context, StatusCodes.Status409Conflict, [conflictException.Message]);
                break;
            case BadHttpRequestException badHttpRequestException:
                SetErrorResult(context, badHttpRequestException.StatusCode, [ResourcesMessageException.INVALID_REQUEST]);
                break;
            default:
                // Detalhes de erros inesperados ficam só no log, nunca na resposta
                logger.UnhandledException(context.Exception, context.HttpContext.Request.Method, context.HttpContext.Request.Path);
                SetErrorResult(context, StatusCodes.Status500InternalServerError, [ResourcesMessageException.UNKNOWN_ERROR]);
                break;
        }
    }

    private void SetErrorResult(ExceptionContext context, int statusCode, List<string> errors)
    {
        var problem = problemDetailsFactory.CreateProblemDetails(context.HttpContext, statusCode);
        problem.Extensions[ProblemDetailsSetup.ErrorsKey] = errors;

        context.Result = new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}
