using NetMQ;
using QuickGraph.Algorithms.Services;
using WorkerApi.Services;
using WorkerApi.Services.Process;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Add services to the container.

builder.Services.AddMemoryCache();
builder.Services.AddTransient<ICachedScorerService, CachedScorerService>();
builder.Services.AddTransient<IFilterGraphService, FilterGraphService>();
builder.Services.AddTransient<ICommandBuildService, CommandBuildService>();
builder.Services.AddTransient<FfmpegRunnerService>();
builder.Services.AddTransient<IProcessFactory, ProcessFactory>();
builder.Services.AddTransient<IProcessWrapper, ProcessWrapper>();
builder.Services.AddTransient<IZmqCommandService, ZmqCommandService>();
builder.Services.AddTransient<IApiKeyValidatorService, ApiKeyValidatorService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();





var runtime = new NetMQRuntime();
runtime.Run();

builder.Services.AddHostedService<TimedScoreService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

app.Run();
