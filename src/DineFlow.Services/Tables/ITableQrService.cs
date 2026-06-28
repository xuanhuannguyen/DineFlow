using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables
{
    public interface ITableQrService
    {
        /// <summary>Sinh token từ Guid.NewGuid().ToString("N"), retry nếu trùng.</summary>
        string GenerateQrToken();

        /// <summary>Tạo QR URL từ CustomerWebBaseUrl (appsettings.json) + "?t=" + qrToken.</summary>
        string GenerateQrUrl(string qrToken);

        /// <summary>Lấy thông tin QR hiện tại của bàn.</summary>
        TableQrDto GetQrByTableId(int tableId);

        /// <summary>Reset QR — chỉ cho phép khi bàn Available.</summary>
        TableQrDto ResetQrToken(int tableId);

        /// <summary>Xác thực QR token cho Customer Web.</summary>
        ValidateQrTokenResponse ValidateQrToken(string qrToken);
    }
}
