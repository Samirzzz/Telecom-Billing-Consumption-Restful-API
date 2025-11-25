using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Formatters;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Formatters
{
    public class SoapXmlFormatter : TextOutputFormatter
    {
        public SoapXmlFormatter()
        {
            SupportedMediaTypes.Add("text/xml");
            SupportedMediaTypes.Add("application/soap+xml");
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            // Check if the response format query parameter is set to SOAP
            var request = context.HttpContext.Request;
            var responseFormatParam = request.Query["responseFormat"].FirstOrDefault();
            
            // Check if responseFormat=1 (Soap) is requested
            bool isSoapRequested = responseFormatParam == "1" || 
                                 responseFormatParam?.ToLower() == "soap" ||
                                 request.Headers.Accept.Any(a => a?.Contains("text/xml") == true || a?.Contains("application/soap+xml") == true);

            if (!isSoapRequested)
                return false;

            return context.ObjectType == typeof(BillingResponse) || 
                   context.ObjectType == typeof(BillingListResponse) ||
                   context.ObjectType == typeof(AuthResponse) ||
                   context.ObjectType == typeof(InvoiceResponse) ||
                   context.ObjectType == typeof(InvoiceListResponse) ||
                   context.ObjectType == typeof(UsageRecordResponse) ||
                   context.ObjectType == typeof(UsageRecordListResponse) ||
                   context.ObjectType == typeof(ConsumptionSummaryResponse) ||
                   context.ObjectType == typeof(BulkUsageRecordResponse) ||
                   context.ObjectType == typeof(SubscriberResponse) ||
                   context.ObjectType == typeof(SubscriberListResponse) ||
                   context.ObjectType == typeof(TopConsumersResponse) ||
                   context.ObjectType == typeof(UsageStatisticsResponse) ||
                   context.ObjectType == typeof(RevenueStatisticsResponse) ||
                   base.CanWriteResult(context);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "text/xml; charset=utf-8";
            var buffer = new StringBuilder();

            // Handle different response types
            switch (context.Object)
            {
                case BillingResponse billing:
                    buffer.Append(GenerateSoapXml(billing));
                    break;
                case BillingListResponse billingList:
                    buffer.Append(GenerateSoapXmlList(billingList));
                    break;
                case AuthResponse auth:
                    buffer.Append(GenerateAuthSoapXml(auth));
                    break;
                case InvoiceResponse invoice:
                    buffer.Append(GenerateInvoiceSoapXml(invoice));
                    break;
                case InvoiceListResponse invoiceList:
                    buffer.Append(GenerateInvoiceListSoapXml(invoiceList));
                    break;
                case UsageRecordResponse usageRecord:
                    buffer.Append(GenerateUsageRecordSoapXml(usageRecord));
                    break;
                case UsageRecordListResponse usageRecordList:
                    buffer.Append(GenerateUsageRecordListSoapXml(usageRecordList));
                    break;
                case ConsumptionSummaryResponse consumptionSummary:
                    buffer.Append(GenerateConsumptionSummarySoapXml(consumptionSummary));
                    break;
                case BulkUsageRecordResponse bulkUsage:
                    buffer.Append(GenerateBulkUsageRecordSoapXml(bulkUsage));
                    break;
                case SubscriberResponse subscriber:
                    buffer.Append(GenerateSubscriberSoapXml(subscriber));
                    break;
                case SubscriberListResponse subscriberList:
                    buffer.Append(GenerateSubscriberListSoapXml(subscriberList));
                    break;
                case TopConsumersResponse topConsumers:
                    buffer.Append(GenerateTopConsumersSoapXml(topConsumers));
                    break;
                case UsageStatisticsResponse usageStats:
                    buffer.Append(GenerateUsageStatisticsSoapXml(usageStats));
                    break;
                case RevenueStatisticsResponse revenueStats:
                    buffer.Append(GenerateRevenueStatisticsSoapXml(revenueStats));
                    break;
            }

            await response.WriteAsync(buffer.ToString(), selectedEncoding);
        }

        private string GenerateSoapXml(BillingResponse billing)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:GetBillingResponse>");
            soap.AppendLine("      <tns:Billing>");
            soap.AppendLine($"        <tns:Id>{billing.Id}</tns:Id>");
            soap.AppendLine($"        <tns:SubscriberId>{billing.SubscriberId}</tns:SubscriberId>");
            soap.AppendLine($"        <tns:Month>{billing.Month}</tns:Month>");
            soap.AppendLine($"        <tns:BillingDate>{billing.BillingDate:yyyy-MM-ddTHH:mm:ssZ}</tns:BillingDate>");
            soap.AppendLine($"        <tns:TotalAmount>{billing.TotalAmount:F2}</tns:TotalAmount>");
            soap.AppendLine($"        <tns:VoiceAmount>{billing.VoiceAmount:F2}</tns:VoiceAmount>");
            soap.AppendLine($"        <tns:DataAmount>{billing.DataAmount:F2}</tns:DataAmount>");
            soap.AppendLine($"        <tns:SMSAmount>{billing.SMSAmount:F2}</tns:SMSAmount>");
            soap.AppendLine($"        <tns:RoamingAmount>{billing.RoamingAmount:F2}</tns:RoamingAmount>");
            soap.AppendLine($"        <tns:VoiceMinutes>{billing.VoiceMinutes}</tns:VoiceMinutes>");
            soap.AppendLine($"        <tns:DataMB>{billing.DataMB}</tns:DataMB>");
            soap.AppendLine($"        <tns:SMSMessages>{billing.SMSMessages}</tns:SMSMessages>");
            soap.AppendLine($"        <tns:RoamingMinutes>{billing.RoamingMinutes}</tns:RoamingMinutes>");
            soap.AppendLine($"        <tns:RoamingDataMB>{billing.RoamingDataMB}</tns:RoamingDataMB>");
            soap.AppendLine($"        <tns:RoamingSMSMessages>{billing.RoamingSMSMessages}</tns:RoamingSMSMessages>");
            
            if (billing.Subscriber != null)
            {
                soap.AppendLine("        <tns:Subscriber>");
                soap.AppendLine($"          <tns:Id>{billing.Subscriber.Id}</tns:Id>");
                soap.AppendLine($"          <tns:Name>{XmlEscape(billing.Subscriber.Name)}</tns:Name>");
                soap.AppendLine($"          <tns:PhoneNumber>{XmlEscape(billing.Subscriber.PhoneNumber)}</tns:PhoneNumber>");
                soap.AppendLine($"          <tns:PlanType>{XmlEscape(billing.Subscriber.PlanType)}</tns:PlanType>");
                soap.AppendLine($"          <tns:Country>{XmlEscape(billing.Subscriber.Country)}</tns:Country>");
                soap.AppendLine($"          <tns:IsRoaming>{billing.Subscriber.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                soap.AppendLine($"          <tns:Active>{billing.Subscriber.Active.ToString().ToLower()}</tns:Active>");
                soap.AppendLine("        </tns:Subscriber>");
            }
            
            soap.AppendLine("      </tns:Billing>");
            soap.AppendLine("    </tns:GetBillingResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateSoapXmlList(BillingListResponse billingList)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:GetBillingListResponse>");
            soap.AppendLine($"      <tns:TotalCount>{billingList.TotalCount}</tns:TotalCount>");
            soap.AppendLine($"      <tns:PageNumber>{billingList.PageNumber}</tns:PageNumber>");
            soap.AppendLine($"      <tns:PageSize>{billingList.PageSize}</tns:PageSize>");
            soap.AppendLine("      <tns:Billings>");
            
            foreach (var billing in billingList.Billings)
            {
                soap.AppendLine("        <tns:Billing>");
                soap.AppendLine($"          <tns:Id>{billing.Id}</tns:Id>");
                soap.AppendLine($"          <tns:SubscriberId>{billing.SubscriberId}</tns:SubscriberId>");
                soap.AppendLine($"          <tns:Month>{billing.Month}</tns:Month>");
                soap.AppendLine($"          <tns:BillingDate>{billing.BillingDate:yyyy-MM-ddTHH:mm:ssZ}</tns:BillingDate>");
                soap.AppendLine($"          <tns:TotalAmount>{billing.TotalAmount:F2}</tns:TotalAmount>");
                soap.AppendLine($"          <tns:VoiceAmount>{billing.VoiceAmount:F2}</tns:VoiceAmount>");
                soap.AppendLine($"          <tns:DataAmount>{billing.DataAmount:F2}</tns:DataAmount>");
                soap.AppendLine($"          <tns:SMSAmount>{billing.SMSAmount:F2}</tns:SMSAmount>");
                soap.AppendLine($"          <tns:RoamingAmount>{billing.RoamingAmount:F2}</tns:RoamingAmount>");
                soap.AppendLine($"          <tns:VoiceMinutes>{billing.VoiceMinutes}</tns:VoiceMinutes>");
                soap.AppendLine($"          <tns:DataMB>{billing.DataMB}</tns:DataMB>");
                soap.AppendLine($"          <tns:SMSMessages>{billing.SMSMessages}</tns:SMSMessages>");
                soap.AppendLine($"          <tns:RoamingMinutes>{billing.RoamingMinutes}</tns:RoamingMinutes>");
                soap.AppendLine($"          <tns:RoamingDataMB>{billing.RoamingDataMB}</tns:RoamingDataMB>");
                soap.AppendLine($"          <tns:RoamingSMSMessages>{billing.RoamingSMSMessages}</tns:RoamingSMSMessages>");
                
                if (billing.Subscriber != null)
                {
                    soap.AppendLine("          <tns:Subscriber>");
                    soap.AppendLine($"            <tns:Id>{billing.Subscriber.Id}</tns:Id>");
                    soap.AppendLine($"            <tns:Name>{XmlEscape(billing.Subscriber.Name)}</tns:Name>");
                    soap.AppendLine($"            <tns:PhoneNumber>{XmlEscape(billing.Subscriber.PhoneNumber)}</tns:PhoneNumber>");
                    soap.AppendLine($"            <tns:PlanType>{XmlEscape(billing.Subscriber.PlanType)}</tns:PlanType>");
                    soap.AppendLine($"            <tns:Country>{XmlEscape(billing.Subscriber.Country)}</tns:Country>");
                    soap.AppendLine($"            <tns:IsRoaming>{billing.Subscriber.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                    soap.AppendLine($"            <tns:Active>{billing.Subscriber.Active.ToString().ToLower()}</tns:Active>");
                    soap.AppendLine("          </tns:Subscriber>");
                }
                
                soap.AppendLine("        </tns:Billing>");
            }
            
            soap.AppendLine("      </tns:Billings>");
            soap.AppendLine("    </tns:GetBillingListResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateAuthSoapXml(AuthResponse auth)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:AuthResponse>");
            soap.AppendLine($"      <tns:Token>{XmlEscape(auth.Token)}</tns:Token>");
            soap.AppendLine($"      <tns:RefreshToken>{XmlEscape(auth.RefreshToken)}</tns:RefreshToken>");
            soap.AppendLine($"      <tns:ExpiresAt>{auth.ExpiresAt:yyyy-MM-ddTHH:mm:ssZ}</tns:ExpiresAt>");
            
            if (auth.User != null)
            {
                soap.AppendLine("      <tns:User>");
                soap.AppendLine($"        <tns:Id>{auth.User.Id}</tns:Id>");
                soap.AppendLine($"        <tns:Username>{XmlEscape(auth.User.Username)}</tns:Username>");
                soap.AppendLine($"        <tns:Email>{XmlEscape(auth.User.Email)}</tns:Email>");
                soap.AppendLine($"        <tns:Role>{XmlEscape(auth.User.Role)}</tns:Role>");
                soap.AppendLine($"        <tns:Name>{XmlEscape(auth.User.Name)}</tns:Name>");
                soap.AppendLine($"        <tns:PhoneNumber>{XmlEscape(auth.User.PhoneNumber)}</tns:PhoneNumber>");
                soap.AppendLine($"        <tns:PlanType>{XmlEscape(auth.User.PlanType)}</tns:PlanType>");
                soap.AppendLine($"        <tns:Country>{XmlEscape(auth.User.Country)}</tns:Country>");
                soap.AppendLine($"        <tns:IsRoaming>{auth.User.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                soap.AppendLine($"        <tns:IsActive>{auth.User.IsActive.ToString().ToLower()}</tns:IsActive>");
                soap.AppendLine("      </tns:User>");
            }
            
            soap.AppendLine("    </tns:AuthResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateInvoiceSoapXml(InvoiceResponse invoice)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:InvoiceResponse>");
            soap.AppendLine("      <tns:Invoice>");
            soap.AppendLine($"        <tns:Id>{invoice.Id}</tns:Id>");
            soap.AppendLine($"        <tns:UserId>{invoice.UserId}</tns:UserId>");
            soap.AppendLine($"        <tns:Month>{invoice.Month}</tns:Month>");
            soap.AppendLine($"        <tns:BillingDate>{invoice.BillingDate:yyyy-MM-ddTHH:mm:ssZ}</tns:BillingDate>");
            soap.AppendLine($"        <tns:TotalAmount>{invoice.TotalAmount:F2}</tns:TotalAmount>");
            soap.AppendLine($"        <tns:VoiceAmount>{invoice.VoiceAmount:F2}</tns:VoiceAmount>");
            soap.AppendLine($"        <tns:DataAmount>{invoice.DataAmount:F2}</tns:DataAmount>");
            soap.AppendLine($"        <tns:SMSAmount>{invoice.SMSAmount:F2}</tns:SMSAmount>");
            soap.AppendLine($"        <tns:RoamingAmount>{invoice.RoamingAmount:F2}</tns:RoamingAmount>");
            soap.AppendLine($"        <tns:VoiceMinutes>{invoice.VoiceMinutes}</tns:VoiceMinutes>");
            soap.AppendLine($"        <tns:DataMB>{invoice.DataMB}</tns:DataMB>");
            soap.AppendLine($"        <tns:SMSMessages>{invoice.SMSMessages}</tns:SMSMessages>");
            soap.AppendLine($"        <tns:RoamingMinutes>{invoice.RoamingMinutes}</tns:RoamingMinutes>");
            soap.AppendLine($"        <tns:RoamingDataMB>{invoice.RoamingDataMB}</tns:RoamingDataMB>");
            soap.AppendLine($"        <tns:RoamingSMSMessages>{invoice.RoamingSMSMessages}</tns:RoamingSMSMessages>");
            
            if (invoice.User != null)
            {
                soap.AppendLine("        <tns:User>");
                soap.AppendLine($"          <tns:Id>{invoice.User.Id}</tns:Id>");
                soap.AppendLine($"          <tns:Username>{XmlEscape(invoice.User.Username)}</tns:Username>");
                soap.AppendLine($"          <tns:Email>{XmlEscape(invoice.User.Email)}</tns:Email>");
                soap.AppendLine($"          <tns:Role>{XmlEscape(invoice.User.Role)}</tns:Role>");
                soap.AppendLine($"          <tns:Name>{XmlEscape(invoice.User.Name)}</tns:Name>");
                soap.AppendLine($"          <tns:PhoneNumber>{XmlEscape(invoice.User.PhoneNumber)}</tns:PhoneNumber>");
                soap.AppendLine($"          <tns:PlanType>{XmlEscape(invoice.User.PlanType)}</tns:PlanType>");
                soap.AppendLine($"          <tns:Country>{XmlEscape(invoice.User.Country)}</tns:Country>");
                soap.AppendLine($"          <tns:IsRoaming>{invoice.User.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                soap.AppendLine($"          <tns:IsActive>{invoice.User.IsActive.ToString().ToLower()}</tns:IsActive>");
                soap.AppendLine("        </tns:User>");
            }
            
            soap.AppendLine("      </tns:Invoice>");
            soap.AppendLine("    </tns:InvoiceResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateInvoiceListSoapXml(InvoiceListResponse invoiceList)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:InvoiceListResponse>");
            soap.AppendLine($"      <tns:TotalCount>{invoiceList.TotalCount}</tns:TotalCount>");
            soap.AppendLine($"      <tns:PageNumber>{invoiceList.PageNumber}</tns:PageNumber>");
            soap.AppendLine($"      <tns:PageSize>{invoiceList.PageSize}</tns:PageSize>");
            soap.AppendLine("      <tns:Invoices>");
            
            foreach (var invoice in invoiceList.Invoices)
            {
                soap.AppendLine("        <tns:Invoice>");
                soap.AppendLine($"          <tns:Id>{invoice.Id}</tns:Id>");
                soap.AppendLine($"          <tns:UserId>{invoice.UserId}</tns:UserId>");
                soap.AppendLine($"          <tns:Month>{invoice.Month}</tns:Month>");
                soap.AppendLine($"          <tns:BillingDate>{invoice.BillingDate:yyyy-MM-ddTHH:mm:ssZ}</tns:BillingDate>");
                soap.AppendLine($"          <tns:TotalAmount>{invoice.TotalAmount:F2}</tns:TotalAmount>");
                soap.AppendLine($"          <tns:VoiceAmount>{invoice.VoiceAmount:F2}</tns:VoiceAmount>");
                soap.AppendLine($"          <tns:DataAmount>{invoice.DataAmount:F2}</tns:DataAmount>");
                soap.AppendLine($"          <tns:SMSAmount>{invoice.SMSAmount:F2}</tns:SMSAmount>");
                soap.AppendLine($"          <tns:RoamingAmount>{invoice.RoamingAmount:F2}</tns:RoamingAmount>");
                soap.AppendLine($"          <tns:VoiceMinutes>{invoice.VoiceMinutes}</tns:VoiceMinutes>");
                soap.AppendLine($"          <tns:DataMB>{invoice.DataMB}</tns:DataMB>");
                soap.AppendLine($"          <tns:SMSMessages>{invoice.SMSMessages}</tns:SMSMessages>");
                soap.AppendLine($"          <tns:RoamingMinutes>{invoice.RoamingMinutes}</tns:RoamingMinutes>");
                soap.AppendLine($"          <tns:RoamingDataMB>{invoice.RoamingDataMB}</tns:RoamingDataMB>");
                soap.AppendLine($"          <tns:RoamingSMSMessages>{invoice.RoamingSMSMessages}</tns:RoamingSMSMessages>");
                
                if (invoice.User != null)
                {
                    soap.AppendLine("          <tns:User>");
                    soap.AppendLine($"            <tns:Id>{invoice.User.Id}</tns:Id>");
                    soap.AppendLine($"            <tns:Username>{XmlEscape(invoice.User.Username)}</tns:Username>");
                    soap.AppendLine($"            <tns:Email>{XmlEscape(invoice.User.Email)}</tns:Email>");
                    soap.AppendLine($"            <tns:Role>{XmlEscape(invoice.User.Role)}</tns:Role>");
                    soap.AppendLine($"            <tns:Name>{XmlEscape(invoice.User.Name)}</tns:Name>");
                    soap.AppendLine($"            <tns:PhoneNumber>{XmlEscape(invoice.User.PhoneNumber)}</tns:PhoneNumber>");
                    soap.AppendLine($"            <tns:PlanType>{XmlEscape(invoice.User.PlanType)}</tns:PlanType>");
                    soap.AppendLine($"            <tns:Country>{XmlEscape(invoice.User.Country)}</tns:Country>");
                    soap.AppendLine($"            <tns:IsRoaming>{invoice.User.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                    soap.AppendLine($"            <tns:IsActive>{invoice.User.IsActive.ToString().ToLower()}</tns:IsActive>");
                    soap.AppendLine("          </tns:User>");
                }
                
                soap.AppendLine("        </tns:Invoice>");
            }
            
            soap.AppendLine("      </tns:Invoices>");
            soap.AppendLine("    </tns:InvoiceListResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateUsageRecordSoapXml(UsageRecordResponse usageRecord)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:UsageRecordResponse>");
            soap.AppendLine("      <tns:UsageRecord>");
            soap.AppendLine($"        <tns:Id>{usageRecord.Id}</tns:Id>");
            soap.AppendLine($"        <tns:UserId>{usageRecord.UserId}</tns:UserId>");
            soap.AppendLine($"        <tns:Timestamp>{usageRecord.Timestamp:yyyy-MM-ddTHH:mm:ssZ}</tns:Timestamp>");
            soap.AppendLine($"        <tns:CallMinutes>{usageRecord.CallMinutes}</tns:CallMinutes>");
            soap.AppendLine($"        <tns:DataMB>{usageRecord.DataMB}</tns:DataMB>");
            soap.AppendLine($"        <tns:SMSCount>{usageRecord.SMSCount}</tns:SMSCount>");
            soap.AppendLine($"        <tns:IsPeakTime>{usageRecord.IsPeakTime.ToString().ToLower()}</tns:IsPeakTime>");
            soap.AppendLine($"        <tns:IsRoaming>{usageRecord.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
            soap.AppendLine($"        <tns:CreatedAt>{usageRecord.CreatedAt:yyyy-MM-ddTHH:mm:ssZ}</tns:CreatedAt>");
            
            if (usageRecord.User != null)
            {
                soap.AppendLine("        <tns:User>");
                soap.AppendLine($"          <tns:Id>{usageRecord.User.Id}</tns:Id>");
                soap.AppendLine($"          <tns:Username>{XmlEscape(usageRecord.User.Username)}</tns:Username>");
                soap.AppendLine($"          <tns:Email>{XmlEscape(usageRecord.User.Email)}</tns:Email>");
                soap.AppendLine($"          <tns:Role>{XmlEscape(usageRecord.User.Role)}</tns:Role>");
                soap.AppendLine($"          <tns:Name>{XmlEscape(usageRecord.User.Name)}</tns:Name>");
                soap.AppendLine($"          <tns:PhoneNumber>{XmlEscape(usageRecord.User.PhoneNumber)}</tns:PhoneNumber>");
                soap.AppendLine($"          <tns:PlanType>{XmlEscape(usageRecord.User.PlanType)}</tns:PlanType>");
                soap.AppendLine($"          <tns:Country>{XmlEscape(usageRecord.User.Country)}</tns:Country>");
                soap.AppendLine($"          <tns:IsRoaming>{usageRecord.User.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                soap.AppendLine($"          <tns:IsActive>{usageRecord.User.IsActive.ToString().ToLower()}</tns:IsActive>");
                soap.AppendLine("        </tns:User>");
            }
            
            soap.AppendLine("      </tns:UsageRecord>");
            soap.AppendLine("    </tns:UsageRecordResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateUsageRecordListSoapXml(UsageRecordListResponse usageRecordList)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:UsageRecordListResponse>");
            soap.AppendLine($"      <tns:TotalCount>{usageRecordList.TotalCount}</tns:TotalCount>");
            soap.AppendLine($"      <tns:PageNumber>{usageRecordList.PageNumber}</tns:PageNumber>");
            soap.AppendLine($"      <tns:PageSize>{usageRecordList.PageSize}</tns:PageSize>");
            soap.AppendLine("      <tns:UsageRecords>");
            
            foreach (var usageRecord in usageRecordList.UsageRecords)
            {
                soap.AppendLine("        <tns:UsageRecord>");
                soap.AppendLine($"          <tns:Id>{usageRecord.Id}</tns:Id>");
                soap.AppendLine($"          <tns:UserId>{usageRecord.UserId}</tns:UserId>");
                soap.AppendLine($"          <tns:Timestamp>{usageRecord.Timestamp:yyyy-MM-ddTHH:mm:ssZ}</tns:Timestamp>");
                soap.AppendLine($"          <tns:CallMinutes>{usageRecord.CallMinutes}</tns:CallMinutes>");
                soap.AppendLine($"          <tns:DataMB>{usageRecord.DataMB}</tns:DataMB>");
                soap.AppendLine($"          <tns:SMSCount>{usageRecord.SMSCount}</tns:SMSCount>");
                soap.AppendLine($"          <tns:IsPeakTime>{usageRecord.IsPeakTime.ToString().ToLower()}</tns:IsPeakTime>");
                soap.AppendLine($"          <tns:IsRoaming>{usageRecord.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                soap.AppendLine($"          <tns:CreatedAt>{usageRecord.CreatedAt:yyyy-MM-ddTHH:mm:ssZ}</tns:CreatedAt>");
                
                if (usageRecord.User != null)
                {
                    soap.AppendLine("          <tns:User>");
                    soap.AppendLine($"            <tns:Id>{usageRecord.User.Id}</tns:Id>");
                    soap.AppendLine($"            <tns:Username>{XmlEscape(usageRecord.User.Username)}</tns:Username>");
                    soap.AppendLine($"            <tns:Email>{XmlEscape(usageRecord.User.Email)}</tns:Email>");
                    soap.AppendLine($"            <tns:Role>{XmlEscape(usageRecord.User.Role)}</tns:Role>");
                    soap.AppendLine($"            <tns:Name>{XmlEscape(usageRecord.User.Name)}</tns:Name>");
                    soap.AppendLine($"            <tns:PhoneNumber>{XmlEscape(usageRecord.User.PhoneNumber)}</tns:PhoneNumber>");
                    soap.AppendLine($"            <tns:PlanType>{XmlEscape(usageRecord.User.PlanType)}</tns:PlanType>");
                    soap.AppendLine($"            <tns:Country>{XmlEscape(usageRecord.User.Country)}</tns:Country>");
                    soap.AppendLine($"            <tns:IsRoaming>{usageRecord.User.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                    soap.AppendLine($"            <tns:IsActive>{usageRecord.User.IsActive.ToString().ToLower()}</tns:IsActive>");
                    soap.AppendLine("          </tns:User>");
                }
                
                soap.AppendLine("        </tns:UsageRecord>");
            }
            
            soap.AppendLine("      </tns:UsageRecords>");
            soap.AppendLine("    </tns:UsageRecordListResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateConsumptionSummarySoapXml(ConsumptionSummaryResponse consumptionSummary)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:ConsumptionSummaryResponse>");
            soap.AppendLine("      <tns:ConsumptionSummary>");
            soap.AppendLine($"        <tns:UserId>{consumptionSummary.UserId}</tns:UserId>");
            soap.AppendLine($"        <tns:Month>{consumptionSummary.Month}</tns:Month>");
            soap.AppendLine($"        <tns:TotalCallMinutes>{consumptionSummary.TotalCallMinutes}</tns:TotalCallMinutes>");
            soap.AppendLine($"        <tns:TotalDataMB>{consumptionSummary.TotalDataMB}</tns:TotalDataMB>");
            soap.AppendLine($"        <tns:TotalSMSCount>{consumptionSummary.TotalSMSCount}</tns:TotalSMSCount>");
            soap.AppendLine($"        <tns:PeakTimeMinutes>{consumptionSummary.PeakTimeMinutes}</tns:PeakTimeMinutes>");
            soap.AppendLine($"        <tns:OffPeakTimeMinutes>{consumptionSummary.OffPeakTimeMinutes}</tns:OffPeakTimeMinutes>");
            soap.AppendLine($"        <tns:RoamingMinutes>{consumptionSummary.RoamingMinutes}</tns:RoamingMinutes>");
            soap.AppendLine($"        <tns:RoamingDataMB>{consumptionSummary.RoamingDataMB}</tns:RoamingDataMB>");
            soap.AppendLine($"        <tns:RoamingSMSCount>{consumptionSummary.RoamingSMSCount}</tns:RoamingSMSCount>");
            
            if (consumptionSummary.UsageRecords.Any())
            {
                soap.AppendLine("        <tns:UsageRecords>");
                foreach (var record in consumptionSummary.UsageRecords)
                {
                    soap.AppendLine("          <tns:UsageRecord>");
                    soap.AppendLine($"            <tns:Id>{record.Id}</tns:Id>");
                    soap.AppendLine($"            <tns:UserId>{record.UserId}</tns:UserId>");
                    soap.AppendLine($"            <tns:Timestamp>{record.Timestamp:yyyy-MM-ddTHH:mm:ssZ}</tns:Timestamp>");
                    soap.AppendLine($"            <tns:CallMinutes>{record.CallMinutes}</tns:CallMinutes>");
                    soap.AppendLine($"            <tns:DataMB>{record.DataMB}</tns:DataMB>");
                    soap.AppendLine($"            <tns:SMSCount>{record.SMSCount}</tns:SMSCount>");
                    soap.AppendLine($"            <tns:IsPeakTime>{record.IsPeakTime.ToString().ToLower()}</tns:IsPeakTime>");
                    soap.AppendLine($"            <tns:IsRoaming>{record.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                    soap.AppendLine($"            <tns:CreatedAt>{record.CreatedAt:yyyy-MM-ddTHH:mm:ssZ}</tns:CreatedAt>");
                    soap.AppendLine("          </tns:UsageRecord>");
                }
                soap.AppendLine("        </tns:UsageRecords>");
            }
            
            soap.AppendLine("      </tns:ConsumptionSummary>");
            soap.AppendLine("    </tns:ConsumptionSummaryResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateBulkUsageRecordSoapXml(BulkUsageRecordResponse bulkUsage)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:BulkUsageRecordResponse>");
            soap.AppendLine($"      <tns:TotalRecords>{bulkUsage.TotalRecords}</tns:TotalRecords>");
            soap.AppendLine($"      <tns:SuccessfullyCreated>{bulkUsage.SuccessfullyCreated}</tns:SuccessfullyCreated>");
            soap.AppendLine($"      <tns:FailedRecords>{bulkUsage.FailedRecords}</tns:FailedRecords>");
            
            if (bulkUsage.Errors.Any())
            {
                soap.AppendLine("      <tns:Errors>");
                foreach (var error in bulkUsage.Errors)
                {
                    soap.AppendLine($"        <tns:Error>{XmlEscape(error)}</tns:Error>");
                }
                soap.AppendLine("      </tns:Errors>");
            }
            
            if (bulkUsage.CreatedRecords.Any())
            {
                soap.AppendLine("      <tns:CreatedRecords>");
                foreach (var record in bulkUsage.CreatedRecords)
                {
                    soap.AppendLine("        <tns:UsageRecord>");
                    soap.AppendLine($"          <tns:Id>{record.Id}</tns:Id>");
                    soap.AppendLine($"          <tns:UserId>{record.UserId}</tns:UserId>");
                    soap.AppendLine($"          <tns:Timestamp>{record.Timestamp:yyyy-MM-ddTHH:mm:ssZ}</tns:Timestamp>");
                    soap.AppendLine($"          <tns:CallMinutes>{record.CallMinutes}</tns:CallMinutes>");
                    soap.AppendLine($"          <tns:DataMB>{record.DataMB}</tns:DataMB>");
                    soap.AppendLine($"          <tns:SMSCount>{record.SMSCount}</tns:SMSCount>");
                    soap.AppendLine($"          <tns:IsPeakTime>{record.IsPeakTime.ToString().ToLower()}</tns:IsPeakTime>");
                    soap.AppendLine($"          <tns:IsRoaming>{record.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                    soap.AppendLine($"          <tns:CreatedAt>{record.CreatedAt:yyyy-MM-ddTHH:mm:ssZ}</tns:CreatedAt>");
                    soap.AppendLine("        </tns:UsageRecord>");
                }
                soap.AppendLine("      </tns:CreatedRecords>");
            }
            
            soap.AppendLine("    </tns:BulkUsageRecordResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateSubscriberSoapXml(SubscriberResponse subscriber)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:SubscriberResponse>");
            soap.AppendLine("      <tns:Subscriber>");
            soap.AppendLine($"        <tns:Id>{subscriber.Id}</tns:Id>");
            soap.AppendLine($"        <tns:Username>{XmlEscape(subscriber.Username)}</tns:Username>");
            soap.AppendLine($"        <tns:Email>{XmlEscape(subscriber.Email)}</tns:Email>");
            soap.AppendLine($"        <tns:Name>{XmlEscape(subscriber.Name)}</tns:Name>");
            soap.AppendLine($"        <tns:PhoneNumber>{XmlEscape(subscriber.PhoneNumber)}</tns:PhoneNumber>");
            soap.AppendLine($"        <tns:PlanType>{XmlEscape(subscriber.PlanType)}</tns:PlanType>");
            soap.AppendLine($"        <tns:Country>{XmlEscape(subscriber.Country)}</tns:Country>");
            soap.AppendLine($"        <tns:IsRoaming>{subscriber.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
            soap.AppendLine($"        <tns:IsActive>{subscriber.IsActive.ToString().ToLower()}</tns:IsActive>");
            soap.AppendLine($"        <tns:CreatedAt>{subscriber.CreatedAt:yyyy-MM-ddTHH:mm:ssZ}</tns:CreatedAt>");
            soap.AppendLine($"        <tns:LastUpdated>{subscriber.LastUpdated:yyyy-MM-ddTHH:mm:ssZ}</tns:LastUpdated>");
            soap.AppendLine($"        <tns:YearsActive>{subscriber.YearsActive}</tns:YearsActive>");
            soap.AppendLine($"        <tns:IsLoyaltyEligible>{subscriber.IsLoyaltyEligible.ToString().ToLower()}</tns:IsLoyaltyEligible>");
            soap.AppendLine("      </tns:Subscriber>");
            soap.AppendLine("    </tns:SubscriberResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateSubscriberListSoapXml(SubscriberListResponse subscriberList)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:SubscriberListResponse>");
            soap.AppendLine($"      <tns:TotalCount>{subscriberList.TotalCount}</tns:TotalCount>");
            soap.AppendLine($"      <tns:PageNumber>{subscriberList.PageNumber}</tns:PageNumber>");
            soap.AppendLine($"      <tns:PageSize>{subscriberList.PageSize}</tns:PageSize>");
            soap.AppendLine("      <tns:Subscribers>");
            
            foreach (var subscriber in subscriberList.Subscribers)
            {
                soap.AppendLine("        <tns:Subscriber>");
                soap.AppendLine($"          <tns:Id>{subscriber.Id}</tns:Id>");
                soap.AppendLine($"          <tns:Username>{XmlEscape(subscriber.Username)}</tns:Username>");
                soap.AppendLine($"          <tns:Email>{XmlEscape(subscriber.Email)}</tns:Email>");
                soap.AppendLine($"          <tns:Name>{XmlEscape(subscriber.Name)}</tns:Name>");
                soap.AppendLine($"          <tns:PhoneNumber>{XmlEscape(subscriber.PhoneNumber)}</tns:PhoneNumber>");
                soap.AppendLine($"          <tns:PlanType>{XmlEscape(subscriber.PlanType)}</tns:PlanType>");
                soap.AppendLine($"          <tns:Country>{XmlEscape(subscriber.Country)}</tns:Country>");
                soap.AppendLine($"          <tns:IsRoaming>{subscriber.IsRoaming.ToString().ToLower()}</tns:IsRoaming>");
                soap.AppendLine($"          <tns:IsActive>{subscriber.IsActive.ToString().ToLower()}</tns:IsActive>");
                soap.AppendLine($"          <tns:CreatedAt>{subscriber.CreatedAt:yyyy-MM-ddTHH:mm:ssZ}</tns:CreatedAt>");
                soap.AppendLine($"          <tns:LastUpdated>{subscriber.LastUpdated:yyyy-MM-ddTHH:mm:ssZ}</tns:LastUpdated>");
                soap.AppendLine($"          <tns:YearsActive>{subscriber.YearsActive}</tns:YearsActive>");
                soap.AppendLine($"          <tns:IsLoyaltyEligible>{subscriber.IsLoyaltyEligible.ToString().ToLower()}</tns:IsLoyaltyEligible>");
                soap.AppendLine("        </tns:Subscriber>");
            }
            
            soap.AppendLine("      </tns:Subscribers>");
            soap.AppendLine("    </tns:SubscriberListResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateTopConsumersSoapXml(TopConsumersResponse topConsumers)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:TopConsumersResponse>");
            soap.AppendLine($"      <tns:Month>{topConsumers.Month}</tns:Month>");
            soap.AppendLine($"      <tns:SortBy>{XmlEscape(topConsumers.SortBy)}</tns:SortBy>");
            soap.AppendLine("      <tns:TopConsumers>");
            
            foreach (var consumer in topConsumers.TopConsumers)
            {
                soap.AppendLine("        <tns:TopConsumer>");
                soap.AppendLine($"          <tns:UserId>{consumer.UserId}</tns:UserId>");
                soap.AppendLine($"          <tns:UserName>{XmlEscape(consumer.UserName)}</tns:UserName>");
                soap.AppendLine($"          <tns:PhoneNumber>{XmlEscape(consumer.PhoneNumber)}</tns:PhoneNumber>");
                soap.AppendLine($"          <tns:PlanType>{XmlEscape(consumer.PlanType)}</tns:PlanType>");
                soap.AppendLine($"          <tns:TotalCallMinutes>{consumer.TotalCallMinutes}</tns:TotalCallMinutes>");
                soap.AppendLine($"          <tns:TotalDataMB>{consumer.TotalDataMB}</tns:TotalDataMB>");
                soap.AppendLine($"          <tns:TotalSMSCount>{consumer.TotalSMSCount}</tns:TotalSMSCount>");
                soap.AppendLine($"          <tns:TotalCost>{consumer.TotalCost:F2}</tns:TotalCost>");
                soap.AppendLine($"          <tns:Rank>{consumer.Rank}</tns:Rank>");
                soap.AppendLine("        </tns:TopConsumer>");
            }
            
            soap.AppendLine("      </tns:TopConsumers>");
            soap.AppendLine("    </tns:TopConsumersResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateUsageStatisticsSoapXml(UsageStatisticsResponse usageStats)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:UsageStatisticsResponse>");
            soap.AppendLine("      <tns:UsageStatistics>");
            soap.AppendLine($"        <tns:Month>{usageStats.Month}</tns:Month>");
            soap.AppendLine($"        <tns:TotalSubscribers>{usageStats.TotalSubscribers}</tns:TotalSubscribers>");
            soap.AppendLine($"        <tns:TotalCallMinutes>{usageStats.TotalCallMinutes}</tns:TotalCallMinutes>");
            soap.AppendLine($"        <tns:TotalDataMB>{usageStats.TotalDataMB}</tns:TotalDataMB>");
            soap.AppendLine($"        <tns:TotalSMSCount>{usageStats.TotalSMSCount}</tns:TotalSMSCount>");
            soap.AppendLine($"        <tns:PeakTimeMinutes>{usageStats.PeakTimeMinutes}</tns:PeakTimeMinutes>");
            soap.AppendLine($"        <tns:OffPeakTimeMinutes>{usageStats.OffPeakTimeMinutes}</tns:OffPeakTimeMinutes>");
            soap.AppendLine($"        <tns:RoamingMinutes>{usageStats.RoamingMinutes}</tns:RoamingMinutes>");
            soap.AppendLine($"        <tns:RoamingDataMB>{usageStats.RoamingDataMB}</tns:RoamingDataMB>");
            soap.AppendLine($"        <tns:RoamingSMSCount>{usageStats.RoamingSMSCount}</tns:RoamingSMSCount>");
            soap.AppendLine($"        <tns:AverageCallMinutesPerUser>{usageStats.AverageCallMinutesPerUser:F2}</tns:AverageCallMinutesPerUser>");
            soap.AppendLine($"        <tns:AverageDataMBPerUser>{usageStats.AverageDataMBPerUser:F2}</tns:AverageDataMBPerUser>");
            soap.AppendLine($"        <tns:AverageSMSCountPerUser>{usageStats.AverageSMSCountPerUser:F2}</tns:AverageSMSCountPerUser>");
            soap.AppendLine("      </tns:UsageStatistics>");
            soap.AppendLine("    </tns:UsageStatisticsResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string GenerateRevenueStatisticsSoapXml(RevenueStatisticsResponse revenueStats)
        {
            var soap = new StringBuilder();
            soap.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soap.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns=\"http://tempuri.org/\">");
            soap.AppendLine("  <soap:Body>");
            soap.AppendLine("    <tns:RevenueStatisticsResponse>");
            soap.AppendLine("      <tns:RevenueStatistics>");
            soap.AppendLine($"        <tns:Month>{revenueStats.Month}</tns:Month>");
            soap.AppendLine($"        <tns:TotalRevenue>{revenueStats.TotalRevenue:F2}</tns:TotalRevenue>");
            soap.AppendLine($"        <tns:VoiceRevenue>{revenueStats.VoiceRevenue:F2}</tns:VoiceRevenue>");
            soap.AppendLine($"        <tns:DataRevenue>{revenueStats.DataRevenue:F2}</tns:DataRevenue>");
            soap.AppendLine($"        <tns:SMSRevenue>{revenueStats.SMSRevenue:F2}</tns:SMSRevenue>");
            soap.AppendLine($"        <tns:RoamingRevenue>{revenueStats.RoamingRevenue:F2}</tns:RoamingRevenue>");
            soap.AppendLine($"        <tns:VATAmount>{revenueStats.VATAmount:F2}</tns:VATAmount>");
            soap.AppendLine($"        <tns:LoyaltyDiscountAmount>{revenueStats.LoyaltyDiscountAmount:F2}</tns:LoyaltyDiscountAmount>");
            soap.AppendLine($"        <tns:TotalBillsGenerated>{revenueStats.TotalBillsGenerated}</tns:TotalBillsGenerated>");
            soap.AppendLine($"        <tns:ActiveSubscribers>{revenueStats.ActiveSubscribers}</tns:ActiveSubscribers>");
            soap.AppendLine($"        <tns:AverageRevenuePerSubscriber>{revenueStats.AverageRevenuePerSubscriber:F2}</tns:AverageRevenuePerSubscriber>");
            
            if (revenueStats.RevenueByPlanType.Any())
            {
                soap.AppendLine("        <tns:RevenueByPlanType>");
                foreach (var planType in revenueStats.RevenueByPlanType)
                {
                    soap.AppendLine("          <tns:PlanTypeRevenue>");
                    soap.AppendLine($"            <tns:PlanType>{XmlEscape(planType.PlanType)}</tns:PlanType>");
                    soap.AppendLine($"            <tns:SubscriberCount>{planType.SubscriberCount}</tns:SubscriberCount>");
                    soap.AppendLine($"            <tns:TotalRevenue>{planType.TotalRevenue:F2}</tns:TotalRevenue>");
                    soap.AppendLine($"            <tns:AverageRevenuePerSubscriber>{planType.AverageRevenuePerSubscriber:F2}</tns:AverageRevenuePerSubscriber>");
                    soap.AppendLine("          </tns:PlanTypeRevenue>");
                }
                soap.AppendLine("        </tns:RevenueByPlanType>");
            }
            
            soap.AppendLine("      </tns:RevenueStatistics>");
            soap.AppendLine("    </tns:RevenueStatisticsResponse>");
            soap.AppendLine("  </soap:Body>");
            soap.AppendLine("</soap:Envelope>");

            return soap.ToString();
        }

        private string XmlEscape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&apos;");
        }
    }
}
