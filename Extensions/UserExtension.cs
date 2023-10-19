using System.Threading.Tasks;
using BrikBotCore.Models.Internal;
using Discord;
using Discord.WebSocket;

namespace BrikBotCore.Extensions
{
	public static class UserExtension
	{
		public static async Task<IGuildUser> FindUserAsync(this IGuild guild, DiscordShardedClient client, ulong userId)
		{
			if (guild is SocketGuild socketGuild)
			{
				var tmp = socketGuild.GetUser(userId);

				if (tmp != null)
					return tmp;
			}

			return await client.Rest.GetGuildUserAsync(guild.Id, userId);
		}

		public static string GetAvatar(this IUser user)
		{
			if (user?.GetAvatarUrl() == null)
			{
				if (user?.GetDefaultAvatarUrl() == null)
					return Config.Instance.URIs.DefaultDiscordIcon;
				return user.GetDefaultAvatarUrl();
			}

			return user.GetAvatarUrl();
		}
	}
}