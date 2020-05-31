using Azuki.Core.Modules.Api;
using Azuki.Core.Properties;
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
        internal static readonly Dictionary<string, Tuple<BaseModule, ILog, MethodInfo>> AdminCommands = new Dictionary<string, Tuple<BaseModule, ILog, MethodInfo>>();
        internal static AzukiConfig config;
        internal static List<Snowflake> Admins;
        internal static ManualResetEvent stopsignal = new ManualResetEvent(false);
        private readonly ILog log;
        private readonly ILog discorelog;
        private readonly List<CoreHandler> handlers = new List<CoreHandler>();
        private readonly Dictionary<string, Tuple<BaseModule, ILog, MethodInfo>> Commands = new Dictionary<string, Tuple<BaseModule, ILog, MethodInfo>>();
        private AssemblyLoadContext loader;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exiting on caught exception.")]
        public AzukiCore() {
            try {
                AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
                List<string> directorylist = new List<string> { "Config", "Modules", "Dependencies" };
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
                discorelog = LogManager.GetLogger("Azuki", "Discore.Main");
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
                XmlSerializer serializer = new XmlSerializer(typeof(AzukiConfig));
                using (XmlReader reader = XmlReader.Create("Config/Main.conf.xml")) {
                    config = serializer.Deserialize(reader) as AzukiConfig;
                }
                Admins = config.Admins;
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Environment.Exit(-1);
            }
        }
        private static void SetupFallbackLogRepository(ILoggerRepository repository) {
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
                log.Info(Resources.ResourceManager.GetString("Initializing", Resources.Culture));
                DiscordHttpClient client = new DiscordHttpClient(config.Token);
                await InitShards(client).ConfigureAwait(true);
                sw.Stop();
                log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("InitialConnectionTime", Resources.Culture), sw.Elapsed.ToString("s\\.f", Resources.Culture)));
                log.Info(Resources.ResourceManager.GetString("LoadingCoreModules", Resources.Culture));
                sw.Reset();
                sw.Start();
                List<Type> modules = Assembly.GetExecutingAssembly().GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BaseModule))).ToList();
                foreach (Type type in modules) {
                    log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("LoadingModule", Resources.Culture), type.FullName));
                    MethodInfo[] possiblecommands = type.GetMethods(
                        BindingFlags.DeclaredOnly |
                        BindingFlags.InvokeMethod |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static
                    ).Where(pcmd => pcmd.GetCustomAttribute<CommandAttribute>() != null)
                    .ToArray();
                    foreach (MethodInfo cmd in possiblecommands) {
                        if (cmd.ReturnType != typeof(Task)) {
                            log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("FoundCommandRejected", Resources.Culture), cmd.Name));
                        } else {
                            if (type.FullName == typeof(AzukiCore).FullName) {
                                AdminCommands.Add("Core." + cmd.Name, new Tuple<BaseModule, ILog, MethodInfo>(this, log, cmd));
                            } else {
                                AdminCommands.Add("Core." + cmd.Name, new Tuple<BaseModule, ILog, MethodInfo>(Activator.CreateInstance(type) as BaseModule, LogManager.GetLogger("Azuki", type.FullName), cmd));
                            }
                            log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("FoundCommand", Resources.Culture), cmd.Name));
                        }
                    }
                }
                sw.Stop();
                log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("LoadedCoreModules", Resources.Culture), sw.Elapsed.ToString("s\\.f", Resources.Culture)));
                log.Info(Resources.ResourceManager.GetString("StartupComplete", Resources.Culture));
                log.Info(Resources.ResourceManager.GetString("AutoLoadingModules", Resources.Culture));
                await LoadModules(null, null).ConfigureAwait(true);
                stopsignal.WaitOne();
                log.Info(Resources.ResourceManager.GetString("ShuttingDown", Resources.Culture));
                await UnLoadModules(null, null).ConfigureAwait(true);
                client.Dispose();
                log.Info(Resources.ResourceManager.GetString("SutdownComplete", Resources.Culture));
            } catch (Exception ex) {
                log.Fatal(ex.ToString());
                Environment.Exit(-1);
            }
        }
        [Command(NeedsHandler = true, NeedsMessage = true)]
        private Task LoadModules(ICoreHandler handler, Message message) {
            try {
                loader = new AssemblyLoadContext("AzukiModules", true);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                log.Info(Resources.ResourceManager.GetString("LoadingModules", Resources.Culture));
                List<Type> expectedparams = new List<Type> { typeof(ICoreHandler), typeof(ulong), typeof(ulong), typeof(string) };
                foreach (string assembly in Directory.GetFiles("Dependencies", "*.dll")) {
                    loader.LoadFromAssemblyPath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + Path.DirectorySeparatorChar + assembly);
                }
                foreach (string assembly in Directory.GetFiles("Modules", "*Module.dll")) {
                    log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("FoundModuleContainer", Resources.Culture), Path.GetFileName(assembly)));
                    List<Type> types = loader.LoadFromAssemblyPath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + Path.DirectorySeparatorChar + assembly).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BaseModule))).ToList();
                    int count = 0;
                    foreach (Type t in types) {
                        count += LoadCommands(t);
                    }
                    log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("LoadedModuleContainer", Resources.Culture), Path.GetFileName(assembly), types.Count, count));
                }
                sw.Stop();
                if (handler != null && message != null) {
                    handler.Respond(message.ChannelId, string.Format(Resources.Culture, Resources.ResourceManager.GetString("LoadedModules", Resources.Culture), sw.Elapsed.ToString("s\\.f", Resources.Culture)));
                }
                log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("LoadedModules", Resources.Culture), sw.Elapsed.ToString("s\\.f", Resources.Culture)));
            } catch (Exception ex) {
                log.Error(ex.ToString());
                throw;
            }
            return Task.CompletedTask;
        }
        private int LoadCommands(Type t) {
            int count = 0;
            log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("LoadingModule", Resources.Culture), t.FullName));
            MethodInfo[] possiblecommands = t.GetMethods(
                BindingFlags.DeclaredOnly |
                BindingFlags.InvokeMethod |
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static
            ).Where(pcmd => pcmd.GetCustomAttribute<CommandAttribute>() != null)
            .ToArray();
            string name = t.Name.EndsWith("Module", StringComparison.OrdinalIgnoreCase) ? t.Name.Remove(t.Name.LastIndexOf("Module", StringComparison.OrdinalIgnoreCase)) : t.Name;
            foreach (MethodInfo cmd in possiblecommands) {
                if (cmd.ReturnType != typeof(Task)) {
                    log.Info(string.Format(Resources.Culture, Resources.ResourceManager.GetString("FoundCommandRejected", Resources.Culture), cmd.Name) + Resources.ResourceManager.GetString("FoundCommandRejectedReasonType", Resources.Culture));
                } else {
                    Commands.Add($"{name}.{cmd.Name}", new Tuple<BaseModule, ILog, MethodInfo>(Activator.CreateInstance(t) as BaseModule, LogManager.GetLogger("Azuki", t.FullName), cmd));
                    Commands[$"{name}.{cmd.Name}"].Item1.LogDebug += delegate (object sender, string text) { Commands[$"{name}.{cmd.Name}"].Item2.Debug(text); };
                    Commands[$"{name}.{cmd.Name}"].Item1.LogInfo += delegate (object sender, string text) { Commands[$"{name}.{cmd.Name}"].Item2.Info(text); };
                    Commands[$"{name}.{cmd.Name}"].Item1.LogWarn += delegate (object sender, string text) { Commands[$"{name}.{cmd.Name}"].Item2.Warn(text); };
                    Commands[$"{name}.{cmd.Name}"].Item1.LogError += delegate (object sender, string text) { Commands[$"{name}.{cmd.Name}"].Item2.Error(text); };
                    count++;
                    log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("FoundCommand", Resources.Culture), cmd.Name));
                }
            }
            return count;
        }
        [Command(NeedsHandler = true, NeedsMessage = true)]
        private Task UnLoadModules(ICoreHandler handler, Message message) {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var c in Commands) {
                Commands.Remove(c.Key);
            }
            loader.Unload();
            sw.Stop();
            if (handler != null && message != null) {
                handler.Respond(message.ChannelId, string.Format(Resources.Culture, Resources.ResourceManager.GetString("UnLoadedModules", Resources.Culture), sw.Elapsed.ToString("s\\.f", Resources.Culture)));
            }
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
                log.Debug(string.Format(Resources.Culture, Resources.ResourceManager.GetString("CreatedHandler", Resources.Culture), shard.Id, handlers.Count));
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