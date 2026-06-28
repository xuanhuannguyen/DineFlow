namespace DineFlow.BusinessObjects.Tables
{
    public class DiningTable
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int AreaId { get; set; }
        public string QrToken { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Area Area { get; set; } = null!;
    }
}
