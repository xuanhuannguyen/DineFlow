using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;
using Microsoft.Extensions.Configuration;

namespace DineFlow.Services.Tables
{
    public class TableQrService : ITableQrService
    {
        private readonly IDiningTableRepository _tableRepo;
        private readonly IConfiguration         _configuration;

        public TableQrService(IDiningTableRepository tableRepo, IConfiguration configuration)
        {
            _tableRepo     = tableRepo;
            _configuration = configuration;
        }

        // BR-QR-001 + BR-QR-004: Sinh token từ Guid, retry nếu trùng
        public string GenerateQrToken()
        {
            string token;
            int attempts = 0;
            do
            {
                if (++attempts > 10)
                    throw new Exception("QR_TOKEN_DUPLICATED: QR token bị trùng, vui lòng thử lại.");

                token = Guid.NewGuid().ToString("N"); // 32 hex chars, no dashes
            }
            while (_tableRepo.IsQrTokenExists(token));

            return token;
        }

        // BR-QR-003: QR URL từ appsettings.json
        public string GenerateQrUrl(string qrToken)
        {
            var baseUrl = _configuration["CustomerWeb:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("CONFIG_CUSTOMER_WEB_BASE_URL_MISSING: Chưa cấu hình CustomerWebBaseUrl trong appsettings.json.");

            return $"{baseUrl.TrimEnd('/')}?t={qrToken}";
        }

        public TableQrDto GetQrByTableId(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            return new TableQrDto
            {
                TableId   = table.TableId,
                TableName = table.TableName,
                QrToken   = table.QrToken,
                QrUrl     = GenerateQrUrl(table.QrToken),
                UpdatedAt = table.UpdatedAt
            };
        }

        // BR-QR-005: Không reset QR khi bàn đang phục vụ
        public TableQrDto ResetQrToken(int tableId)
        {
            var table = _tableRepo.GetById(tableId)
                ?? throw new Exception("TABLE_NOT_FOUND: Không tìm thấy bàn.");

            if (table.Status == TableStatus.Occupied || table.Status == TableStatus.WaitingPayment)
                throw new Exception("QR_RESET_BLOCKED_ACTIVE_TABLE: Không thể reset QR khi bàn đang phục vụ hoặc chờ thanh toán.");

            var newToken = GenerateQrToken();
            _tableRepo.UpdateQrToken(tableId, newToken);

            return new TableQrDto
            {
                TableId   = table.TableId,
                TableName = table.TableName,
                QrToken   = newToken,
                QrUrl     = GenerateQrUrl(newToken),
                UpdatedAt = DateTime.UtcNow
            };
        }

        // BR-QR-006: Validate QR token cho Customer Web
        public ValidateQrTokenResponse ValidateQrToken(string qrToken)
        {
            if (string.IsNullOrWhiteSpace(qrToken))
                return ValidateQrTokenResponse.Invalid("QR_TOKEN_INVALID: QR không hợp lệ hoặc đã hết hiệu lực.");

            var table = _tableRepo.GetByQrToken(qrToken);
            if (table == null)
                return ValidateQrTokenResponse.Invalid("QR_TOKEN_INVALID: QR không hợp lệ hoặc đã hết hiệu lực.");

            if (!table.IsActive)
                return new ValidateQrTokenResponse
                {
                    IsValid     = true,
                    TableId     = table.TableId,
                    TableName   = table.TableName,
                    AreaName    = table.Area?.AreaName,
                    TableStatus = table.Status,
                    CanOrder    = false,
                    Message     = "TABLE_INACTIVE: Bàn hiện không còn hoạt động."
                };

            bool canOrder = table.Status != TableStatus.WaitingPayment;

            return new ValidateQrTokenResponse
            {
                IsValid     = true,
                TableId     = table.TableId,
                TableName   = table.TableName,
                AreaName    = table.Area?.AreaName,
                TableStatus = table.Status,
                CanOrder    = canOrder,
                Message     = canOrder ? "QR hợp lệ." : "Bàn đang chờ thanh toán, không thể gọi thêm món."
            };
        }
    }
}
