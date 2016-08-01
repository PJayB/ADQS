using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace AudioSwitchCommon
{
    public class Configuration
    {
        // Constants
        public const string ConfigurationFileName = "config.xml";
        private const string ConfigurationDirectoryName = "AudioDeviceQS";
        
        // Public Properties
        public List<string> ExclusionIDs = new List<string>();

        public static string ConfigurationDirectory
        {
            get
            {
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(directory, ConfigurationDirectoryName);
            }
        }

        public static string ConfigurationFullPath
        {
            get
            {
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(directory, ConfigurationDirectoryName, ConfigurationFileName);
            }            
        }

        private Configuration()
        {
        }

        public static Configuration Load()
        {
            Configuration config = new Configuration();

            try
            {
                var xmlLoader = new XmlSerializer(typeof(Configuration));
                using (var xmlFile = new FileStream(ConfigurationFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    config = xmlLoader.Deserialize(xmlFile) as Configuration;
                    xmlFile.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load config: " + ex.Message);
            }

            return config;
        }

        public void Save()
        {
            // Create the directory
            try
            {
                Directory.CreateDirectory(ConfigurationDirectory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create directory: " + ex.Message);
            }

            // Write file
            try
            {
                var xmlWriter = new XmlSerializer(typeof(Configuration));
                using (var file = new FileStream(ConfigurationFullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                {
                    xmlWriter.Serialize(file, this);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to serialize settings: " + ex.Message);
            }
        }
    }
}
