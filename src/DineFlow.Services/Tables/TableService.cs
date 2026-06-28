using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;

namespace DineFlow.Services.Tables
{
    public class TableService : ITableService
    {
        private readonly IDiningTableRepository _tableRepo;
        private readonly IAreaRepository        _areaRepo;
        private readonly ITableQrService        _qrService;

        public TableService(IDiningTableRepository tableRepo,
                            IAreaRepository areaRepo,
                            ITableQrService qrService)
        {
            _tableRepo  = tableRepo;
            _areaRepo   = areaRepo;
            _qrService  = qrService;
        }

        public List<DiningTableDto> GetAllTables(string? keyword = null, int? areaId = null,
                                                  string? status = null, bool? isActive = null)
        {
            return _tableRepo.Search(keyword, areaId, status, isActive)
                             .Select(t => MapToDto(t))
                             .ToList();
        }

        public List<DiningTableDto> GetActiveTables(int? areaId = null, string? status = null)
        {
            return _tableRepo.Search(null, areaId, status, isActive: true)
                             .Select(t => MapToDto(t))
                             .ToList();
        }

        public DiningTableDto GetTableById(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");
            return MapToDto(table);
        }

        public DiningTableDto CreateTable(CreateTableRequest request)
        {
            // BR-TABLE-002: Validate TableName
            var name = request.TableName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
                throw new Exception("TABLE_NAME_REQUIRED: Tên bàn không được để trống.");
            if (name.Length > 50)
                throw new Exception("TABLE_NAME_TOO_LONG: Tên bàn không được vượt quá 50 ký tự.");

            // BR-TABLE-003: Validate AreaId
            var area = _areaRepo.GetById(request.AreaId)
                ?? throw new Exception("AREA_NOT_FOUND: Không tìm thấy khu vực.");
            if (!area.IsActive)
                throw new Exception("AREA_INACTIVE: Khu vực đã ngưng hoạt động.");

            // Unique: AreaId + TableName
            if (_tableRepo.IsTableNameExistsInArea(name, request.AreaId))
                throw new Exception("TABLE_DUPLICATED: Bàn đã tồn tại trong khu vực này.");

            // BR-TABLE-001: Auto-generate QrToken
            var qrToken = _qrService.GenerateQrToken();
            var qrUrl   = _qrService.GenerateQrUrl(qrToken);

            var table = new DiningTable
            {
                TableName = name,
                AreaId    = request.AreaId,
                QrToken   = qrToken,
                Status    = TableStatus.Available,
                IsActive  = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = _tableRepo.Add(table);
            return MapToDtoWithQrUrl(created, qrUrl);
        }

        public DiningTableDto UpdateTable(int tableId, UpdateTableRequest request)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            var name = request.TableName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
                throw new Exception("TABLE_NAME_REQUIRED: Tên bàn không được để trống.");
            if (name.Length > 50)
                throw new Exception("TABLE_NAME_TOO_LONG: Tên bàn không được vượt quá 50 ký tự.");

            var area = _areaRepo.GetById(request.AreaId)
                ?? throw new Exception("AREA_NOT_FOUND: Không tìm thấy khu vực.");
            if (!area.IsActive)
                throw new Exception("AREA_INACTIVE: Khu vực đã ngưng hoạt động.");

            if (_tableRepo.IsTableNameExistsInArea(name, request.AreaId, excludeTableId: tableId))
                throw new Exception("TABLE_DUPLICATED: Bàn đã tồn tại trong khu vực này.");

            table.TableName = name;
            table.AreaId    = request.AreaId;
            table.Area      = area;
            table.UpdatedAt = DateTime.UtcNow;

            _tableRepo.Update(table);
            return MapToDto(table);
        }

        public void DeactivateTable(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            if (!table.IsActive) return;

            // BR-TABLE-008: Không deactivate khi đang phục vụ
            if (table.Status == TableStatus.Occupied || table.Status == TableStatus.WaitingPayment)
                throw new Exception("TABLE_HAS_ACTIVE_SESSION: Không thể ẩn bàn đang có phiên phục vụ.");

            _tableRepo.SetActive(tableId, false);
        }

        public void ReactivateTable(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            if (table.IsActive) return;

            _tableRepo.SetActive(tableId, true);

            // BR-TABLE-009: Nếu không có session active thì reset về Available
            if (table.Status != TableStatus.Available)
                _tableRepo.UpdateStatus(tableId, TableStatus.Available);
        }

        public void EnsureTableCanServe(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            if (!table.IsActive)
                throw new Exception("TABLE_INACTIVE: Bàn hiện không còn hoạt động.");
        }

        public void UpdateTableStatus(int tableId, string status, string reason)
        {
            if (!TableStatus.IsValid(status))
                throw new Exception("INVALID_TABLE_STATUS: Trạng thái bàn không hợp lệ.");

            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            _tableRepo.UpdateStatus(tableId, status);
        }

        // ── Mappers ───────────────────────────────────────────────────────────
        private DiningTableDto MapToDto(DiningTable t) => MapToDtoWithQrUrl(t, _qrService.GenerateQrUrl(t.QrToken));

        private static DiningTableDto MapToDtoWithQrUrl(DiningTable t, string qrUrl) => new()
        {
            TableId   = t.TableId,
            TableName = t.TableName,
            AreaId    = t.AreaId,
            AreaName  = t.Area?.AreaName ?? string.Empty,
            QrToken   = t.QrToken,
            QrUrl     = qrUrl,
            Status    = t.Status,
            IsActive  = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}
