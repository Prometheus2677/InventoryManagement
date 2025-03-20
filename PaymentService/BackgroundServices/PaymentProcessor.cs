using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using PaymentService.Data;
using Microsoft.Extensions.Hosting;

namespace PaymentService.BackgroundServices
{
    public class PaymentProcessor : BackgroundService
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _orderTopic = "orders";
        private readonly string _paymentTopic = "payments";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "payment-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(_orderTopic);

            using var producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = _bootstrapServers }).Build();

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);
                var orderEvent = JsonSerializer.Deserialize<OrderEvent>(consumeResult.Value);

                var paymentJson = JsonSerializer.Serialize(new { orderEvent.OrderId, Status = "Completed" });
                await producer.ProduceAsync(_paymentTopic, new Message<string, string> { Key = orderEvent.OrderId.ToString(), Value = paymentJson });

                Console.WriteLine($"✅ Payment completed for Order ID: {orderEvent.OrderId}");
            }
        }
    }

    public class OrderEvent
    {
        public Guid OrderId { get; set; }
    }
}
