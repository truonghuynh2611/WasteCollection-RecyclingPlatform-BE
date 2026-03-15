using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class ReportImageRepository : GenericRepository<ReportImage>, IReportImageRepository
{
    public ReportImageRepository(WasteManagementContext context) : base(context)
    {
    }
}
