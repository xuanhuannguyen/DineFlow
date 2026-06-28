namespace DineFlow.BusinessObjects.Tables
{
    /// <summary>
    /// Hằng số trạng thái bàn ăn.
    /// Không dùng enum để tránh chuyển đổi khi lưu xuống database dạng string.
    /// </summary>
    public static class TableStatus
    {
        public const string Available       = "Available";
        public const string Occupied        = "Occupied";
        public const string WaitingPayment  = "WaitingPayment";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Available,
            Occupied,
            WaitingPayment
        };

        public static bool IsValid(string status) =>
            status == Available || status == Occupied || status == WaitingPayment;
    }
}
