using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.Tables;

namespace DineFlow.Repositories.Tables
{
    public class AreaRepository : IAreaRepository
    {
        private readonly AreaDAO _areaDAO;

        public AreaRepository(AreaDAO areaDAO)
        {
            _areaDAO = areaDAO;
        }

        public List<Area> GetAll()               => _areaDAO.GetAll();
        public List<Area> GetActive()            => _areaDAO.GetActive();
        public Area? GetById(int areaId)         => _areaDAO.GetById(areaId);
        public Area? GetByName(string areaName)  => _areaDAO.GetByName(areaName);
        public Area Add(Area area)               => _areaDAO.Add(area);
        public void Update(Area area)            => _areaDAO.Update(area);
        public void SetActive(int areaId, bool isActive) => _areaDAO.SetActive(areaId, isActive);
        public bool HasActiveTables(int areaId)  => _areaDAO.HasActiveTables(areaId);
        public bool HasOccupiedTables(int areaId) => _areaDAO.HasOccupiedTables(areaId);
        public bool IsNameExists(string areaName, int? excludeAreaId = null)
            => _areaDAO.IsNameExists(areaName, excludeAreaId);
        public int CountTablesInArea(int areaId) => _areaDAO.CountTablesInArea(areaId);
    }
}
