using FIAP.TechChallenge.ByteMeBurger.Domain.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FIAP.TechChallenge.ByteMeBurger.Api;

/// <summary>
/// Filter class to handle domain exceptions.
/// </summary>
public class DomainExceptionFilter : IExceptionFilter
{
    private readonly ILogger<DomainExceptionFilter> _logger;

    public DomainExceptionFilter(ILogger<DomainExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DomainException) return;
        _logger.LogError(context.Exception, "An unhandled domain exception has occurred.");

        context.Result = new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "The request could not be processed.",
            Detail = context.Exception.Message,
        });
    }
}