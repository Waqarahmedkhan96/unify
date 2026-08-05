namespace Unify.Erp.Api.Common;

public static class ValidationProblemExtensions
{
    public static IResult ToProblem(this ValidationResult validationResult, HttpContext httpContext)
    {
        return Results.ValidationProblem(
            validationResult.Errors,
            title: "Validation failed.",
            type: "https://httpstatuses.com/400",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "common.validation_failed",
                ["correlationId"] = httpContext.TraceIdentifier
            });
    }
}
