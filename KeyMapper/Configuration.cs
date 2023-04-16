using KeyMapper.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace KeyMapper
{
    public sealed class Configuration
    {
        public string WindowName { get; }
        public string ProcessName { get; }
        public ReadOnlyDictionary<short, short> KeyMapDict { get; }

        private readonly Regex windowNameRx = new Regex($"{nameof(WindowName)} *= *(.+)", RegexOptions.Compiled);
        private readonly Regex processNameRx = new Regex($"{nameof(ProcessName)} *= *(.+)", RegexOptions.Compiled);
        private readonly Regex keyMapDictRx = new Regex($"{nameof(KeyMapDict)} *:", RegexOptions.Compiled);
        private readonly Regex keyMapRx = new Regex(@"    (.+) *= *(.+)", RegexOptions.Compiled);
        private readonly Int16Converter shortConverter = new Int16Converter();

        public Configuration()
        {
            WindowName = "Warcraft III";
            ProcessName = "War3.exe";
            KeyMapDict = new ReadOnlyDictionary<short, short>(new Dictionary<short, short>
            {
                {0x61, 0x31},
                {0x62, 0x32}
            });
        }

        public Configuration(string configFilePath)
        {
            var lines = File.ReadAllLines(configFilePath);
            for(int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if(windowNameRx.IsMatch(line))
                {
                    if(WindowName != default) throw new InvalidConfigurationException(configFilePath, i);

                    WindowName = windowNameRx.Match(line).Groups[1].Value.Trim();
                    continue;
                }

                if(processNameRx.IsMatch(line))
                {
                    if(ProcessName != default) throw new InvalidConfigurationException(configFilePath, i);
                    ProcessName = processNameRx.Match(line).Groups[1].Value.Trim();
                    continue;
                }

                if(keyMapDictRx.IsMatch(line))
                {
                    if(KeyMapDict != default) throw new InvalidConfigurationException(configFilePath, i);
                    var keyMapDict = new Dictionary<short, short>();
                    while(i < lines.Length - 1)
                    {
                        line = lines[++i];
                        if(!line.StartsWith("    "))
                        {
                            i--;
                            break;
                        }

                        var match = keyMapRx.Match(line);
                        if(!match.Success) throw new InvalidConfigurationException(configFilePath, i);
                        var group = match.Groups;
                        try
                        {
                            var originalKey = (short)shortConverter.ConvertFromString(group[1].Value.Trim());
                            var targetKey = (short)shortConverter.ConvertFromString(group[2].Value.Trim());
                            keyMapDict.Add(originalKey, targetKey);
                        }
                        catch(Exception e)
                        {
                            throw new InvalidConfigurationException(configFilePath, i, e);
                        }
                    }
                    KeyMapDict = new ReadOnlyDictionary<short, short>(keyMapDict);
                    continue;
                }

                throw new InvalidConfigurationException(configFilePath, i);
            }
        }

        public void Save(string configFilePath)
        {
            var lines = new List<string>();
            lines.Add($"{nameof(WindowName)}={WindowName}");
            lines.Add($"{nameof(ProcessName)}={ProcessName}");
            lines.Add($"{nameof(KeyMapDict)}:");
            foreach(var kvp in KeyMapDict!)
            {
                lines.Add($"    0x{kvp.Key,0:X2}=0x{kvp.Value,0:X2}");
            }
            File.WriteAllLines(configFilePath, lines);
        }
    }
}
