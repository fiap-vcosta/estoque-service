using Api.Contracts.Validation;

namespace Api.Controllers.Staff.ItemEstoque.CreateItemEstoque;

public class CreateItemEstoqueRequestValidator : IValidator<CreateItemEstoqueRequest>
{
    public ValidationResult Validate(CreateItemEstoqueRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            result.Errors.Add("Código não pode estar vazio.");
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            result.Errors.Add("Nome não pode estar vazio.");
        }

        if (request.PrecoVenda <= 0)
        {
            result.Errors.Add("Preço de venda deve ser maior que zero.");
        }

        if (request.Saldo < 0)
        {
            result.Errors.Add("Saldo não pode ser negativo.");
        }

        if (request.SaldoReservado < 0)
        {
            result.Errors.Add("Saldo reservado não pode ser negativo.");
        }

        if (request.SaldoReservado > request.Saldo)
        {
            result.Errors.Add("Saldo reservado não pode ser maior que o saldo.");
        }

        return result;
    }
}
