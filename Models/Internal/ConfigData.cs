using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BrikBotCore.Extensions;
using Serilog;
using YamlDotNet.Serialization;

namespace BrikBotCore.Models.Internal
{
	public class BotConfig
	{
		public ulong BotID { get; set; }
		public string Token { get; set; }
		public ulong OwnerID { get; set; }
		public ulong MainGuildID { get; set; }
		public string BrikSetApiKey { get; set; }

		[YamlIgnore] public Blacklist Blacklist { get; set; }
	}

	public class FileSystemConfig
	{
		public string TemporaryFolder { get; set; }
	}

	public class ValuesConfig
	{
		public int BotHellLimit { get; set; }
	}

	public class URIConfig
	{
		public string DiscordURL { get; set; }
		public string InviteURL { get; set; }
		public string WebsiteURL { get; set; }
		public string DefaultDiscordIcon { get; set; }
	}

	public class Blacklist
	{
		public List<ulong> CommandUserIDs { get; set; }
		public List<ulong> GuildOwnerUserIDs { get; set; }
		public List<ulong> GuildIDs { get; set; }
	}

	public class Config
	{
		private const string ConfigPath = @"Settings/config.yaml"; //Linux
		public BotConfig Bot { get; set; }
		public FileSystemConfig FileSystem { get; set; }
		public ValuesConfig Values { get; set; }
		public URIConfig URIs { get; set; }

		public string DbConnectionString { get; set; }
		//private const string ConfigPath = @"C:\Users\Administrator\Documents\GitLab\BrikBot\BrikBotCore\Settings\config.yaml"; //Windows

		public static Config Instance { get; private set;  } = Initialize();

		public async Task SaveAsync()
		{
			if (YamlExtension.TrySerialize(this, out var yaml))
				await File.WriteAllTextAsync(ConfigPath, yaml);
		}
		
		public static void ReloadConfig()
		{
			try
			{
				var config = DeserializeYaml<Config>(ConfigPath);
				config.Bot.Blacklist = LoadBlacklist();
				Instance = config;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex.Message + "\n" + ex.StackTrace);
				Instance = null;
			}
		}

		private static T DeserializeYaml<T>(string path)
		{
			if (File.Exists(path))
			{
				var yaml = File.ReadAllText(path);
				if (YamlExtension.TryDeserialize(yaml, out T value))
					return value;
			}
			else
			{
				Log.Fatal("No config file found!");
			}

			throw new SerializationException($"Could not deserialize yaml file: \'{path}\'");
		}

		private static Config Initialize()
		{
			try
			{
				var config = DeserializeYaml<Config>(ConfigPath);
				config.Bot.Blacklist = LoadBlacklist();
				return config;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}

		private static Blacklist LoadBlacklist()
		{
			return DeserializeYaml<Blacklist>(@"Settings/blacklist.yaml"); //Linux
		}
		//=> DeserializeYaml<Blacklist>(@"C:\Users\Administrator\Documents\GitLab\BrikBot\BrikBotCore\Settings\blacklist.yaml"); //Windows
	}
}