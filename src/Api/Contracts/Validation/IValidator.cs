namespace Api.Contracts.Validation;

public interface IValidator<in TRequest>
{
    ValidationResult Validate(TRequest request);
}
