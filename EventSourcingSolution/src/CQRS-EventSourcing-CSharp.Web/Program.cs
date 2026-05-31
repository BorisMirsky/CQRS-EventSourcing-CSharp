using CQRS_EventSourcing_CSharp.Application.Abstractions;
using CQRS_EventSourcing_CSharp.Application.CommandHandlers;
using CQRS_EventSourcing_CSharp.Application.Common;
using CQRS_EventSourcing_CSharp.Application.QueryHandlers;
using CQRS_EventSourcing_CSharp.DataAccess.EventStore;
using CQRS_EventSourcing_CSharp.DataAccess.ReadModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<WithdrawMoneyHandler>();
builder.Services.AddScoped<FreezeAccountHandler>();
builder.Services.AddScoped<UnfreezeAccountHandler>();
builder.Services.AddSingleton<IEventStore>(sp => new SqliteEventStore(connection));
builder.Services.AddScoped<OpenAccountHandler>();
builder.Services.AddScoped<DepositMoneyHandler>();

builder.Services.AddScoped<GetBalanceHandler>();
builder.Services.AddScoped<GetTransactionHistoryHandler>();
builder.Services.AddScoped<GetBalanceOnDateHandler>();
builder.Services.AddSingleton<IReadModelRepository>(sp => new SqliteReadModelRepository(connection));



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseCors(x =>
{
    x.WithHeaders().AllowAnyHeader();
    x.WithOrigins("http://localhost:5108");
    x.WithMethods().AllowAnyMethod();
});

app.UseCors();
app.MapControllers();
app.Run();