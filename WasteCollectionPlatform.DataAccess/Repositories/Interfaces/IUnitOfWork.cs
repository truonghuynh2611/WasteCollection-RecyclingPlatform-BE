namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern interface for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ICitizenRepository Citizens { get; }
    ICollectorRepository Collectors { get; }
    IEnterpriseRepository Enterprises { get; }
    IDistrictRepository Districts { get; }
    ITeamRepository Teams { get; }
    IWasteReportRepository WasteReports { get; }
    IVoucherRepository Vouchers { get; }
    IPointHistoryRepository PointHistories { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
