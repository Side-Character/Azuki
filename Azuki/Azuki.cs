using AzukiModuleApi;
using Discore;
using Discore.Http;
using Discore.WebSocket;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chris {
    public class Azuki {
        internal static readonly Dictionary<string, Tuple<BaseModule, MethodInfo>> AdminCommands = new Dictionary<string, Tuple<BaseModule, MethodInfo>>();
        internal static Config config;
        internal static List<Snowflake> Admins;
        internal static ManualResetEvent stopsignal = new ManualResetEvent(false);
        private readonly ILog log;
        private readonly ILog discorelog;
        private readonly List<CoreHandler> handlers = new List<CoreHandler>();
        private readonly Dictionary<string, Tuple<BaseModule, MethodInfo>> Commands = new Dictionary<string, Tuple<BaseModule, MethodInfo>>();
        static Azuki() {
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            try {
                using (StreamReader reader = new StreamReader("Config/Main.conf.xml")) {
                    config = serializer.Deserialize(reader) as Config;
                }
                Admins = config.Admins;
                /*using (StreamWriter writer = new StreamWriter("Config/Main.conf.xml")) {
                    serializer.Serialize(writer, config);
                }*/
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }
        public Azuki() {
            ILoggerRepository repository = LogManager.CreateRepository("Azuki");
            XmlConfigurator.ConfigureAndWatch(repository, new FileInfo("Config/Log.conf.xml"));

            if (repository.GetAppenders().Length == 0) {
                Console.WriteLine("Fallback to Console only logging cause of missing config");
                SetupFallbackLogRepository(repository);
            }

            log = LogManager.GetLogger("Azuki", "Core.Main");
            discorelog = LogManager.GetLogger("Azuki", "DiscordAPI");
            InitializeEnvironment();
        }
        private void SetupFallbackLogRepository(ILoggerRepository repository) {
            Hierarchy hierarchy = (Hierarchy)repository;
            PatternLayout patternLayout = new PatternLayout {
                ConversionPattern = "%date %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();
            ConsoleAppender appender = new ConsoleAppender {
                Layout = patternLayout
            };
            hierarchy.Root.AddAppender(appender);
            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
        }
        private void InitializeEnvironment() {
            List<string> directorylist = new List<string> { "Config", "Modules" };
            foreach (string dir in directorylist) {
                Directory.CreateDirectory(dir);
            }
        }
        public static void Main(string[] args) {
            Azuki chris = new Azuki();
            chris.Run().Wait();
        }
        public async Task Run() {
            try {
                Stopwatch sw = new Stopwatch();
                log.Info("Initializing discord connection...");
                sw.Start();
                DiscoreLogger.MinimumLevel = DiscoreLogLevel.Error;
                if (discorelog.IsWarnEnabled) {
                    DiscoreLogger.MinimumLevel = DiscoreLogLevel.Warning;
                }
                if (discorelog.IsInfoEnabled) {
                    DiscoreLogger.MinimumLevel = DiscoreLogLevel.Info;
                }
                if (discorelog.IsDebugEnabled) {
                    DiscoreLogger.MinimumLevel = DiscoreLogLevel.Debug;
                }
                DiscoreLogger.OnLog += DiscoreLogger_OnLog;
                DiscordHttpClient client = new DiscordHttpClient(config.Token);
                await InitShards(client);
                sw.Stop();
                log.Info($"Discord connection Initialized in {sw.Elapsed.ToString(@"s\.f")}s.");
                log.Info($"Loading CoreModules...");
                sw.Reset();
                sw.Start();
                List<Type> modules = Assembly.GetExecutingAssembly().GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BaseModule))).ToList();
                foreach (Type type in modules) {
                    log.Debug("Loading " + type.Name + " Module...");
                    MethodInfo[] possiblecommands = type.GetMethods(
                        BindingFlags.DeclaredOnly |
                        BindingFlags.InvokeMethod |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance
                    ).Where(pcmd => pcmd.GetCustomAttribute<CommandAttribute>() != null)
                    .ToArray();
                    foreach (MethodInfo cmd in possiblecommands) {
                        AdminCommands.Add("Core." + cmd.Name, new Tuple<BaseModule, MethodInfo>(Activator.CreateInstance(type) as BaseModule, cmd));
                        log.Debug($"Found Command {cmd.Name}");
                    }
                }
                sw.Stop();
                log.Info($"CoreModules loaded in {sw.Elapsed.ToString(@"s\.f")}s.");
                sw.Reset();
                sw.Start();
                log.Info($"Loading Modules...");
                List<Type> expectedparams = new List<Type> { typeof(ICoreHandler), typeof(ulong), typeof(ulong), typeof(string) };
                foreach (string assembly in Directory.GetFiles("Modules", "*Module.dll")) {
                    log.Info($"Found possible Module container {Path.GetFileName(assembly)}");
                    List<Type> types = Assembly.LoadFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + assembly).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BaseModule))).ToList();
                    int c = 0;
                    foreach (Type t in types) {
                        MethodInfo[] possiblecommands = t.GetMethods(
                            BindingFlags.DeclaredOnly |
                            BindingFlags.InvokeMethod |
                            BindingFlags.Public |
                            BindingFlags.Instance
                        ).Where(pcmd => pcmd.GetCustomAttribute<CommandAttribute>() != null)
                        .ToArray();
                        string name = (t.Name.EndsWith("Module")) ? t.Name.Remove(t.Name.LastIndexOf("Module")) : t.Name;
                        foreach (MethodInfo cmd in possiblecommands) {
                            Commands.Add(name + "." + cmd.Name, new Tuple<BaseModule, MethodInfo>(Activator.CreateInstance(t) as BaseModule, cmd));
                            log.Debug($"Found Command {cmd.Name}");
                        }
                    }
                    log.Info($"Module container {Path.GetFileName(assembly)} contained {types.Count} modules with {c} commands.");
                }
                sw.Stop();
                log.Info($"Modules loaded in {sw.Elapsed.ToString(@"s\.f")}s.");
                log.Info("Startup complete.");
                stopsignal.WaitOne();
            } catch (Exception ex) {
                log.Fatal(ex);
            }
        }
        private async Task InitShards(DiscordHttpClient client) {
            List<Task> handlerinits = new List<Task>();
            int MinShards = await client.GetBotRequiredShards();
            for (int i = handlers.Count; i < MinShards; i++) {
                Shard s = new Shard(config.Token, i, MinShards);
                handlerinits.Add(s.StartAsync().ContinueWith(delegate { InitHandler(client, s); }));
            }
            Task.WaitAll(handlerinits.ToArray());
        }
        private void InitHandler(DiscordHttpClient client, Shard shard) {
            handlers.Add(new CoreHandler(client, shard, Commands));
            log.Debug("Created CoreHandler " + handlers.Count);
        }
        private void DiscoreLogger_OnLog(object sender, DiscoreLogEventArgs e) {
            switch (e.Message.Type) {
                case DiscoreLogLevel.Debug: {
                        discorelog.Debug(e.Message.Content);
                        break;
                    }
                case DiscoreLogLevel.Info: {
                        discorelog.Info(e.Message.Content);
                        break;
                    }
                case DiscoreLogLevel.Warning: {
                        discorelog.Warn(e.Message.Content);
                        break;
                    }
                case DiscoreLogLevel.Error: {
                        discorelog.Error(e.Message.Content);
                        break;
                    }
            }
        }
    }
}