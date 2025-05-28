using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;

namespace Webhooks.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebhookDbContext>();

            await dbContext.Database.MigrateAsync();
    }
}
