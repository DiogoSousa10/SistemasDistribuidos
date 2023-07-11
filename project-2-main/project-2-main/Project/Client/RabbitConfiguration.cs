using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using IModel = RabbitMQ.Client.IModel;

namespace GrpcServer
{
    public class RabbitConfiguration
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private string _defaultExchangeName;
        private string _defaultRoutingKey;

        public RabbitConfiguration(string exchangeName, string routingKey)
        {
            _defaultExchangeName = exchangeName;
            _defaultRoutingKey = routingKey;

            _connection = CreateConnection();
            _channel = CreateChannel(_connection);
            SetupExchange(_channel, _defaultExchangeName);
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

        public void ConsumeMessages(Action<string> messageAction, string queueName)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                messageAction(message);
            };
            _channel.BasicConsume(queueName, true, consumer);
        }

        public void ListQueueMessages(string queueName)
        {
            var messageCount = GetMessageCount(queueName);
            Console.WriteLine($"Número de mensagens na fila: {messageCount}");

            if (messageCount > 0)
            {
                Console.WriteLine("Mensagens na fila:");

                BasicGetResult message;
                while ((message = _channel.BasicGet(queueName, autoAck: true)) != null)
                {
                    var messageBody = Encoding.UTF8.GetString(message.Body.ToArray());
                    Console.WriteLine("-" + messageBody);
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Não há mensagens na fila.");
            }
        }

        private int GetMessageCount(string queueName)
        {
            var queueInfo = _channel.QueueDeclarePassive(queueName);
            return (int)queueInfo.MessageCount;
        }
    }
}