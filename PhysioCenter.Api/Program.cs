using PhysioCenter.Api;
using PhysioCenter.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PhysioCenter")
    ?? throw new InvalidOperationException("Connection string 'PhysioCenter' is missing in appsettings.json.");

// The service layer (PhysioCenter.Services) owns the DbContext and all business logic.
var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var sessionsTopic = builder.Configuration["Kafka:SessionsTopic"] ?? "physio.sessions";

builder.Services.AddPhysioCenterServices(connectionString, kafkaBootstrapServers, sessionsTopic);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Physio Center API", Version = "v1",
        Description = "Patients, therapists, therapy plans, sessions and progress notes." });
    // XML comments of the controllers and of the DTOs (PhysioCenter.Services) become Swagger descriptions.
    foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "PhysioCenter.*.xml"))
        options.IncludeXmlComments(xml);
});

var app = builder.Build();

app.UseExceptionHandler();

// Swagger UI stays on in every environment so the API can be tried right after "dotnet run".
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
