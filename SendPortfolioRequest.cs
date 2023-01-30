using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using System.IO;
using System.Threading.Tasks;

namespace emailsender
{
    public class SendPortfolioRequest
    {
        [FunctionName("SendPortfolieEmail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendEmail/portfolio")] HttpRequest req,
            ILogger log,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // requestBody contains email
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var form = JsonConvert.DeserializeObject<Form>(requestBody);

            var targetEmailAddress = System.Environment.GetEnvironmentVariable("TargetEmailAddress");
            if (string.IsNullOrEmpty(targetEmailAddress))
            {
                targetEmailAddress = "hello@test.io";
            }

            var message = new SendGridMessage();
            message.AddTo(new EmailAddress(targetEmailAddress));
            message.SetFrom(new EmailAddress(form.From));
            message.SetSubject($"Portfolio Request received!");
            message.AddContent("text/plain", $"Hi there,\n \nThank for your interest in our portfolio!\nWe successfully received your request, and will get back to you shortly.\n \nBest wishes,\nThe Team");

            await messageCollector.AddAsync(message);

            var responseMessage = new SendGridMessage();
            responseMessage.AddTo(new EmailAddress(form.From));
            responseMessage.SetFrom(new EmailAddress(targetEmailAddress));
            responseMessage.SetSubject($"Portfolio Request received!");
            responseMessage.AddContent("text/plain", $"Hi there,\n \nThank for your interest in our portfolio!\nWe successfully received your request, and will get back to you shortly.\n \nBest wishes,\nThe Team");

            await messageCollector.AddAsync(responseMessage);

            return new OkObjectResult("Emails sent.");
        }
    }
}
