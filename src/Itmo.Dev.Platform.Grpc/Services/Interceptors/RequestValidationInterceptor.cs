using Grpc.Core;
using Grpc.Core.Interceptors;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Itmo.Dev.Platform.Grpc.Services.Interceptors;

internal sealed class RequestValidationInterceptor : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (request is IValidatableObject validatableObject)
        {
            Validate(validatableObject);
        }

        return continuation(request, context);
    }

    private static void Validate(IValidatableObject validatableObject)
    {
        var validationContext = new ValidationContext(validatableObject);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(validatableObject, validationContext, validationResults);

        if (isValid is false)
        {
            var errorBuilder = new StringBuilder("Ошибка валидации: ");

            foreach (ValidationResult validationResult in validationResults)
            {
                errorBuilder.Append('[');
                errorBuilder.AppendJoin(", ", validationResult.MemberNames);
                errorBuilder.Append("] = \"");
                errorBuilder.Append(validationResult.ErrorMessage);
                errorBuilder.Append("\"; ");
            }

            throw new RpcException(new Status(StatusCode.InvalidArgument, errorBuilder.ToString()));
        }
    }
}
