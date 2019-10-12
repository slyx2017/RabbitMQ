using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入1or其他(1:send;Other:receive)");
            string type = Console.ReadLine();
            if (type == "1")
            {
                var factory = new ConnectionFactory();
                factory.HostName = "127.0.0.1";
                factory.Port = 5672;
                using (var conn = factory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        
                        //在MQ上定义一个持久化队列，如果名称相同不会重复创建
                        channel.QueueDeclare("MyRabbitMQ", true, false, false, null);
                        Console.WriteLine("请输入内容...");
                        while (true)
                        {
                            //传递的消息内容
                            string message = string.Format("Message_{0}", Console.ReadLine());
                            var body = Encoding.UTF8.GetBytes(message);
                            var properties = channel.CreateBasicProperties();
                            properties.DeliveryMode = 2;

                            //开始传递
                            channel.BasicPublish("", "MyRabbitMQ", properties, body);
                            Console.WriteLine("消息发送成功：{0}", message);
                        }
                    }
                }
            }
            else
            {
                var factory = new ConnectionFactory();
                factory.HostName = "127.0.0.1";
                factory.Port = 5672;
                using (var conn = factory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        //在MQ上定义一个持久化队列，如果名称相同不会重复创建
                        channel.QueueDeclare("MyRabbitMQ", true, false, false, null);

                        //输入1，那如果接收一个消息，但是没有应答，则客户端不会收到下一个消息
                        channel.BasicQos(0, 1, false);

                        Console.WriteLine("Listening...");

                        //在队列上定义一个消费者
                        var consumer = new QueueingBasicConsumer(channel);
                        channel.BasicConsume("MyRabbitMQ", false, consumer);

                        while (true)
                        {
                            var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                            var body = ea.Body;
                            string message = Encoding.UTF8.GetString(body);

                            Console.WriteLine("消息接收成功:" + message);

                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                    }

                    //var factory = new ConnectionFactory();
                    //factory.HostName = "127.0.0.1";
                    //factory.UserName = "guest";
                    //factory.Password = "guest";

                    //using (var conn=factory.CreateConnection())
                    //{
                    //    using (var channel=conn.CreateModel())
                    //    {
                    //        //创建一个名称为hello的消息队列
                    //        channel.QueueDeclare("hello",false, false, false, null);

                    //        //传递的消息内容
                    //        string message = "Hello World";
                    //        var body = Encoding.UTF8.GetBytes(message);

                    //        //开始传递
                    //        channel.BasicPublish("", "hello", null, body);
                    //        Console.WriteLine("已发送：{0}", message);
                    //        Console.ReadLine();
                    //    }
                    //}
                }
            }
        }
    }
}
