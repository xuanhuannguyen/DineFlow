using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.Tables;

namespace DineFlow.Repositories.Tables
{
    public class DiningTableRepository : IDiningTableRepository
    {
        private readonly DiningTableDAO _tableDAO;

        public DiningTableRepository(DiningTableDAO tableDAO)
        {
            _tableDAO = tableDAO;
        }

        public List<DiningTable> GetAll()            => _tableDAO.GetAll();
        public List<DiningTable> GetActive()         => _tableDAO.GetActive();
        public DiningTable? GetById(int tableId)     => _tableDAO.GetById(tableId);
        public DiningTable? GetByQrToken(string qrToken) => _tableDAO.GetByQrToken(qrToken);
        public List<DiningTable> GetByAreaId(int areaId) => _tableDAO.GetByAreaId(areaId);
        public List<DiningTable> GetByStatus(string status) => _tableDAO.GetByStatus(status);

        public List<DiningTable> Search(string? keyword, int? areaId, string? status, bool? isActive)
            => _tableDAO.Search(keyword, areaId, status, isActive);

        public DiningTable Add(DiningTable table)    => _tableDAO.Add(table);
        public void Update(DiningTable table)        => _tableDAO.Update(table);
        public void SetActive(int tableId, bool isActive) => _tableDAO.SetActive(tableId, isActive);
        public void UpdateStatus(int tableId, string status) => _tableDAO.UpdateStatus(tableId, status);
        public void UpdateQrToken(int tableId, string newQrToken) => _tableDAO.UpdateQrToken(tableId, newQrToken);
        public bool IsQrTokenExists(string qrToken)  => _tableDAO.IsQrTokenExists(qrToken);

        public bool IsTableNameExistsInArea(string tableName, int areaId, int? excludeTableId = null)
            => _tableDAO.IsTableNameExistsInArea(tableName, areaId, excludeTableId);
    }
}
