using System;
using System.Linq;
using System.Threading.Tasks;
using BrikBotCore.Models.Internal;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace BrikBotCore.Extensions
{
	public static class OtherExtensions
	{
		public static int SecondsToMilliseconds(this int time)
		{
			return time * 1000;
		}

		/// <summary>
		///     Deletes a <see cref="IUserMessage" />.
		/// </summary>
		/// <param name="msg">The message to delete.</param>
		public static Task TryDeleteAsync(this IMessage msg)
		{
			try
			{
				_ = Task.Run(async () =>
				{
					try
					{
						if (!(msg?.Channel is SocketGuildChannel channel)) return;
						if (channel.CanDeleteMessages(channel.Guild.GetUser(Config.Instance.Bot.BotID)))
						{
							await Task.Delay(1000);
							await msg.DeleteAsync();
						}
					}
					catch (HttpException)
					{
						//Nothing, message was already deleted somehow
					}
				});
			}
			catch (Exception)
			{
				//Log.Error($"{ex.Message} \n {ex.StackTrace}");
			}

			return Task.CompletedTask;
		}

		/// <summary>
		///     Check if a user is blacklisted from using commands in BrikBot.
		/// </summary>
		/// <param name="user">The user to check.</param>
		/// <returns>True/False</returns>
		public static bool IsCommandBlacklisted(this IUser user)
		{
			return Config.Instance.Bot.Blacklist.CommandUserIDs.Any(id => id == user.Id);
		}

		/// <summary>
		///     Check if a guild (or the guild's owner) is blacklisted from BrikBot.
		/// </summary>
		/// <param name="guild">The user to check.</param>
		/// <returns>True/False</returns>
		public static bool IsBlacklisted(this SocketGuild guild)
		{
			return Config.Instance.Bot.Blacklist.GuildOwnerUserIDs.Any(id => id == guild.OwnerId) || Config.Instance.Bot.Blacklist.GuildIDs.Any(id => id == guild.Id);
		}

		/// <summary>
		///     Check if the guild is considered a bot hell.
		/// </summary>
		/// <param name="guild">The guild to check.</param>
		/// <returns>True/False</returns>
		public static bool IsBothell(this SocketGuild guild)
		{
			//if(!guild.HasAllMembers) guild.DownloadUsersAsync();
			var bots = guild.Users.Count(u => u.IsBot);
			var humans = guild.Users.Count(u => !u.IsBot);

			if (guild.Id == 454933217666007052 || guild.Id == 450100127256936458) return false;

			if (bots > Config.Instance.Values.BotHellLimit)
				if (bots > humans)
					return true;

			return false;
		}
	}
}