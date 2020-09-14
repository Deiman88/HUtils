using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace HUtils
{
    public class RabbitMaster
    {
        public static void Send(string queue, string args, string? comm)
        {
            if (args != null)
            {
                var factory = new ConnectionFactory()
                {
#if DEBUG
                    HostName = "localhost"
#else                   
                    HostName = "srv-captain--rabbit",
                    UserName = "rabbitUser",
                    Password = "m2Z5xMp6S8"
#endif
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    
                    var logg = "to queue "+queue+" "+comm;
                    var body = Encoding.UTF8.GetBytes(args.ToString());

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                        routingKey: queue,
                        basicProperties: properties,
                        body: body);

                    //Console.WriteLine(" [x] Sent {0}", logg);
                }
            }
        }
    }
}
