using CrisisPlatform.Api.Common.Security;
using CrisisPlatform.Api.Common.Security.Logging;
using CrisisPlatform.Api.Modules.Requests;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddRedaction();

// ── Core MVC / routing ──────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── OpenAPI / Swagger ───────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Cross-cutting middleware ─────────────────────────────────────────────────
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddAuthRateLimiting(builder.Configuration);

// ── Module services ──────────────────────────────────────────────────────────
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<RequestService>();

var app = builder.Build();

// ── HTTP pipeline ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.PolicyName);
app.UseRateLimiter();

app.UseRouting();
app.MapControllers();

app.Run();
