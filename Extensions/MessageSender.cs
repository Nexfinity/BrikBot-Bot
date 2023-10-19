using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrikBotCore.Cache;
using BrikBotCore.Enums;
using BrikBotCore.Models.Internal;
using BrikBotCore.Services;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace BrikBotCore.Extensions
{
	public class MessageSender
	{
		private static readonly List<ulong> OwnersNotified = new List<ulong>();
		private readonly Logger _log;

		public MessageSender(Logger log)
		{
			_log = log;
		}

		//Check channel permissions to see if BrikBot can send messages there
		public bool CanSendMessage(IChannel channel, bool notifyOwner = false)
		{
			try
			{
				var guild = ((SocketGuildChannel) channel)?.Guild;

				if (guild?.CurrentUser == null) return false;

				//TODO: Check if bot has been "timed out"

				var chanPerms = guild.CurrentUser.GetPermissions((IGuildChannel) channel);

				if (guild.CurrentUser.IsAdministrator()) return true;

				if (!chanPerms.ViewChannel || !chanPerms.SendMessages || !chanPerms.EmbedLinks)
				{
					if (notifyOwner && !OwnersNotified.Contains(guild.OwnerId))
					{
						var builder = new EmbedBuilder()
							.WithLimitedTitle("Core | Error")
							.WithUrl(Config.Instance.URIs.WebsiteURL)
							.WithColorType(EmbedColor.Ok)
							.WithLimitedDescription("BrikBot needs SendMessage, ReadMessage and Embed Link permissons!\n" +
							                        "Some things such as moderation commands will require extra permissions such as Kick, Ban or Manage Roles.\n" +
							                        "This message was sent because BrikBot tried to post or edit something and lacked permissions.\n" +
							                        "Either the audit log, user join/leave announcements or other functions are malfunctioning!\n\n" +
							                        $"Guild Affected: {guild.Name} / {guild.Id}\n" +
							                        $"Channel Affected: {channel.Name} / {channel.Id}\n" +
							                        $"Link to channel: https://discord.com/channels/{guild.Id}/{channel.Id}");

						try
						{
							try
							{
								guild.Owner.CreateDMChannelAsync().Result.SendMessageAsync("", false, builder.Build());
							}
							catch
							{
								//Nothing
							}

							OwnersNotified.Add(guild.OwnerId);
						}
						catch (Exception)
						{
							//Sometimes users have DMs blocked, not really much we can do about this but it generates spam issues on Sentry :(
						}
					}

					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex);
				return false;
			}
		}

		//Send normal message/embed
		public async Task<IUserMessage> SendAsync(IMessageChannel channel, string content = null, EmbedBuilder embed = null, AllowedMentions allowedMentions = null, ComponentBuilder component = null, MessageReference reference = null)
		{
			try
			{
				if (!CanSendMessage(channel)) return null; 
				
				return await channel.SendMessageAsync(content?.TrimTo(1997, true) ?? (embed == null ? "Unknown error occurred." : ""), false, embed?.Build(), null, allowedMentions == null ? AllowedMentions.None : AllowedMentions.All, reference, component?.Build());
			}
			catch (HttpException ex)
			{
				if (ex.Message.Contains("The operation has timed out") || ex.Message.Contains("Missing Permissions")) return null;
				var inner = ex.InnerException;
				if (inner != null && inner.Message.Contains("REPLIES_UNKNOWN_MESSAGE"))
				{
					try
					{
						return await channel.SendMessageAsync(content?.TrimTo(1997, true) ?? (embed == null ? "Unknown error occurred." : ""), false, embed?.Build(), null, allowedMentions == null ? AllowedMentions.None : AllowedMentions.All, null, component?.Build());
					}
					catch (Exception)
					{
						_log.ErrorException(inner, notes: "Channel1: " + channel.Id);
					}
				}
				
				_log.ErrorException(ex, notes: "Channel2: " + channel.Id);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("The operation has timed out") || ex.Message.Contains("Missing Permissions")) return null;

				_log.ErrorException(ex, notes: "Channel3: " + channel.Id);
			}

			return null;
		}
	}
}