using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketApi.Api.Contracts.Tickets;
using TicketApi.Application.Tickets.Commands.AtualizarTicket;
using TicketApi.Application.Tickets.Commands.CriarTicket;
using TicketApi.Application.Tickets.Commands.DeletarTicket;
using TicketApi.Application.Tickets.Queries;
using TicketApi.Application.Tickets.Queries.ListarTickets;
using TicketApi.Application.Tickets.Queries.ObterTicketPorId;
using TicketApi.Domain.Enums;

namespace TicketApi.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TicketCriadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarTicketRequest request)
    {
        var command = new CriarTicketCommand(
            request.Titulo,
            request.Descricao,
            request.EmpresaId,
            request.UsuarioId);

        var ticketId = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = ticketId },
            new TicketCriadoResponse(ticketId));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] StatusTicket? status,
        [FromQuery] Guid? empresaId)
    {
        var tickets = await _mediator.Send(new ListarTicketsQuery(status, empresaId));
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var ticket = await _mediator.Send(new ObterTicketPorIdQuery(id));
        return Ok(ticket);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTicketRequest request)
    {
        var command = new AtualizarTicketCommand(
            id,
            request.Descricao,
            request.Status);

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _mediator.Send(new DeletarTicketCommand(id));
        return NoContent();
    }
}
