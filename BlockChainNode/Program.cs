using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using BlockChainNode.Lib.Logging;
using BlockChainNode.Lib.Modules;
using BlockChainNode.Lib.Net;
using Nancy;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace BlockChainNode
{
    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
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
                var hosts = NodeBalance.RegisterSelfAt(target);
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

            var hostConfigs = new HostConfiguration
            {
                UrlReservations = new UrlReservations {CreateAutomatically = true}
            };
            var host = new NancyHost(localUri, new DefaultNancyBootstrapper(), hostConfigs);

            host.Start();

            Logger.Log.Debug("Синхронизация текущей цепочки блоков...");
            NodeBalance.RebalanceSelf();
            Logger.Log.Debug($"Успешно! В цепи: {OperationModule.Machine.Chain.Count} блоков.");

            Logger.Log.Info("Узел запущен. Нажмите ESC для остановки.");

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }
            }

            Logger.Log.Info("Узел останавливается...");
            host.Stop();
            Logger.Log.Info("Узел остановлен. Нажмите любую клавишу для выхода");
            Console.ReadKey(true);
        }
    }
}