using FluentValidation;

namespace TicketApi.Application.Tickets.Commands.AtualizarTicket;

public class AtualizarTicketCommandValidator : AbstractValidator<AtualizarTicketCommand>
{
    public AtualizarTicketCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("O identificador do ticket é obrigatório.");

        RuleFor(x => x.Descricao)
            .MaximumLength(2000).WithMessage("A descrição deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("O status informado é inválido.");
    }
}
