using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables
{
    /// <summary>
    /// Internal port contract — chỉ Member 4/5 được gọi.
    /// KHÔNG đăng ký trong DI container của WPF layer.
    /// WPF không được gọi trực tiếp các method này.
    /// </summary>
    public interface ITableStatusPort
    {
        void SetTableOccupied(int tableId, int tableSessionId);
        void SetTableWaitingPayment(int tableId, int tableSessionId);
        void SetTableAvailable(int tableId, int tableSessionId);
        DiningTableDto SyncTableStatus(int tableId);
    }
}
