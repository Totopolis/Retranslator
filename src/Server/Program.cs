using Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services
    .AddSerilog((IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
    {
        loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Console()
            // TODO: setup logs path into /var/logs/ on *nix
            .WriteTo.File("retranslator-log.txt");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
