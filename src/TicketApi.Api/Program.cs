using FluentValidation;
using MediatR;
using TicketApi.Application.Common.Behaviors;
using TicketApi.Domain.Repositories;
using TicketApi.Infrastructure.Repositories;
using TicketApi.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TicketApi.Application.Tickets.Commands.CriarTicket.CriarTicketCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(TicketApi.Application.Tickets.Commands.CriarTicket.CriarTicketCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();