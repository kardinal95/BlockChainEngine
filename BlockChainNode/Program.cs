using System;
using System.Collections.Generic;
using System.Configuration;
using BlockChainNode.Lib.Logging;
using BlockChainNode.Modules;
using BlockChainNode.Net;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace BlockChainNode
{
    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Uri localUri = null;

            Logger.Init();
            Logger.Log.Info("Запуск программы...");

            Logger.Log.Info($"Привязка к адресу {ConfigurationManager.AppSettings["host"]}...");
            try
            {
                localUri = new Uri(ConfigurationManager.AppSettings["host"]);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is UriFormatException)
            {
                Logger.Log.Fatal("Указанный адрес неверен!");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Logger.Log.Info("Успешно!");
            NodeBalance.NodeSet = new HashSet<string> {localUri.ToString()};

            Logger.Log.Info("Производится первоначальная настройка...");
            try
            {
                Logger.Log.Debug($"Соединение с {ConfigurationManager.AppSettings["target"]}...");
                var target = ConfigurationManager.AppSettings["target"];
                var hosts = NodeBalance.RegisterLocalAtNode(target, localUri.ToString());
                NodeBalance.NodeSet = JsonConvert.DeserializeObject<HashSet<string>>(hosts);
                Logger.Log.Debug($"Получено {NodeBalance.NodeSet.Count} узлов!");
            }
            catch (ApplicationException exc)
            {
                Logger.Log.Fatal(exc.Message, exc);
                Console.ReadKey();
                Environment.Exit(1);
            }
            catch (InvalidOperationException)
            {
                Logger.Log.Warn("Соединение с узлами не установлено!");
                Logger.Log.Warn("Игнорируйте это предупреждение если узел единственный.");
            }

            Logger.Log.Info("Настройка завершена.");

            var host = new NancyHost(localUri);
            var running = true;

            host.Start();

            Logger.Log.Debug("Синхронизация текущей цепочки блоков...");
            NodeBalance.SyncNode(ConfigurationManager.AppSettings["host"]);
            Logger.Log.Debug($"Успешно! В цепи: {OperationModule.Machine.Chain.Count} блоков.");

            Logger.Log.Info("Узел запущен. Для остановки введите exit или quit.");

            while (running is true)
            {
                var input = Console.ReadLine();
                if (input == "exit" || input == "quit")
                {
                    running = false;
                }
            }

            host.Stop();
        }
    }
}