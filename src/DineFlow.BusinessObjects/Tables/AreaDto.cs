namespace DineFlow.BusinessObjects.Tables
{
    public class AreaDto
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TableCount { get; set; }
    }
}
