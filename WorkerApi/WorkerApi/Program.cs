using WorkerApi.Services;
using WorkerApi.Services.Process;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Création du dossier logs s'il n'existe pas
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsPath))
{
    try
    {
        Directory.CreateDirectory(logsPath);
        Console.WriteLine($"Dossier logs créé avec succès : {logsPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur lors de la création du dossier logs : {ex.Message}");
    }
}

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("/app/logs/ffmpeg-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Ajout de Serilog au conteneur de services
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

// Configure logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.AddEventSourceLogger();

// Add services to the container.
builder.Services.AddTransient<IFilterGraphService, FilterGraphService>();
builder.Services.AddTransient<ICommandBuildService, CommandBuildService>();
builder.Services.AddTransient<IFilterComplexBuilder, FilterComplexBuilder>();
builder.Services.AddTransient<FfmpegRunnerService>();
builder.Services.AddTransient<IProcessFactory, ProcessFactory>();
builder.Services.AddTransient<IProcessWrapper, ProcessWrapper>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
