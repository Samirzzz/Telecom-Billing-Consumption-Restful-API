using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly TelecomBillingDbContext _context;

        public InvoiceService(TelecomBillingDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceResponse?> GetInvoiceAsync(int userId, string month)
        {
            var invoice = await _context.Invoices
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.UserId == userId && i.Month == month);

            return invoice != null ? MapToInvoiceResponse(invoice) : null;
        }

        public async Task<InvoiceListResponse> GetInvoicesAsync(int? userId = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Invoices.Include(i => i.User).AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(i => i.UserId == userId.Value);
            }

            var totalCount = await query.CountAsync();
            var invoices = await query
                .OrderByDescending(i => i.BillingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new InvoiceListResponse
            {
                Invoices = invoices.Select(MapToInvoiceResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<InvoiceResponse> CreateInvoiceAsync(InvoiceRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Check if invoice already exists for this month
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.UserId == request.UserId && i.Month == request.Month);

            if (existingInvoice != null)
            {
                throw new InvalidOperationException("Invoice already exists for this user and month");
            }

            // Generate sample invoice data (in a real application, this would be calculated)
            var invoice = new Invoice
            {
                UserId = request.UserId,
                Month = request.Month,
                BillingDate = DateTime.UtcNow,
                VoiceMinutes = Random.Shared.Next(100, 1000),
                DataMB = Random.Shared.Next(500, 5000),
                SMSMessages = Random.Shared.Next(50, 500),
                RoamingMinutes = user.IsRoaming ? Random.Shared.Next(0, 200) : 0,
                RoamingDataMB = user.IsRoaming ? Random.Shared.Next(0, 1000) : 0,
                RoamingSMSMessages = user.IsRoaming ? Random.Shared.Next(0, 50) : 0,
                CreatedAt = DateTime.UtcNow
            };

            // Calculate amounts based on plan type (sample rates)
            invoice.VoiceAmount = invoice.VoiceMinutes * 0.05m; // $0.05 per minute
            invoice.DataAmount = invoice.DataMB * 0.01m; // $0.01 per MB
            invoice.SMSAmount = invoice.SMSMessages * 0.10m; // $0.10 per SMS
            invoice.RoamingAmount = (invoice.RoamingMinutes * 0.15m) + (invoice.RoamingDataMB * 0.05m) + (invoice.RoamingSMSMessages * 0.25m);
            invoice.TotalAmount = invoice.VoiceAmount + invoice.DataAmount + invoice.SMSAmount + invoice.RoamingAmount;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return await GetInvoiceAsync(invoice.UserId, invoice.Month) ?? 
                   throw new InvalidOperationException("Failed to retrieve created invoice");
        }

        public async Task<InvoiceResponse?> UpdateInvoiceAsync(int id, InvoiceRequest request)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return null;
            }

            invoice.UserId = request.UserId;
            invoice.Month = request.Month;
            invoice.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetInvoiceAsync(invoice.UserId, invoice.Month);
        }

        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return false;
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> GetInvoiceWithFormatAsync(int userId, string month, ResponseFormat format)
        {
            var invoice = await GetInvoiceAsync(userId, month);
            
            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found");
            }

            return format switch
            {
                ResponseFormat.Json => invoice,
                ResponseFormat.Soap => invoice,
                ResponseFormat.Xml => invoice,
                _ => invoice
            };
        }

        public async Task<object> GetInvoicesWithFormatAsync(int? userId, ResponseFormat format, int pageNumber = 1, int pageSize = 10)
        {
            var invoiceList = await GetInvoicesAsync(userId, pageNumber, pageSize);

            return format switch
            {
                ResponseFormat.Json => invoiceList,
                ResponseFormat.Soap => invoiceList,
                ResponseFormat.Xml => invoiceList,
                _ => invoiceList
            };
        }

        private static InvoiceResponse MapToInvoiceResponse(Invoice invoice)
        {
            return new InvoiceResponse
            {
                Id = invoice.Id,
                UserId = invoice.UserId,
                Month = invoice.Month,
                BillingDate = invoice.BillingDate,
                TotalAmount = invoice.TotalAmount,
                VoiceAmount = invoice.VoiceAmount,
                DataAmount = invoice.DataAmount,
                SMSAmount = invoice.SMSAmount,
                RoamingAmount = invoice.RoamingAmount,
                VoiceMinutes = invoice.VoiceMinutes,
                DataMB = invoice.DataMB,
                SMSMessages = invoice.SMSMessages,
                RoamingMinutes = invoice.RoamingMinutes,
                RoamingDataMB = invoice.RoamingDataMB,
                RoamingSMSMessages = invoice.RoamingSMSMessages,
                User = invoice.User != null ? new UserInfo
                {
                    Id = invoice.User.Id,
                    Name = invoice.User.Name,
                    PhoneNumber = invoice.User.PhoneNumber,
                    PlanType = invoice.User.PlanType,
                    Country = invoice.User.Country,
                    IsRoaming = invoice.User.IsRoaming,
                    IsActive = invoice.User.IsActive
                } : null
            };
        }
    }
}