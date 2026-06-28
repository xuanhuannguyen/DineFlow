using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;

namespace DineFlow.Services.Tables
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _areaRepo;

        public AreaService(IAreaRepository areaRepo)
        {
            _areaRepo = areaRepo;
        }

        public List<AreaDto> GetAllAreas()
        {
            return _areaRepo.GetAll()
                            .Select(MapToDto)
                            .ToList();
        }

        public List<AreaDto> GetActiveAreas()
        {
            return _areaRepo.GetActive()
                            .Select(MapToDto)
                            .ToList();
        }

        public AreaDto GetAreaById(int areaId)
        {
            var area = _areaRepo.GetById(areaId)
                ?? throw new Exception("AREA_NOT_FOUND: Không tìm thấy khu vực.");
            return MapToDto(area);
        }

        public AreaDto CreateArea(CreateAreaRequest request)
        {
            // Validate
            var name = request.AreaName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
                throw new Exception("AREA_NAME_REQUIRED: Tên khu vực không được để trống.");
            if (name.Length > 50)
                throw new Exception("AREA_NAME_TOO_LONG: Tên khu vực không được vượt quá 50 ký tự.");
            if (_areaRepo.IsNameExists(name))
                throw new Exception("AREA_NAME_DUPLICATED: Tên khu vực đã tồn tại.");

            var desc = request.Description?.Trim();
            if (desc?.Length > 200)
                throw new Exception("Mô tả không được vượt quá 200 ký tự.");

            var area = new Area
            {
                AreaName    = name,
                Description = desc,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow
            };

            var created = _areaRepo.Add(area);
            return MapToDto(created);
        }

        public AreaDto UpdateArea(int areaId, UpdateAreaRequest request)
        {
            var area = _areaRepo.GetById(areaId)
                ?? throw new Exception("AREA_NOT_FOUND: Không tìm thấy khu vực.");

            var name = request.AreaName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
                throw new Exception("AREA_NAME_REQUIRED: Tên khu vực không được để trống.");
            if (name.Length > 50)
                throw new Exception("AREA_NAME_TOO_LONG: Tên khu vực không được vượt quá 50 ký tự.");
            if (_areaRepo.IsNameExists(name, excludeAreaId: areaId))
                throw new Exception("AREA_NAME_DUPLICATED: Tên khu vực đã tồn tại.");

            var desc = request.Description?.Trim();
            if (desc?.Length > 200)
                throw new Exception("Mô tả không được vượt quá 200 ký tự.");

            area.AreaName    = name;
            area.Description = desc;
            area.UpdatedAt   = DateTime.UtcNow;

            _areaRepo.Update(area);
            return MapToDto(area);
        }

        public void DeactivateArea(int areaId)
        {
            var area = _areaRepo.GetById(areaId)
                ?? throw new Exception("AREA_NOT_FOUND: Không tìm thấy khu vực.");

            if (!area.IsActive)
                return; // Đã inactive rồi — không làm gì

            // Chặn nếu có bàn đang phục vụ
            if (_areaRepo.HasOccupiedTables(areaId))
                throw new Exception("AREA_HAS_TABLES: Khu vực đang có bàn hoạt động (Occupied/WaitingPayment). Hãy xử lý bàn trước.");

            _areaRepo.SetActive(areaId, false);
        }

        public void ReactivateArea(int areaId)
        {
            var area = _areaRepo.GetById(areaId)
                ?? throw new Exception("AREA_NOT_FOUND: Không tìm thấy khu vực.");

            if (area.IsActive) return;

            _areaRepo.SetActive(areaId, true);
        }

        // ── Mapper ────────────────────────────────────────────────────────────
        private AreaDto MapToDto(Area area) => new()
        {
            AreaId      = area.AreaId,
            AreaName    = area.AreaName,
            Description = area.Description,
            IsActive    = area.IsActive,
            TableCount  = _areaRepo.CountTablesInArea(area.AreaId)
        };
    }
}
