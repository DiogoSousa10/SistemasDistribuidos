using RabbitMQ.Client;
using System.Text;
using IModel = RabbitMQ.Client.IModel;

namespace GrpcServer
{
    public class RabbitConfiguration
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _defaultExchangeName;
        private readonly string _defaultRoutingKey;

        public RabbitConfiguration(string defaultExchangeName, string defaultRoutingKey)
        {
            _defaultExchangeName = defaultExchangeName;
            _defaultRoutingKey = defaultRoutingKey;
            _connection = CreateConnection();
            _channel = CreateChannel(_connection);
            SetupExchange(_channel, _defaultExchangeName);
        }

        public void PublishMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish("", queueName, null, body);
            Thread.Sleep(20000);
            Console.WriteLine("Mensagem enviada para o tópico: " + queueName);
        }

        public async Task PublishMessageAsync(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await Task.Run(() =>
            {
                _channel.BasicPublish("", queueName, null, body);
                //Thread.Sleep(2000);
            });
            Console.WriteLine("Mensagem enviada para o tópico: " + queueName);
        }

        public void CloseConnection()
        {
            _channel.Close();
            _connection.Close();
        }

        private IConnection CreateConnection()
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://25.43.242.182:5672");
            factory.ClientProvidedName = "Rabbit Sender App";
            return factory.CreateConnection();
        }

        private IModel CreateChannel(IConnection connection)
        {
            return connection.CreateModel();
        }

        private void SetupExchange(IModel channel, string exchangeName)
        {
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        }

        public void SetupQueue(string queueName)
        {
            _channel.QueueDeclare(queueName, false, false, false, null);
        }

        public void BindQueue(string queueName)
        {
            _channel.QueueBind(queueName, _defaultExchangeName, _defaultRoutingKey);
        }
    }
}