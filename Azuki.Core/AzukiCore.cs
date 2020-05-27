using Azuki.Core.Properties;
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
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Azuki.Core {
    public class AzukiCore : BaseModule {
        internal static readonly Dictionary<string, Tuple<BaseModule, MethodInfo>> AdminCommands = new Dictionary<string, Tuple<BaseModule, MethodInfo>>();
        internal static Config.Config config;
        internal static List<Snowflake> Admins;
        internal static ManualResetEvent stopsignal = new ManualResetEvent(false);
        private readonly ILog log;
        private readonly ILog discorelog;
        private readonly List<CoreHandler> handlers = new List<CoreHandler>();
        private readonly Dictionary<string, Tuple<BaseModule, MethodInfo>> Commands = new Dictionary<string, Tuple<BaseModule, MethodInfo>>();
        private AssemblyLoadContext loader;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exiting on caught exception.")]
        public AzukiCore() {
            try {
                AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
                List<string> directorylist = new List<string> { "Config", "Modules" };
                foreach (string dir in directorylist) {
                    Directory.CreateDirectory(dir);
                }
                Console.WriteLine(Resources.ResourceManager.GetString("ReadLogSettings", Resources.Culture));
                ILoggerRepository repository = LogManager.CreateRepository("Azuki");
                XmlConfigurator.ConfigureAndWatch(repository, new FileInfo("Config/Log.conf.xml"));
                if (repository.GetAppenders().Length == 0) {
                    Console.WriteLine(Resources.ResourceManager.GetString("LoggerFallback", Resources.Culture));
                    SetupFallbackLogRepository(repository);
                }
                log = LogManager.GetLogger("Azuki", "Core.Main");
                discorelog = LogManager.GetLogger("Azuki", "DiscordAPI");
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
                XmlSerializer serializer = new XmlSerializer(typeof(Config.Config));
                using (XmlReader reader = XmlReader.Create("Config/Main.conf.xml")) {
                    config = serializer.Deserialize(reader) as Config.Config;
                }
                Admins = config.Admins;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Environment.Exit(-1);
            }
        }
        private void SetupFallbackLogRepository(ILoggerRepository repository) {
            try {
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
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
        public static void Main() {
            AzukiCore azuki = new AzukiCore();
            azuki.Run().Wait();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exiting on caught exception.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Added to container, should be disposed on shutdown.")]
        public async Task Run() {
            try {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                log.Info("Initializing discord connection...");
                DiscordHttpClient client = new DiscordHttpClient(config.Token);
                await InitShards(client).ConfigureAwait(true);
                sw.Stop();
                log.Info($"Discord connection Initialized in {sw.Elapsed:s\\.f}s.");
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
                        if (cmd.ReturnType != typeof(Task)) {
                            log.Debug($"Found Command {cmd.Name}, was rejected.(Reason: Wrong return type, must be Task)");
                        } else {
                            if (type.FullName == typeof(AzukiCore).FullName) {
                                AdminCommands.Add("Core." + cmd.Name, new Tuple<BaseModule, MethodInfo>(this, cmd));
                            } else {
                                AdminCommands.Add("Core." + cmd.Name, new Tuple<BaseModule, MethodInfo>(Activator.CreateInstance(type) as BaseModule, cmd));
                            }
                            log.Debug($"Found Command {cmd.Name}");
                        }
                    }
                }
                sw.Stop();
                log.Info($"Core Modules loaded in {sw.Elapsed:s\\.f}s.");
                log.Info("Startup complete.");
                log.Info($"Auto-loading Modules for startup...");
                await LoadModules().ConfigureAwait(true);
                stopsignal.WaitOne();
                log.Info("Shuttin down...");
                await UnLoadModules().ConfigureAwait(true);
                log.Info("Shutdown complete.");
            } catch (Exception ex) {
                log.Fatal(ex.ToString());
                Environment.Exit(-1);
            }
        }
        [Command()]
        private Task LoadModules() {
            try {
                loader = new AssemblyLoadContext("AzukiModules", true);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                log.Info($"Loading Modules...");
                List<Type> expectedparams = new List<Type> { typeof(ICoreHandler), typeof(ulong), typeof(ulong), typeof(string) };
                foreach (string assembly in Directory.GetFiles("Modules", "*Module.dll")) {
                    log.Info($"Found possible Module container {Path.GetFileName(assembly)}");
                    List<Type> types = loader.LoadFromAssemblyPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + assembly).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BaseModule))).ToList();
                    int c = 0;
                    foreach (Type t in types) {
                        MethodInfo[] possiblecommands = t.GetMethods(
                            BindingFlags.DeclaredOnly |
                            BindingFlags.InvokeMethod |
                            BindingFlags.Public |
                            BindingFlags.Instance
                        ).Where(pcmd => pcmd.GetCustomAttribute<CommandAttribute>() != null)
                        .ToArray();
                        string name = t.Name.EndsWith("Module", StringComparison.OrdinalIgnoreCase) ? t.Name.Remove(t.Name.LastIndexOf("Module", StringComparison.OrdinalIgnoreCase)) : t.Name;
                        foreach (MethodInfo cmd in possiblecommands) {
                            if (cmd.ReturnType != typeof(Task)) {
                                log.Debug($"Found Command {cmd.Name}, was rejected.(Reason: Wrong return type, must be Task)");
                            } else {
                                Commands.Add(name + "." + cmd.Name, new Tuple<BaseModule, MethodInfo>(Activator.CreateInstance(t) as BaseModule, cmd));
                                c++;
                                log.Debug($"Found Command {cmd.Name}");
                            }
                        }
                    }
                    log.Info($"Module container {Path.GetFileName(assembly)} contained {types.Count} module(s) with {c} command(s).");
                }
                sw.Stop();
                log.Info($"Modules loaded in {sw.Elapsed:s\\.f}s.");
            } catch (Exception ex) {
                log.Error(ex.ToString());
                throw;
            }
            return Task.CompletedTask;
        }
        [Command()]
        private Task UnLoadModules() {
            foreach (var c in Commands) {
                Commands.Remove(c.Key);
            }
            loader.Unload();
            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Added to container, should be disposed on shutdown.")]
        private async Task InitShards(DiscordHttpClient client) {
            try {
                List<Task> handlerinits = new List<Task>();
                int MinShards = await client.GetBotRequiredShards().ConfigureAwait(true);
                for (int i = handlers.Count; i < MinShards; i++) {
                    Shard s = new Shard(config.Token, i, MinShards);
                    handlerinits.Add(s.StartAsync().ContinueWith(delegate {
                        InitHandler(client, s);
                    }, TaskScheduler.Default));
                }
                Task.WaitAll(handlerinits.ToArray());
            } catch (Exception ex) {
                log.Error(ex.ToString());
                throw;
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exiting on caught exception.")]
        private void InitHandler(DiscordHttpClient client, Shard shard) {
            try {
                handlers.Add(new CoreHandler(client, shard, Commands));
                log.Debug($"Created CoreHandler for shard {shard.Id}, Current count: {handlers.Count}");
            } catch (Exception ex) {
                log.Fatal(ex.ToString());
                Environment.Exit(-1);
            }
        }
        private void DiscoreLogger_OnLog(object sender, DiscoreLogEventArgs e) {
            try {
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
                    default:
                    case DiscoreLogLevel.Error: {
                            discorelog.Error(e.Message.Content);
                            break;
                        }
                }
            } catch (Exception ex) {
                log.Warn(ex.ToString());
                throw;
            }
        }
    }
}