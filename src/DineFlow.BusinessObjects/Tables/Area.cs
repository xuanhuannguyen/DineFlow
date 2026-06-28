namespace DineFlow.BusinessObjects.Tables
{
    public class Area
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<DiningTable> Tables { get; set; } = new List<DiningTable>();
    }
}
