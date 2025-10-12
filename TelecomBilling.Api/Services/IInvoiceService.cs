using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse?> GetInvoiceAsync(int userId, string month);
        Task<InvoiceListResponse> GetInvoicesAsync(int? userId = null, int pageNumber = 1, int pageSize = 10);
        Task<InvoiceResponse> CreateInvoiceAsync(InvoiceRequest request);
        Task<InvoiceResponse?> UpdateInvoiceAsync(int id, InvoiceRequest request);
        Task<bool> DeleteInvoiceAsync(int id);
        Task<object> GetInvoiceWithFormatAsync(int userId, string month, ResponseFormat format);
        Task<object> GetInvoicesWithFormatAsync(int? userId, ResponseFormat format, int pageNumber = 1, int pageSize = 10);
    }
}
