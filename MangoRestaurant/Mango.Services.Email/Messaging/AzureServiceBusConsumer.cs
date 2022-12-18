using Azure.Messaging.ServiceBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionEmail;
        private readonly string _orderUpdatePaymentResultTopic;
        
        private readonly EmailRepository _emailRepository;
        
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor _orderUpdatePaymentStatusProcessor;

        public AzureServiceBusConsumer(EmailRepository emailRepository, IConfiguration configuration)
        {
            _emailRepository = emailRepository;
            _configuration = configuration;

            _serviceBusConnectionString = _configuration.GetValue<string>("ConnectionStrings:AzureServiceBus");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("AzureTopics:OrderUpdatePaymentResult");
            _subscriptionEmail = _configuration.GetValue<string>("AzureSubscriptions:Email");

            var serviceBusClient = new ServiceBusClient(_serviceBusConnectionString);
            _orderUpdatePaymentStatusProcessor = serviceBusClient.CreateProcessor(_orderUpdatePaymentResultTopic, _subscriptionEmail);
        }

        public async Task Start()
        {
            _orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            _orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await _orderUpdatePaymentStatusProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            var paymentResult = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            try
            {
                await _emailRepository.SendAndLogEmail(paymentResult);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
