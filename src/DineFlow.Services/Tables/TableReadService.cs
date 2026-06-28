using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;

namespace DineFlow.Services.Tables
{
    public class TableReadService : ITableReadService
    {
        private readonly IDiningTableRepository _tableRepo;
        private readonly ITableQrService        _qrService;

        public TableReadService(IDiningTableRepository tableRepo, ITableQrService qrService)
        {
            _tableRepo = tableRepo;
            _qrService = qrService;
        }

        public List<TableStatusSummaryDto> GetTableStatusOverview(int? areaId = null,
                                                                   string? status = null,
                                                                   string? keyword = null)
        {
            var tables = _tableRepo.Search(keyword, areaId, status, isActive: true);

            return tables.Select(t => new TableStatusSummaryDto
            {
                TableId          = t.TableId,
                TableName        = t.TableName,
                AreaName         = t.Area?.AreaName,
                Status           = t.Status,
                IsActive         = t.IsActive,
                // Member 4/5 fields — null/0 until integration
                CurrentSessionId = null,
                StartedAt        = null,
                OrderCount       = 0,
                BillCount        = 0,
                UnpaidAmount     = 0
            }).ToList();
        }

        public List<DiningTableDto> GetTablesByArea(int areaId)
        {
            return _tableRepo.GetByAreaId(areaId)
                             .Select(MapToDto)
                             .ToList();
        }

        public List<DiningTableDto> GetTablesByStatus(string status)
        {
            return _tableRepo.GetByStatus(status)
                             .Select(MapToDto)
                             .ToList();
        }

        public List<DiningTableDto> SearchTables(string keyword)
        {
            return _tableRepo.Search(keyword, null, null, isActive: true)
                             .Select(MapToDto)
                             .ToList();
        }

        private DiningTableDto MapToDto(DiningTable t) => new()
        {
            TableId   = t.TableId,
            TableName = t.TableName,
            AreaId    = t.AreaId,
            AreaName  = t.Area?.AreaName ?? string.Empty,
            QrToken   = t.QrToken,
            QrUrl     = _qrService.GenerateQrUrl(t.QrToken),
            Status    = t.Status,
            IsActive  = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}
