using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using STELA_AUTH.Core.Entities.Request;
using STELA_AUTH.Core.IRepository;

namespace STELA_AUTH.Infrastructure.Service
{
    public class RabbitMqUpdateProfileBackgroundService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqUpdateProfileBackgroundService(
            IServiceScopeFactory serviceFactory,
            string hostname,
            string queueName,
            string userName,
            string password)
        {
            _hostname = hostname;
            _queueName = queueName;
            _userName = userName;
            _password = password;
            _serviceFactory = serviceFactory;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _userName,
                Password = _password,
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await HandleMessageAsync(message);
            };

            _channel.BasicConsume(
                queue: _queueName,
                autoAck: true,
                consumer: consumer);

            await Task.CompletedTask;
        }

        private async Task<UpdateProfileBody?> HandleMessageAsync(string message)
        {
            using var scope = _serviceFactory.CreateScope();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            var updateProfileBody = JsonSerializer.Deserialize<UpdateProfileBody>(message);
            if (updateProfileBody == null)
                return null;

            var account = await accountRepository.UpdateImage(updateProfileBody.AccountId, updateProfileBody.FileName);
            if (account == null)
                return null;

            return updateProfileBody;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}