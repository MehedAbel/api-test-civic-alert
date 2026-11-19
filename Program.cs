using CivicAlert.Api.Data;
using CivicAlert.Api.Extensions;
using CivicAlert.Api.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCivicAlertServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Civic Alert API v1");
    options.RoutePrefix = "swagger";
});

if (!string.IsNullOrEmpty(app.Configuration["ASPNETCORE_HTTPS_PORT"]) ||
    !string.IsNullOrEmpty(app.Configuration["HTTPS_PORT"]))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseCors("CivicAlertCors");
app.UseAuthentication();
app.UseAuthorization();

// Controller-based REST API endpoints (/api/auth/*, /api/issues/*, /api/profile/*, /api/admin/*)
app.MapControllers();
app.MapHealthChecks("/health");

// Apply pending EF Core SQL Server migrations and seed initial roles/data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CivicAlertDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.Run();
