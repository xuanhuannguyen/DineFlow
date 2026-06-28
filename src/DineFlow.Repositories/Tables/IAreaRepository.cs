using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Repositories.Tables
{
    public interface IAreaRepository
    {
        List<Area> GetAll();
        List<Area> GetActive();
        Area? GetById(int areaId);
        Area? GetByName(string areaName);
        Area Add(Area area);
        void Update(Area area);
        void SetActive(int areaId, bool isActive);
        bool HasActiveTables(int areaId);
        bool HasOccupiedTables(int areaId);
        bool IsNameExists(string areaName, int? excludeAreaId = null);
        int CountTablesInArea(int areaId);
    }
}
