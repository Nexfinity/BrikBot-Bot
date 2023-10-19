using Discord;
using Discord.WebSocket;

namespace BrikBotCore.Extensions
{
	public static class PermissionsExtension
	{
		/// <summary>
		///     Check a user's permissions to see if they are considered a stage mod.
		/// </summary>
		/// <param name="channel">The stage to check.</param>
		/// <param name="user">The user to check.</param>
		/// <returns>True/False</returns>
		public static bool IsAdministrator(this IGuildUser user)
		{
			return user.GuildPermissions.Administrator;
		}

		/// <summary>
		///     Check a user's permissions to see if they can delete messages in the provided channel.
		/// </summary>
		/// <param name="channel">The channel to check.</param>
		/// <param name="user">The user to check.</param>
		/// <returns>True/False</returns>
		public static bool CanDeleteMessages(this SocketGuildChannel channel, SocketGuildUser user)
		{
			//Check global guild perms
			if (user.IsAdministrator()) return true;

			//Check if user can view the channel
			var chanPerms = user.GetPermissions(channel);
			if (!chanPerms.ViewChannel || !chanPerms.ReadMessageHistory) return false;

			//Check channel overrides
			return chanPerms.ManageMessages;
		}
	}
}