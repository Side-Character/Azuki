using System;

namespace Azuki.Core.Modules.Api {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class CommandAttribute : Attribute {
        public bool NeedsHandler {
            get;
            set;
        }
        public bool NeedsMessage {
            get;
            set;
        }
        public bool NeedsVoice {
            get;
            set;
        }
        public bool HasParams {
            get;
            set;
        }
        public CommandAttribute() {
        }
    }
}
