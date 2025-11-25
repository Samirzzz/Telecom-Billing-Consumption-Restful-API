using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Utils;

namespace TelecomBilling.Api.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly TelecomBillingDbContext _context;
        private readonly ICostCalculationService _costCalculationService;

        public InvoiceService(TelecomBillingDbContext context, ICostCalculationService costCalculationService)
        {
            _context = context;
            _costCalculationService = costCalculationService;
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

            if (string.IsNullOrWhiteSpace(request.Month))
            {
                throw new ArgumentException("Month is required and must be in format YYYY-MM (e.g., 2024-10)");
            }

            var normalizedMonth = MonthFormatHelper.NormalizeMonthFormat(request.Month);
            if (string.IsNullOrEmpty(normalizedMonth))
            {
                throw new ArgumentException($"Invalid month format: '{request.Month}'. Expected format: YYYY-MM (e.g., 2024-10)");
            }

            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.UserId == request.UserId && i.Month == normalizedMonth);

            if (existingInvoice != null)
            {
                throw new InvalidOperationException("Invoice already exists for this user and month");
            }

            var startDate = MonthFormatHelper.ParseMonthToStartDate(request.Month);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usageRecords = await _context.UsageRecords
                .Where(ur => ur.UserId == request.UserId && ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .OrderBy(ur => ur.Timestamp)
                .ToListAsync();

            if (!usageRecords.Any())
            {
                throw new InvalidOperationException("No usage records found for this month");
            }

            var invoice = new Invoice
            {
                UserId = request.UserId,
                Month = normalizedMonth,
                BillingDate = DateTime.UtcNow,
                VoiceMinutes = usageRecords.Sum(ur => ur.CallMinutes),
                DataMB = usageRecords.Sum(ur => ur.DataMB),
                SMSMessages = usageRecords.Sum(ur => ur.SMSCount),
                RoamingMinutes = usageRecords.Where(ur => ur.IsRoaming).Sum(ur => ur.CallMinutes),
                RoamingDataMB = usageRecords.Where(ur => ur.IsRoaming).Sum(ur => ur.DataMB),
                RoamingSMSMessages = usageRecords.Where(ur => ur.IsRoaming).Sum(ur => ur.SMSCount),
                CreatedAt = DateTime.UtcNow
            };

            invoice.VoiceAmount = usageRecords.Sum(ur => ur.CallCost);
            invoice.DataAmount = usageRecords.Sum(ur => ur.DataCost);
            invoice.SMSAmount = usageRecords.Sum(ur => ur.SMSCost);
            
            var roamingRecords = usageRecords.Where(ur => ur.IsRoaming).ToList();
            invoice.RoamingAmount = roamingRecords.Sum(ur => ur.DataCost) + 
                                   roamingRecords.Sum(ur => ur.SMSCost) + 
                                   roamingRecords.Sum(ur => ur.CallCost);
            
            invoice.TotalAmount = usageRecords.Sum(ur => ur.TotalCost);

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

            var normalizedMonth = MonthFormatHelper.NormalizeMonthFormat(request.Month);
            if (string.IsNullOrEmpty(normalizedMonth))
            {
                throw new ArgumentException($"Invalid month format: '{request.Month}'. Expected format: YYYY-MM (e.g., 2024-10)");
            }

            invoice.UserId = request.UserId;
            invoice.Month = normalizedMonth;
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
            var normalizedMonth = MonthFormatHelper.NormalizeMonthFormat(month) ?? month;
            var invoice = await GetInvoiceAsync(userId, normalizedMonth);
            
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

        public async Task<RevenueStatisticsResponse> GetRevenueStatisticsAsync(string? month, int? year)
        {
            var targetMonth = string.IsNullOrEmpty(month) ? DateTime.UtcNow.ToString("yyyy-MM") : MonthFormatHelper.NormalizeMonthFormat(month) ?? month;
            
            IQueryable<Invoice> query = _context.Invoices.Include(i => i.User);
            
            if (year.HasValue)
            {
                var startDate = new DateTime(year.Value, 1, 1);
                var endDate = new DateTime(year.Value, 12, 31);
                query = query.Where(i => i.BillingDate >= startDate && i.BillingDate <= endDate);
            }
            else
            {
                var startDate = MonthFormatHelper.ParseMonthToStartDate(month ?? DateTime.UtcNow.ToString("yyyy-MM"));
                var endDate = startDate.AddMonths(1).AddDays(-1);
                query = query.Where(i => i.BillingDate >= startDate && i.BillingDate <= endDate);
            }

            var revenueStats = await query
                .GroupBy(i => 1)
                .Select(g => new
                {
                    TotalRevenue = g.Sum(i => i.TotalAmount),
                    VoiceRevenue = g.Sum(i => i.VoiceAmount),
                    DataRevenue = g.Sum(i => i.DataAmount),
                    SMSRevenue = g.Sum(i => i.SMSAmount),
                    RoamingRevenue = g.Sum(i => i.RoamingAmount),
                    TotalBillsGenerated = g.Count(),
                    VATAmount = g.Sum(i => i.TotalAmount) * 0.15m, // Assuming 15% VAT
                    LoyaltyDiscountAmount = g.Sum(i => i.TotalAmount) * 0.05m // Assuming 5% loyalty discount
                })
                .FirstOrDefaultAsync();

            var activeSubscribers = await _context.Users.CountAsync(u => u.IsActive);

            var revenueByPlanType = await query
                .GroupBy(i => i.User!.PlanType)
                .Select(g => new RevenueByPlanType
                {
                    PlanType = g.Key,
                    SubscriberCount = g.Select(i => i.UserId).Distinct().Count(),
                    TotalRevenue = g.Sum(i => i.TotalAmount),
                    AverageRevenuePerSubscriber = g.Sum(i => i.TotalAmount) / g.Select(i => i.UserId).Distinct().Count()
                })
                .ToListAsync();

            return new RevenueStatisticsResponse
            {
                Month = year.HasValue ? year.Value.ToString() : targetMonth,
                TotalRevenue = revenueStats?.TotalRevenue ?? 0,
                VoiceRevenue = revenueStats?.VoiceRevenue ?? 0,
                DataRevenue = revenueStats?.DataRevenue ?? 0,
                SMSRevenue = revenueStats?.SMSRevenue ?? 0,
                RoamingRevenue = revenueStats?.RoamingRevenue ?? 0,
                VATAmount = revenueStats?.VATAmount ?? 0,
                LoyaltyDiscountAmount = revenueStats?.LoyaltyDiscountAmount ?? 0,
                TotalBillsGenerated = revenueStats?.TotalBillsGenerated ?? 0,
                ActiveSubscribers = activeSubscribers,
                AverageRevenuePerSubscriber = activeSubscribers > 0 ? (revenueStats?.TotalRevenue ?? 0) / activeSubscribers : 0,
                RevenueByPlanType = revenueByPlanType
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