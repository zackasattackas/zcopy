using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;

namespace BananaHomie.ZCopy.Internal
{
    internal class ZCopyEnvironment
    {
        [EnvironmentVariable("ZCOPY_DISABLE_ANSI_CONSOLE", false)]
        public bool DisableAnsiConsole { get; set; }

        [EnvironmentVariable("ZCOPY_DEFAULT_COMMAND_LINE_OPTIONS")]
        public string DefaultCommandLineOptions { get; set; }

        public ZCopyEnvironment()
        {
            SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;
        }

        ~ZCopyEnvironment()
        {
            SystemEvents.UserPreferenceChanged -= SystemEventsOnUserPreferenceChanged;
        }

        private void SystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General)
                return;

            foreach (var property in typeof(ZCopyEnvironment).GetProperties().Where(p => p.IsDefined(typeof(EnvironmentVariableAttribute))))
            {
                var attr = property.GetCustomAttribute<EnvironmentVariableAttribute>();
                var value = Environment.GetEnvironmentVariable(attr.Name, EnvironmentVariableTarget.User);

                property.SetValue(this, value ?? attr.DefaultValue);
            }
        }
    }
}
