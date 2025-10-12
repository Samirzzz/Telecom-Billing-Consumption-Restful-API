var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World 👋 from Telecom Billing API");

app.Run();
