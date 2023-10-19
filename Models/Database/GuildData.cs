using BrikBotCore.Attributes;
using JetBrains.Annotations;

namespace BrikBotCore.Models.Database
{
	public class GuildData
	{
		[DataNames("GuildID")] public string GuildID { get; [UsedImplicitly] set; }
		[DataNames("BETA")] public bool Beta { get; [UsedImplicitly] set; }
	}
}