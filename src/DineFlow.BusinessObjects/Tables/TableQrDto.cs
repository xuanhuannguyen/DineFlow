namespace DineFlow.BusinessObjects.Tables
{
    public class TableQrDto
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public string QrToken { get; set; } = null!;
        public string QrUrl { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }
    }
}
