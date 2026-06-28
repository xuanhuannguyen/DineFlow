namespace DineFlow.BusinessObjects.Tables
{
    /// <summary>
    /// Request DTO for updating table name and area.
    /// QrToken, Status, IsActive are NOT included — they go through dedicated endpoints.
    /// </summary>
    public class UpdateTableRequest
    {
        public string TableName { get; set; } = null!;
        public int AreaId { get; set; }
    }
}
