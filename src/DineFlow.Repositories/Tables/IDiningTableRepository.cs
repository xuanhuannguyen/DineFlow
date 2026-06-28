using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Repositories.Tables
{
    public interface IDiningTableRepository
    {
        List<DiningTable> GetAll();
        List<DiningTable> GetActive();
        DiningTable? GetById(int tableId);
        DiningTable? GetByQrToken(string qrToken);
        List<DiningTable> GetByAreaId(int areaId);
        List<DiningTable> GetByStatus(string status);
        List<DiningTable> Search(string? keyword, int? areaId, string? status, bool? isActive);
        DiningTable Add(DiningTable table);
        void Update(DiningTable table);
        void SetActive(int tableId, bool isActive);
        void UpdateStatus(int tableId, string status);
        void UpdateQrToken(int tableId, string newQrToken);
        bool IsQrTokenExists(string qrToken);
        bool IsTableNameExistsInArea(string tableName, int areaId, int? excludeTableId = null);
    }
}
