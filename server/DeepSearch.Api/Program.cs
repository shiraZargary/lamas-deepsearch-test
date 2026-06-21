using DeepSearch.Api.Middleware;
using DeepSearch.Application;
using DeepSearch.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Cloud hosting: bind to the platform-provided PORT (Render/Cloud Run/etc.) ---
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// --- Logging (Serilog) ---
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
                .WriteTo.Console());

// --- CORS for the Angular client ---
const string ClientCorsPolicy = "AllowClient";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
    options.AddPolicy(ClientCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// --- MVC + Swagger ---
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Application & Infrastructure layers ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Behind a cloud reverse proxy (Render) TLS is already terminated, so only
    // redirect to HTTPS locally to avoid redirect loops / port issues.
    app.UseHttpsRedirection();
}

app.UseCors(ClientCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
