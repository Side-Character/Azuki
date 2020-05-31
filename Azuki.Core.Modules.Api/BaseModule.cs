using System;

namespace Azuki.Core.Modules.Api {
    public abstract class BaseModule {
        public event EventHandler<string> LogDebug;
        public event EventHandler<string> LogInfo;
        public event EventHandler<string> LogWarn;
        public event EventHandler<string> LogError;

        protected void Debug(string text) {
            LogDebug?.Invoke(this, text);
        }
        protected void Info(string text) {
            LogInfo?.Invoke(this, text);
        }
        protected void Warn(string text) {
            LogWarn?.Invoke(this, text);
        }
        protected void Error(string text) {
            LogError?.Invoke(this, text);
        }
    }
}
