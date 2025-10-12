using TelecomBilling.Api.DTOs;

namespace TelecomBilling.Api.Services
{
    public interface ITariffRuleService
    {
        Task<TariffRuleListResponse> GetTariffRulesAsync(int pageNumber = 1, int pageSize = 10);
        Task<TariffRuleResponse?> GetTariffRuleAsync(int id);
        Task<TariffRuleResponse> CreateTariffRuleAsync(TariffRuleRequest request);
        Task<TariffRuleResponse?> UpdateTariffRuleAsync(int id, TariffRuleRequest request);
        Task<bool> DeleteTariffRuleAsync(int id);
        Task<TariffRuleResponse?> GetActiveTariffRuleAsync(string planType);
    }
}
