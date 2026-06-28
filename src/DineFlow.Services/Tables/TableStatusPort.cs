using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;

namespace DineFlow.Services.Tables
{
    /// <summary>
    /// Internal port — chỉ Member 4/5 được inject và gọi.
    /// KHÔNG đăng ký trong DI container của WPFApp.
    /// </summary>
    public class TableStatusPort : ITableStatusPort
    {
        private readonly IDiningTableRepository _tableRepo;
        private readonly ITableQrService        _qrService;

        public TableStatusPort(IDiningTableRepository tableRepo, ITableQrService qrService)
        {
            _tableRepo = tableRepo;
            _qrService = qrService;
        }

        public void SetTableOccupied(int tableId, int tableSessionId)
        {
            EnsureTableExists(tableId);
            _tableRepo.UpdateStatus(tableId, TableStatus.Occupied);
        }

        public void SetTableWaitingPayment(int tableId, int tableSessionId)
        {
            EnsureTableExists(tableId);
            _tableRepo.UpdateStatus(tableId, TableStatus.WaitingPayment);
        }

        public void SetTableAvailable(int tableId, int tableSessionId)
        {
            EnsureTableExists(tableId);
            _tableRepo.UpdateStatus(tableId, TableStatus.Available);
        }

        public DiningTableDto SyncTableStatus(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            return new DiningTableDto
            {
                TableId   = table.TableId,
                TableName = table.TableName,
                AreaId    = table.AreaId,
                AreaName  = table.Area?.AreaName ?? string.Empty,
                QrToken   = table.QrToken,
                QrUrl     = _qrService.GenerateQrUrl(table.QrToken),
                Status    = table.Status,
                IsActive  = table.IsActive,
                CreatedAt = table.CreatedAt,
                UpdatedAt = table.UpdatedAt
            };
        }

        private void EnsureTableExists(int tableId)
        {
            if (_tableRepo.GetById(tableId) == null)
                throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");
        }
    }
}
