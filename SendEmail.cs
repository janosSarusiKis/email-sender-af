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
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] IAsyncCollector<SendGridMessage> messageCollector)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var form = JsonConvert.DeserializeObject<Form>(requestBody);

            var targetEmailAddress = System.Environment.GetEnvironmentVariable("TargetEmailAddress");
            if (string.IsNullOrEmpty(targetEmailAddress))
            {
                targetEmailAddress = "hello@test.io";
            }

            var message = new SendGridMessage();
            message.AddTo(new EmailAddress(targetEmailAddress));
            message.AddContent("text/plain", form.Content);
            message.SetFrom(new EmailAddress(form.From));
            message.SetSubject($"Test website form submitted by {form.Name}");

            await messageCollector.AddAsync(message);

            var responseMessage = new SendGridMessage();
            responseMessage.AddTo(new EmailAddress(form.From));
            responseMessage.SetFrom(new EmailAddress(targetEmailAddress));
            responseMessage.SetSubject($"We received your message");

            responseMessage.AddContent("text/plain", $"Hello {form.Name},\n \nThis is just a quick note to confirm that we successfully received your message. \nWe look forward to talking to you soon! \n \nCheers, \nThe Team");

            await messageCollector.AddAsync(responseMessage);

            return new OkObjectResult("Emails sent.");
        }
    }
}
