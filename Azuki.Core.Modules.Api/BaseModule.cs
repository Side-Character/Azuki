using System;

namespace Azuki.Core.Modules.Api {
    public abstract class BaseModule {
        public event EventHandler<string> LogDebug;
        public event EventHandler<string> LogInfo;
        public event EventHandler<string> LogWarn;
        public event EventHandler<string> LogError;
    }
}
