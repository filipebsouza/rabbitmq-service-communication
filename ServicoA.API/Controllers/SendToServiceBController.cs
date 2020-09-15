using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ServicoA.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendToServicoBController : ControllerBase
    {
        public SendToServicoBController() { }

        [HttpPost]
        public async Task<ActionResult> Send()
        {
            return await Task<ActionResult>.Run(() =>
            {
                var objeto = new WeatherForecast
                {
                    Date = DateTime.Now.Date,
                    TemperatureC = 50,
                    Summary = "Sei la"
                };

                var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "ServicoB_Queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var json = JsonConvert.SerializeObject(objeto);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: "", routingKey: "ServicoB_Queue", basicProperties: null, body: body);
                }

                return Ok(objeto);
            });

        }
    }
}