using System;
using System.Linq;
using System.Threading.Tasks;
using BrikBotCore.Enums;
using BrikBotCore.Extensions;
using BrikBotCore.Models.Internal;
using BrikBotCore.Services;
using Discord;
using Discord.Interactions;

namespace BrikBotCore.Interactions.SlashCommands
{
	public class CoreCommands : InteractionModuleBase<ShardedInteractionContext>
	{
		private readonly Logger _log;
		public CoreCommands(Logger log)
		{
			_log = log;
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("help", "Displays various information regarding the bot.")]
		public async Task HelpAsync()
		{
			try
			{
				EmbedBuilder builder = new EmbedBuilder()
					.WithLimitedTitle("Core | Bot Commands")
					.WithUrl(Config.Instance.URIs.WebsiteURL)
					.WithColorType(EmbedColor.Ok)
					.WithLimitedDescription("The prefix for this guild is `/`\n" +
					                        "If you are looking for a list of bot commands and how they work, click the `Bot Commands` link below. If you need help figuring out something check out our `FAQ` (Frequently Asked Questions) link.")
					.RequestedBy(Context.User, Context.Guild, $" • Guild ID: {Context.Guild.Id}");


				builder.WithLimitedField(
					"Useful Links\n",
					$"<:CB_Commands:591728832508199074> [Bot Commands]({Config.Instance.URIs.WebsiteURL}/commands.html)\n" +
					$"<:CB_DiscordV2:591729052562227212> [Support Discord]({Config.Instance.URIs.DiscordURL})\n" +
					$"<:CB_Invite:454704383750570024> [Bot Invite Link]({Config.Instance.URIs.InviteURL})\n" +
					$"<:CB_Exclamation:453693573997527041> [Privacy Policy]({Config.Instance.URIs.WebsiteURL}/privacy.html)");

				await RespondAsync("", new[] { builder.Build() });
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}

		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("invite", "Generates a link to invite me to your guild.")]
		public async Task Invite()
		{
			try
			{
				var builder = new EmbedBuilder()
					.WithLimitedTitle("Core | Invite")
					.WithUrl(Config.Instance.URIs.WebsiteURL)
					.WithColorType(EmbedColor.Ok)
					.WithLimitedField("BrikBot", $"[Click Here]({Config.Instance.URIs.InviteURL})");

				await RespondAsync(embed: builder.Build());
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}

		[SlashCommand("top", "View a list of the largest guilds BrikBot is in.")]
		public async Task Top()
		{
			try
			{
				var count = 1;
				var guilds = Context.Client.Guilds.OrderByDescending(g => g.MemberCount);
				var guildInfo = guilds.Take(10).Aggregate("", (current, g) => current + $"**#{count++}** -- **Member Count:** {g.MemberCount:N0} -- **Name:** {g.Name}\n");

				await RespondAsync(guildInfo);
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
	}
}