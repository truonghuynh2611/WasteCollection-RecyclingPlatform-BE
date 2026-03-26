namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Unit of Work pattern interface for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICitizenRepository Citizens { get; }
    ICollectorRepository Collectors { get; }
    IDistrictRepository Districts { get; }
    ITeamRepository Teams { get; }
    IWasteReportRepository WasteReports { get; }
    IVoucherRepository Vouchers { get; }
    IPointHistoryRepository PointHistories { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IPendingRegistrationRepository PendingRegistrations { get; }
    IAreaRepository Areas { get; }
    ISystemConfigurationRepository SystemConfigurations { get; }
    IWasteReportItemRepository WasteReportItems { get; }
    IReportImageRepository ReportImages { get; }
    IReportAssignmentRepository ReportAssignments { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
