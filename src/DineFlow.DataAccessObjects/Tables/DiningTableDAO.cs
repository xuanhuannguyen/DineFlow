using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Tables
{
    public class DiningTableDAO
    {
        private readonly AppDbContext _context;

        public DiningTableDAO(AppDbContext context)
        {
            _context = context;
        }

        public List<DiningTable> GetAll()
        {
            return _context.DiningTables
                           .Include(t => t.Area)
                           .OrderBy(t => t.AreaId)
                           .ThenBy(t => t.TableName)
                           .ToList();
        }

        public List<DiningTable> GetActive()
        {
            return _context.DiningTables
                           .Include(t => t.Area)
                           .Where(t => t.IsActive)
                           .OrderBy(t => t.AreaId)
                           .ThenBy(t => t.TableName)
                           .ToList();
        }

        public DiningTable? GetById(int tableId)
        {
            return _context.DiningTables
                           .Include(t => t.Area)
                           .FirstOrDefault(t => t.TableId == tableId);
        }

        public DiningTable? GetByQrToken(string qrToken)
        {
            return _context.DiningTables
                           .Include(t => t.Area)
                           .FirstOrDefault(t => t.QrToken == qrToken);
        }

        public List<DiningTable> GetByAreaId(int areaId)
        {
            return _context.DiningTables
                           .Include(t => t.Area)
                           .Where(t => t.AreaId == areaId)
                           .OrderBy(t => t.TableName)
                           .ToList();
        }

        public List<DiningTable> GetByStatus(string status)
        {
            return _context.DiningTables
                           .Include(t => t.Area)
                           .Where(t => t.IsActive && t.Status == status)
                           .OrderBy(t => t.AreaId)
                           .ThenBy(t => t.TableName)
                           .ToList();
        }

        public List<DiningTable> Search(string? keyword, int? areaId, string? status, bool? isActive)
        {
            var query = _context.DiningTables
                                .Include(t => t.Area)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(t => t.TableName.Contains(keyword));

            if (areaId.HasValue)
                query = query.Where(t => t.AreaId == areaId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            return query.OrderBy(t => t.AreaId)
                        .ThenBy(t => t.TableName)
                        .ToList();
        }

        public DiningTable Add(DiningTable table)
        {
            _context.DiningTables.Add(table);
            _context.SaveChanges();

            // Reload with navigation
            _context.Entry(table).Reference(t => t.Area).Load();
            return table;
        }

        public void Update(DiningTable table)
        {
            _context.DiningTables.Update(table);
            _context.SaveChanges();
        }

        public void SetActive(int tableId, bool isActive)
        {
            var table = _context.DiningTables.Find(tableId);
            if (table == null) return;

            table.IsActive  = isActive;
            table.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void UpdateStatus(int tableId, string status)
        {
            var table = _context.DiningTables.Find(tableId);
            if (table == null) return;

            table.Status    = status;
            table.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void UpdateQrToken(int tableId, string newQrToken)
        {
            var table = _context.DiningTables.Find(tableId);
            if (table == null) return;

            table.QrToken   = newQrToken;
            table.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public bool IsQrTokenExists(string qrToken)
        {
            return _context.DiningTables.Any(t => t.QrToken == qrToken);
        }

        public bool IsTableNameExistsInArea(string tableName, int areaId, int? excludeTableId = null)
        {
            return _context.DiningTables
                           .Any(t => t.TableName == tableName
                                  && t.AreaId == areaId
                                  && (excludeTableId == null || t.TableId != excludeTableId));
        }
    }
}
