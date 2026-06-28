using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Tables
{
    public class AreaDAO
    {
        private readonly AppDbContext _context;

        public AreaDAO(AppDbContext context)
        {
            _context = context;
        }

        public List<Area> GetAll()
        {
            return _context.Areas
                           .OrderBy(a => a.AreaName)
                           .ToList();
        }

        public List<Area> GetActive()
        {
            return _context.Areas
                           .Where(a => a.IsActive)
                           .OrderBy(a => a.AreaName)
                           .ToList();
        }

        public Area? GetById(int areaId)
        {
            return _context.Areas
                           .FirstOrDefault(a => a.AreaId == areaId);
        }

        public Area? GetByName(string areaName)
        {
            return _context.Areas
                           .FirstOrDefault(a => a.AreaName == areaName);
        }

        public Area Add(Area area)
        {
            _context.Areas.Add(area);
            _context.SaveChanges();
            return area;
        }

        public void Update(Area area)
        {
            _context.Areas.Update(area);
            _context.SaveChanges();
        }

        public void SetActive(int areaId, bool isActive)
        {
            var area = _context.Areas.Find(areaId);
            if (area == null) return;

            area.IsActive  = isActive;
            area.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        /// <summary>
        /// Kiểm tra khu vực còn bàn đang Active hay không (dùng để chặn deactivate).
        /// </summary>
        public bool HasActiveTables(int areaId)
        {
            return _context.DiningTables
                           .Any(t => t.AreaId == areaId && t.IsActive);
        }

        /// <summary>
        /// Kiểm tra khu vực có bàn đang phục vụ (Occupied/WaitingPayment).
        /// </summary>
        public bool HasOccupiedTables(int areaId)
        {
            return _context.DiningTables
                           .Any(t => t.AreaId == areaId
                                  && (t.Status == "Occupied" || t.Status == "WaitingPayment"));
        }

        public bool IsNameExists(string areaName, int? excludeAreaId = null)
        {
            return _context.Areas
                           .Any(a => a.AreaName == areaName
                                  && (excludeAreaId == null || a.AreaId != excludeAreaId));
        }

        public int CountTablesInArea(int areaId)
        {
            return _context.DiningTables.Count(t => t.AreaId == areaId);
        }
    }
}
