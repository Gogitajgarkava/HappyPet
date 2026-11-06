using FluentValidation;
using HappyPet.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HappyPet.WebAPI.Filters
{
    /// <summary>
    /// Runs the FluentValidation validator registered for each action argument.
    /// Validation failures use the same error envelope as model-binding failures.
    /// </summary>
    public sealed class FluentValidationActionFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public FluentValidationActionFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null)
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                {
                    continue;
                }

                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(
                    validationContext,
                    context.HttpContext.RequestAborted);

                if (validationResult.IsValid)
                {
                    continue;
                }

                var errors = validationResult.Errors
                    .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName)
                        ? "request"
                        : error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

                context.Result = new BadRequestObjectResult(new ApiErrorResponse
                {
                    Code = "VALIDATION_ERROR",
                    Message = "მოთხოვნის მონაცემები არასწორია.",
                    TraceId = context.HttpContext.TraceIdentifier,
                    Errors = errors
                });

                return;
            }

            await next();
        }
    }
}
