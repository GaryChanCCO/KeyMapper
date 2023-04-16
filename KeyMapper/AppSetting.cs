using KeyMapper.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace KeyMapper
{
    public class AppSetting
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? ProcessId { get; private set; }
        public string? ProcessName { get; private set; }
        public string? WindowName { get; private set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        [JsonInclude]
        public ReadOnlyDictionary<int, int>? KeyMapDict { get; }

        public AppSetting EnsureValidSetting()
        {
            if(ProcessId == null && ProcessName == null && WindowName == null)
            {
                throw new Exception($"You must provide one of the following: {nameof(ProcessId)}, {nameof(ProcessName)}, {nameof(WindowName)}");
            }

            if(KeyMapDict == null)
            {
                throw new Exception($"You must provide {nameof(KeyMapDict)}");
            }

            return this;
        }
    }
}
