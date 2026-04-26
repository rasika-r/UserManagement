using UserManagementAPI.Repository;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "API for managing user records for TechHive Solutions",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "TechHive Solutions",
            Email = "support@techhive.com"
        }
    });
});

// Register repository with dependency injection
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
// MIDDLEWARE ORDER IS CRITICAL:
// 1. Error-handling middleware first (catches all exceptions)
// 2. Authentication middleware next (validates tokens)
// 3. Logging middleware last (logs all requests/responses)

// Apply error-handling middleware first
app.UseMiddleware<ErrorHandlingMiddleware>();

// Apply authentication middleware
app.UseMiddleware<AuthenticationMiddleware>();

// Apply logging middleware last
app.UseMiddleware<LoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
