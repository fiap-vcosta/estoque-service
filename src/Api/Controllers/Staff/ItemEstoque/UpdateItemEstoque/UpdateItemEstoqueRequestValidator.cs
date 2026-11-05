using Api.Contracts.Validation;

namespace Api.Controllers.Staff.ItemEstoque.UpdateItemEstoque;

public class UpdateItemEstoqueRequestValidator : IValidator<UpdateItemEstoqueRequest>
{
    public ValidationResult Validate(UpdateItemEstoqueRequest request)
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

        return result;
    }
}
