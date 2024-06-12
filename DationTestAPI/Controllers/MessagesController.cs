using Azure.Messaging.ServiceBus;
using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("[controller]")]
public class MessagesController(IConfiguration configuration) : ControllerBase
{
    private readonly string _connectionString = configuration["ServiceBus:ConnectionString"];
    private readonly string _queueName = configuration["ServiceBus:QueueName"];

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Message message)
    {
        try
        {            
            message.ProcessDate = DateTime.UtcNow;
            message.AuthorName = "Cristian Gantiva";

            
            message.InsurancePayment = new InsurancePayment
            {
                PaymentId = 12345,
                PaymentDatetime = DateTime.UtcNow,
                Franchise = "Visa",
                Currency = "COP",
                Amount = 1500000
            };            
            await using var client = new ServiceBusClient(_connectionString);            

            ServiceBusSender sender = client.CreateSender(_queueName);
            
            string messageBody = JsonSerializer.Serialize(message);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(serviceBusMessage);

            return Ok(new { response = true, msg = "" });
        }
        catch 
        {            
            return StatusCode(500, new { response = false, msg = "error in the queue" });
        }
    }
}
