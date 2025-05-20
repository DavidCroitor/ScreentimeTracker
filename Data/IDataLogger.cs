using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScreentimeTracker.Data.Models;

namespace ScreentimeTracker.Data;

public interface IDataLogger : IDisposable
{
        /// <summary>
        /// Asynchronously logs a completed application usage session.
        /// Useful if logging involves I/O that could block the main tracking loop.
        /// </summary>
        /// <param name="sessionEntry">The details of the application usage session to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogSessionAsync(AppUsageEntry sessionEntry);

        /// <summary>
        /// Ensures that the underlying data store (e.g., database, file) is initialized.
        /// This might be called once at application startup.
        /// </summary>
        void EnsureInitialized();

        /// <summary>
        /// Retrieves aggregated application usage statistics for a given date range.
        /// </summary>
        /// <param name="startDateUtc">The start date (UTC) of the period to query.</param>
        /// <param name="endDateUtc">The end date (UTC) of the period to query.</param>
        /// <returns>A collection of statistics, perhaps grouped by application name.</returns>
        // IEnumerable<AppUsageStat> GetUsageStats(DateTime startDateUtc, DateTime endDateUtc);
        Task<IEnumerable<AppUsageStat>> GetUsageStatsAsync(DateTime startDateUtc, DateTime endDateUtc);

}