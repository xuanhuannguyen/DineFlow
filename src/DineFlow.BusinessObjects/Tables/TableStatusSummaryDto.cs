namespace DineFlow.BusinessObjects.Tables
{
    /// <summary>
    /// Composite DTO cho màn tổng quan bàn của Staff/Admin WPF.
    /// Dữ liệu bàn do Member 2 cung cấp;
    /// CurrentSessionId/OrderCount do Member 4 cung cấp;
    /// BillCount/UnpaidAmount do Member 5 cung cấp (để null/0 nếu chưa tích hợp).
    /// </summary>
    public class TableStatusSummaryDto
    {
        // Member 2 data
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public string? AreaName { get; set; }
        public string Status { get; set; } = null!;
        public bool IsActive { get; set; }

        // Member 4 data
        public int? CurrentSessionId { get; set; }
        public DateTime? StartedAt { get; set; }
        public int OrderCount { get; set; }

        // Member 5 data
        public int BillCount { get; set; }
        public decimal UnpaidAmount { get; set; }
    }
}
