using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HUtils
{
    public static class RabbitSlave
    {
        public static void Recive(string queue, 
                                  Func<string, Task> task
                                 )
        {
            var cf = new ConnectionFactory{ 
#if DEBUG
                HostName = "localhost"
#else 
                HostName = "srv-captain--rabbit",
                UserName = "rabbitUser",
                Password = "m2Z5xMp6S8"
#endif
                ,DispatchConsumersAsync = true };
            using(var conn = cf.CreateConnection())
            using(var model = conn.CreateModel())
            {
                model.QueueDeclare(queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                var consumer = new AsyncEventingBasicConsumer(model);
                //var tag = model.BasicConsume(queue, false, consumer);
                consumer.Received += async (o, ea) =>
                {
                    string comment = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                    if (ea != null)
                    {
                        Task.Run(async () =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            await Task.Run(() => task(message));

                            model.BasicAck(ea.DeliveryTag, false);

                            //Console.WriteLine(comment + " [x] Received {0}", message);
                            Console.WriteLine(comment + " [x] Received");
                            Console.WriteLine(comment + " Delivery: " + ea.DeliveryTag);
                            await Task.Yield();
                        });
                    }
                    if (ea == null)
                    {
                        Console.WriteLine(comment + " [x] No Message ");
                        await Task.Yield();
                    }
                };
                model.BasicConsume(queue: queue,
                    autoAck: false,
                    consumer: consumer);
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
