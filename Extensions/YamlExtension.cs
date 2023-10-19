using YamlDotNet.Serialization;

namespace BrikBotCore.Extensions
{
	public class YamlExtension
	{
		public static bool TryDeserialize<T>(string yaml, out T value)
		{
			try
			{
				var deserializer = new Deserializer();
				value = deserializer.Deserialize<T>(yaml);
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		public static bool TrySerialize(object obj, out string yaml)
		{
			try
			{
				var serializer = new Serializer();
				yaml = serializer.Serialize(obj);
				return true;
			}
			catch
			{
				yaml = string.Empty;
				return false;
			}
		}
	}
}