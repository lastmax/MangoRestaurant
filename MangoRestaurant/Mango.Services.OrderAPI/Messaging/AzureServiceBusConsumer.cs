using AutoMapper;
using Azure.Messaging.ServiceBus;
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
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        private readonly ServiceBusProcessor _checkoutProcessor;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _mapper = mapper;

            _serviceBusConnectionString = _configuration.GetValue<string>("ConnectionStrings:AzureServiceBus");
            _subscriptionOrder = _configuration.GetValue<string>("AzureSubscriptions:Orders");
            _checkoutMessageTopic = _configuration.GetValue<string>("AzureTopics:Checkout");

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
        }
    }
}
