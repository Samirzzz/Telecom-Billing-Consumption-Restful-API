using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public class TariffRuleService : ITariffRuleService
    {
        private readonly TelecomBillingDbContext _context;

        public TariffRuleService(TelecomBillingDbContext context)
        {
            _context = context;
        }

        public async Task<TariffRuleListResponse> GetTariffRulesAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.TariffRules.AsQueryable();
            var totalCount = await query.CountAsync();
            
            var tariffRules = await query
                .OrderBy(tr => tr.PlanType)
                .ThenBy(tr => tr.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new TariffRuleListResponse
            {
                TariffRules = tariffRules.Select(MapToTariffRuleResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<TariffRuleResponse?> GetTariffRuleAsync(int id)
        {
            var tariffRule = await _context.TariffRules.FindAsync(id);
            return tariffRule != null ? MapToTariffRuleResponse(tariffRule) : null;
        }

        public async Task<TariffRuleResponse> CreateTariffRuleAsync(TariffRuleRequest request)
        {
            var tariffRule = new TariffRule
            {
                Name = request.Name,
                PlanType = request.PlanType,
                VoicePeakRate = request.VoicePeakRate,
                VoiceOffPeakRate = request.VoiceOffPeakRate,
                DataRate = request.DataRate,
                SMSRate = request.SMSRate,
                RoamingVoiceRate = request.RoamingVoiceRate,
                RoamingDataRate = request.RoamingDataRate,
                RoamingSMSRate = request.RoamingSMSRate,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.TariffRules.Add(tariffRule);
            await _context.SaveChangesAsync();

            return MapToTariffRuleResponse(tariffRule);
        }

        public async Task<TariffRuleResponse?> UpdateTariffRuleAsync(int id, TariffRuleRequest request)
        {
            var tariffRule = await _context.TariffRules.FindAsync(id);
            if (tariffRule == null)
            {
                return null;
            }

            tariffRule.Name = request.Name;
            tariffRule.PlanType = request.PlanType;
            tariffRule.VoicePeakRate = request.VoicePeakRate;
            tariffRule.VoiceOffPeakRate = request.VoiceOffPeakRate;
            tariffRule.DataRate = request.DataRate;
            tariffRule.SMSRate = request.SMSRate;
            tariffRule.RoamingVoiceRate = request.RoamingVoiceRate;
            tariffRule.RoamingDataRate = request.RoamingDataRate;
            tariffRule.RoamingSMSRate = request.RoamingSMSRate;
            tariffRule.IsActive = request.IsActive;
            tariffRule.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToTariffRuleResponse(tariffRule);
        }

        public async Task<bool> DeleteTariffRuleAsync(int id)
        {
            var tariffRule = await _context.TariffRules.FindAsync(id);
            if (tariffRule == null)
            {
                return false;
            }

            _context.TariffRules.Remove(tariffRule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TariffRuleResponse?> GetActiveTariffRuleAsync(string planType)
        {
            var tariffRule = await _context.TariffRules
                .FirstOrDefaultAsync(tr => tr.PlanType == planType && tr.IsActive);

            return tariffRule != null ? MapToTariffRuleResponse(tariffRule) : null;
        }

        private static TariffRuleResponse MapToTariffRuleResponse(TariffRule tariffRule)
        {
            return new TariffRuleResponse
            {
                Id = tariffRule.Id,
                Name = tariffRule.Name,
                PlanType = tariffRule.PlanType,
                VoicePeakRate = tariffRule.VoicePeakRate,
                VoiceOffPeakRate = tariffRule.VoiceOffPeakRate,
                DataRate = tariffRule.DataRate,
                SMSRate = tariffRule.SMSRate,
                RoamingVoiceRate = tariffRule.RoamingVoiceRate,
                RoamingDataRate = tariffRule.RoamingDataRate,
                RoamingSMSRate = tariffRule.RoamingSMSRate,
                IsActive = tariffRule.IsActive,
                CreatedAt = tariffRule.CreatedAt,
                LastUpdated = tariffRule.LastUpdated
            };
        }
    }
}
