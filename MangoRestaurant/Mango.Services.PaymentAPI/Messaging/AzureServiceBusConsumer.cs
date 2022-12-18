using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using Newtonsoft.Json;
using PaymentProcessor;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionPayment;
        private readonly string _orderPaymentProcessTopic;
        private readonly string _orderUpdatePaymentResultTopic;
        
        private readonly IProcessPayment _processPayment;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;

        private ServiceBusProcessor _orderPaymentProcessor;

        public AzureServiceBusConsumer(IProcessPayment processPayment, IConfiguration configuration, IMessageBus messageBus)
        {
            _processPayment = processPayment;
            _configuration = configuration;
            _messageBus = messageBus;

            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopics");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("orderupdatepaymentresulttopic");
            _subscriptionPayment = _configuration.GetValue<string>("OrderPaymentProcessSub");

            _orderPaymentProcessor = new ServiceBusClient(_serviceBusConnectionString)
                .CreateProcessor(_orderPaymentProcessTopic, _subscriptionPayment);
        }

        public async Task Start()
        {
            _orderPaymentProcessor.ProcessMessageAsync += ProcessPayments;
            _orderPaymentProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderPaymentProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _orderPaymentProcessor.StopProcessingAsync();
            await _orderPaymentProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task ProcessPayments(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            var paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);

            var paymentResult = _processPayment.PaymentProcessor();
            var updatedPaymentResultMessage = new UpdatePaymentResultMessage()
            {
                Status = paymentResult,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email
            };

            try
            {
                await _messageBus.PublishMessage(updatedPaymentResultMessage, _orderUpdatePaymentResultTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
