using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Persistence;

public class OrderDbContextInitializer
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderDbContextInitializer> _logger;
    private readonly IConfiguration _configuration;

    public OrderDbContextInitializer(
        OrderDbContext context,
        ILogger<OrderDbContextInitializer> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Check if auto-migration is disabled
            var autoMigrate = _configuration.GetValue<bool>("Database:AutoMigrate", true);
            
            if (!autoMigrate)
            {
                _logger.LogInformation("Auto-migration is disabled. Skipping database initialization.");
                return;
            }

            // Check if we're using SQL Server (not InMemory)
            if (_context.Database.IsSqlServer())
            {
                _logger.LogInformation("Applying database migrations...");
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied successfully");
            }
            else if (_context.Database.IsInMemory())
            {
                _logger.LogInformation("Using InMemory database, ensuring created...");
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("InMemory database created successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // Seed işlemi gerekirse buraya eklenebilir
        // Örneğin: test siparişleri, varsayılan veriler vb.
        
        _logger.LogInformation("Database seeding completed (no seed data configured)");
        await Task.CompletedTask;
    }
}

