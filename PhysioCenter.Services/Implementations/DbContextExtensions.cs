using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PhysioCenter.Services.Data;
using PhysioCenter.Services.Exceptions;

namespace PhysioCenter.Services.Implementations;

internal static class DbContextExtensions
{
    /// <summary>
    /// Saves changes and turns the database constraint errors that can still happen
    /// (for example two requests racing for the same e-mail) into service exceptions.
    /// </summary>
    public static async Task SaveOrTranslateAsync(this PhysioDbContext db, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("A record with the same unique value already exists.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new ConflictException("The change conflicts with related records or a database rule.");
        }
    }
}
