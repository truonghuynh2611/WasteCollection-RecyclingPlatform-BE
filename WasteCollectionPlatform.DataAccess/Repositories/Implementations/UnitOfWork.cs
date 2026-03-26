using Microsoft.EntityFrameworkCore.Storage;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Unit of Work pattern implementation for transaction management
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly WasteManagementContext _context;
    private IDbContextTransaction? _transaction;
    
    // Lazy initialization of repositories
    private IUserRepository? _users;
    private ICitizenRepository? _citizens;
    private ICollectorRepository? _collectors;
    private IDistrictRepository? _districts;
    private ITeamRepository? _teams;
    private IWasteReportRepository? _wasteReports;
    private IVoucherRepository? _vouchers;
    private IPointHistoryRepository? _pointHistories;
    private IRefreshTokenRepository? _refreshTokens;
    private IPendingRegistrationRepository? _pendingRegistrations;
    private IAreaRepository? _areas;
    private ISystemConfigurationRepository? _systemConfigurations;
    private IWasteReportItemRepository? _wasteReportItems;
    private IReportImageRepository? _reportImages;
    private IReportAssignmentRepository? _reportAssignments;

    public UnitOfWork(WasteManagementContext context)
    {
        _context = context;
    }
    
    public IUserRepository Users => 
        _users ??= new UserRepository(_context);
    
    public ICitizenRepository Citizens => 
        _citizens ??= new CitizenRepository(_context);
    
    public ICollectorRepository Collectors => 
        _collectors ??= new CollectorRepository(_context);
    
    
    public IDistrictRepository Districts => 
        _districts ??= new DistrictRepository(_context);
    
    public ITeamRepository Teams => 
        _teams ??= new TeamRepository(_context);
    
    public IWasteReportRepository WasteReports => 
        _wasteReports ??= new WasteReportRepository(_context);
    
    public IVoucherRepository Vouchers => 
        _vouchers ??= new VoucherRepository(_context);
    
    public IPointHistoryRepository PointHistories => 
        _pointHistories ??= new PointHistoryRepository(_context);
    
    public IRefreshTokenRepository RefreshTokens => 
        _refreshTokens ??= new RefreshTokenRepository(_context);

    public IPendingRegistrationRepository PendingRegistrations => 
        _pendingRegistrations ??= new PendingRegistrationRepository(_context);
    
    public IAreaRepository Areas =>
        _areas ??= new AreaRepository(_context);

    public ISystemConfigurationRepository SystemConfigurations =>
        _systemConfigurations ??= new SystemConfigurationRepository(_context);

    public IWasteReportItemRepository WasteReportItems =>
        _wasteReportItems ??= new WasteReportItemRepository(_context);

    public IReportImageRepository ReportImages =>
        _reportImages ??= new ReportImageRepository(_context);

    public IReportAssignmentRepository ReportAssignments =>
        _reportAssignments ??= new ReportAssignmentRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction;
    }
    
    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
