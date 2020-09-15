using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using System.Text;

namespace ServicoB.API.Messaging
{
    public class ReceiverServiceBQueue : BackgroundService
    {
        private IModel _channel;
        private IConnection _connection;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly string _password;

        public ReceiverServiceBQueue(IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {
            _hostname = rabbitMqOptions.Value.Hostname;
            _queueName = rabbitMqOptions.Value.QueueName;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;
            InitializeRabbitMqListener();
        }

        private void InitializeRabbitMqListener()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                RequestedHeartbeat = new System.TimeSpan(8)
            };

            _connection = factory.CreateConnection();
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                //var updateCustomerFullNameModel = JsonConvert.DeserializeObject<UpdateCustomerFullNameModel>(content);

                //HandleMessage(updateCustomerFullNameModel);

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_queueName, false, consumer);

            return Task.CompletedTask;
        }
        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}