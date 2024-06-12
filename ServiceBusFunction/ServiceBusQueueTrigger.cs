using Azure.Messaging.ServiceBus;
using Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;


public class ServiceBusQueueTrigger
{
    private readonly string _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
    private readonly string _queueName = "queue1";
    private readonly string _dbConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

    [FunctionName("ProcessServiceBusQueueMessage")]
    public async Task Run([ServiceBusTrigger("queue1", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ILogger log)
    {
        

        try
        {
            var messageBody = message.Body.ToString();
            var messageData = JsonSerializer.Deserialize<Message>(messageBody);

            if (messageData.AuthorName != "Cristian Gantiva")
            {
                log.LogInformation("Message does not contain the required author name.");
                return;
            }

            var xmlMessage = new XElement("Message",
                new XElement("Id", messageData.Id),
                new XElement("Name", messageData.Name),
                new XElement("Surname", messageData.Surname),
                new XElement("ProcessDate", messageData.ProcessDate),
                new XElement("AuthorName", messageData.AuthorName),
                new XElement("InsurancePayment",
                    new XElement("PaymentId", messageData.InsurancePayment.PaymentId),
                    new XElement("PaymentDatetime", messageData.InsurancePayment.PaymentDatetime),
                    new XElement("Franchise", messageData.InsurancePayment.Franchise),
                    new XElement("Currency", messageData.InsurancePayment.Currency),
                    new XElement("Amount", messageData.InsurancePayment.Amount)
                )
            );

            using (var connection = new SqlConnection(_dbConnectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("INSERT INTO DataMessage (XmlContent) VALUES (@XmlContent)", connection);
                command.Parameters.AddWithValue("@XmlContent", xmlMessage.ToString());

                await command.ExecuteNonQueryAsync();
                log.LogInformation("Message successfully stored in the database.");
            }

            var client = new ServiceBusClient(_connectionString);
            var receiver = client.CreateReceiver(_queueName);
            var receivedMessage = await receiver.ReceiveMessageAsync();

            await receiver.CompleteMessageAsync(receivedMessage);
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing message: {ex.Message}");
            File.AppendAllText("error.log", $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
        }
    }


}
