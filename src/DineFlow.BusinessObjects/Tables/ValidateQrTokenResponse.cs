namespace DineFlow.BusinessObjects.Tables
{
    /// <summary>
    /// Response returned to Customer Web after QR token validation.
    /// </summary>
    public class ValidateQrTokenResponse
    {
        public bool IsValid { get; set; }
        public int? TableId { get; set; }
        public string? TableName { get; set; }
        public string? AreaName { get; set; }
        public string? TableStatus { get; set; }
        public bool CanOrder { get; set; }
        public string Message { get; set; } = null!;

        public static ValidateQrTokenResponse Invalid(string message) => new()
        {
            IsValid    = false,
            TableId    = null,
            TableName  = null,
            AreaName   = null,
            TableStatus = null,
            CanOrder   = false,
            Message    = message
        };
    }
}
