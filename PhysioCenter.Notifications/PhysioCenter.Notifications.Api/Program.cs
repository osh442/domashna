using PhysioCenter.Notifications.Api;
using PhysioCenter.Notifications.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Notifications")
    ?? throw new InvalidOperationException("Connection string 'Notifications' is missing in appsettings.json.");

builder.Services.AddNotificationsDatabase(connectionString);
builder.Services.AddHostedService<SessionEventsConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Physio Notifications API", Version = "v1",
        Description = "Notifications created from session events received through Kafka." });
    foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "PhysioCenter.Notifications.*.xml"))
        options.IncludeXmlComments(xml);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
