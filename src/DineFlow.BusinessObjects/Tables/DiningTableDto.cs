namespace DineFlow.BusinessObjects.Tables
{
    public class DiningTableDto
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int AreaId { get; set; }
        public string AreaName { get; set; } = null!;
        public string QrToken { get; set; } = null!;
        public string QrUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
