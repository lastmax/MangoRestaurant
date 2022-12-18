using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionOrder;
        private readonly string _checkoutMessageTopic;
        private readonly string _orderPaymentProcessTopic;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly string _checkoutQueue;
        
        private readonly EmailRepository _orderRepository;
        
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly IMapper _mapper;

        private ServiceBusProcessor _checkoutProcessor;
        private ServiceBusProcessor _orderUpdatePaymentStatusProcessor;

        public AzureServiceBusConsumer(EmailRepository orderRepository, IConfiguration configuration, IMessageBus messageBus, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _mapper = mapper;
            _messageBus = messageBus;

            _serviceBusConnectionString = _configuration.GetValue<string>("ConnectionStrings:AzureServiceBus");
            _orderPaymentProcessTopic = _configuration.GetValue<string>("AzureTopics:OrderPaymentProcess");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("AzureTopics:OrderUpdatePaymentResult");
            _checkoutMessageTopic = _configuration.GetValue<string>("AzureTopics:Checkout");
            _subscriptionOrder = _configuration.GetValue<string>("AzureSubscriptions:Orders");
            _checkoutQueue = _configuration.GetValue<string>("AzureQueues:Checkout");

            var serviceBusClient = new ServiceBusClient(_serviceBusConnectionString);
            _checkoutProcessor = serviceBusClient.CreateProcessor(_checkoutQueue);
            _orderUpdatePaymentStatusProcessor = serviceBusClient.CreateProcessor(_orderUpdatePaymentResultTopic, _subscriptionOrder);
        }

        public async Task Start()
        {
            _checkoutProcessor.ProcessMessageAsync += OnCheckoutMessageReceived;
            _checkoutProcessor.ProcessErrorAsync += ErrorHandler;
            await _checkoutProcessor.StartProcessingAsync();

            _orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            _orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _checkoutProcessor.StopProcessingAsync();
            await _checkoutProcessor.DisposeAsync();

            await _orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await _orderUpdatePaymentStatusProcessor.DisposeAsync();
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
                OrderTotal = mappedHeader.OrderTotal,
                Email = mappedHeader.Email
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

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            var paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            await _orderRepository.UpdateOrderPaymentStatus(paymentResultMessage.OrderId, paymentResultMessage.Status);
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
