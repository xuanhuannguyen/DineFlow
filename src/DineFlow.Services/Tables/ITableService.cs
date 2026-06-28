using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables
{
    public interface ITableService
    {
        List<DiningTableDto> GetAllTables(string? keyword = null, int? areaId = null,
                                          string? status = null, bool? isActive = null);
        List<DiningTableDto> GetActiveTables(int? areaId = null, string? status = null);
        DiningTableDto GetTableById(int tableId);
        DiningTableDto CreateTable(CreateTableRequest request);
        DiningTableDto UpdateTable(int tableId, UpdateTableRequest request);
        void DeactivateTable(int tableId);
        void ReactivateTable(int tableId);

        /// <summary>
        /// Internal contract: kiểm tra bàn active và có thể phục vụ.
        /// Ném exception nếu bàn không tồn tại hoặc inactive.
        /// </summary>
        void EnsureTableCanServe(int tableId);

        /// <summary>
        /// Internal contract: cập nhật status bàn bởi module nghiệp vụ (Member 4/5).
        /// </summary>
        void UpdateTableStatus(int tableId, string status, string reason);
    }
}
