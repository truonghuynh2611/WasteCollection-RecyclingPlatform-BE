using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class WasteReportItemRepository : GenericRepository<WasteReportItem>, IWasteReportItemRepository
{
    public WasteReportItemRepository(WasteManagementContext context) : base(context)
    {
    }
}
