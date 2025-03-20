using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "orders";

        public OrdersController(OrderDbContext context)
        {
            _context = context;
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            order.Id = Guid.NewGuid();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var config = new ProducerConfig { BootstrapServers = _bootstrapServers };
            using var producer = new ProducerBuilder<string, string>(config).Build();
            var orderJson = JsonSerializer.Serialize(new { order.Id });

            await producer.ProduceAsync(_topic, new Message<string, string> { Key = order.Id.ToString(), Value = orderJson });

            return Ok(new { Message = "Order created", Order = order });
        }
    }
}
