using FluentValidation;

namespace TicketApi.Application.Tickets.Commands.CriarTicket;

public class CriarTicketCommandValidator : AbstractValidator<CriarTicketCommand>
{
    public CriarTicketCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.EmpresaId)
            .NotEqual(Guid.Empty).WithMessage("A empresa é obrigatória.");

        RuleFor(x => x.UsuarioId)
            .NotEqual(Guid.Empty).WithMessage("O usuário responsável é obrigatório.");
    }
}