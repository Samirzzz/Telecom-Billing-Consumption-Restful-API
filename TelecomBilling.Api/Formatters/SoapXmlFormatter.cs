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
            return context.ObjectType == typeof(BillingResponse) || 
                   context.ObjectType == typeof(BillingListResponse) ||
                   base.CanWriteResult(context);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            if (context.Object is BillingResponse billing)
            {
                buffer.Append(GenerateSoapXml(billing));
            }
            else if (context.Object is BillingListResponse billingList)
            {
                buffer.Append(GenerateSoapXmlList(billingList));
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
