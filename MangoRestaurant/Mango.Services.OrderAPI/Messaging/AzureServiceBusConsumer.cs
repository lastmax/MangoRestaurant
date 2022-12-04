using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionOrder;
        private readonly string _checkoutMessageTopic;
        private readonly string _orderPaymentProcessTopic;
        
        private readonly OrderRepository _orderRepository;
        
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly IMapper _mapper;

        private readonly ServiceBusProcessor _checkoutProcessor;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration, IMessageBus messageBus, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _mapper = mapper;
            _messageBus = messageBus;

            _serviceBusConnectionString = _configuration.GetValue<string>("ConnectionStrings:AzureServiceBus");
            _orderPaymentProcessTopic = _configuration.GetValue<string>("AzureTopics:OrderPaymentProcess");
            _checkoutMessageTopic = _configuration.GetValue<string>("AzureTopics:Checkout");
            _subscriptionOrder = _configuration.GetValue<string>("AzureSubscriptions:Orders");

            _checkoutProcessor = new ServiceBusClient(_serviceBusConnectionString)
                .CreateProcessor(_checkoutMessageTopic, _subscriptionOrder);
        }

        public async Task Start()
        {
            _checkoutProcessor.ProcessMessageAsync += OnCheckoutMessageReceived;
            _checkoutProcessor.ProcessErrorAsync += ErrorHandler;
            await _checkoutProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _checkoutProcessor.StopProcessingAsync();
            await _checkoutProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnCheckoutMessageReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            var checkoutHeaderDTO = JsonConvert.DeserializeObject<CheckoutHeaderDTO>(body);
            var mappedHeader = _mapper.Map<OrderHeader>(checkoutHeaderDTO);

            await _orderRepository.AddOrder(mappedHeader);

            var paymentRequestMessage = new PaymentRequestMessage()
            {
                Name = $"{mappedHeader.FirstName} {mappedHeader.LastName}",
                CardNumber = mappedHeader.CardNumber,
                CVV = mappedHeader.CVV,
                ExpiryMonthYear = mappedHeader.ExpiryMonthYear,
                OrderId = mappedHeader.OrderHeaderId,
                OrderTotal = mappedHeader.OrderTotal
            };

            try
            {
                await _messageBus.PublishMessage(paymentRequestMessage, _orderPaymentProcessTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
