using System.Configuration;
using CED.Config;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CED.Tests._461.Config
{
    public abstract class ConfigSectionBasedTests
    {
        protected static EventConfigSection LoadEventConfigSection(string fileName, string configSectionName)
        {
            var fileMap =
                new ExeConfigurationFileMap { ExeConfigFilename = fileName };

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            return configuration.GetSection(configSectionName) as EventConfigSection;
        }

        protected static void EnableLogging()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            fileTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message} ${exception:format=toString}";
            fileTarget.FileName = "${basedir}/file.txt";
            
            var rule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
        }
    }
}