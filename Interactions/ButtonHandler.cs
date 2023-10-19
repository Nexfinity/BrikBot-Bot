using System.Threading.Tasks;
using BrikBotCore.Cache;
using BrikBotCore.Extensions;
using BrikBotCore.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
namespace BrikBotCore.Interactions
{
	[EnabledInDm(false)]
	public class ButtonHandler : InteractionModuleBase<ShardedInteractionContext>
	{
		private readonly Logger _log;
		private readonly DiscordShardedClient _client;
		private readonly DataCache _cache;

		public ButtonHandler(Logger log, DiscordShardedClient discord, DataCache cache)
		{
			_log = log;
			_cache = cache;
			_client = discord;
		}

		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[ComponentInteraction("cb_delete")]
		public async Task Delete()
		{
			await DeferAsync();
			await ((SocketMessageComponent)Context.Interaction).Message.TryDeleteAsync();
		}
	}
}