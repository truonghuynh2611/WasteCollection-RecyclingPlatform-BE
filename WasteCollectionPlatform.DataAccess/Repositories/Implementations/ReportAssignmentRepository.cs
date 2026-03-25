using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations
{
    public class ReportAssignmentRepository : GenericRepository<ReportAssignment>, IReportAssignmentRepository
    {
        public ReportAssignmentRepository(WasteManagementContext context) : base(context)
        {
        }
    }
}
