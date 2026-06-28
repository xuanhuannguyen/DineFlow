using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables
{
    public interface ITableReadService
    {
        List<TableStatusSummaryDto> GetTableStatusOverview(int? areaId = null,
                                                           string? status = null,
                                                           string? keyword = null);
        List<DiningTableDto> GetTablesByArea(int areaId);
        List<DiningTableDto> GetTablesByStatus(string status);
        List<DiningTableDto> SearchTables(string keyword);
    }
}
