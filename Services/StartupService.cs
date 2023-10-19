using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BrikBotCore.Enums;
using BrikBotCore.Extensions;
using BrikBotCore.Models.Internal;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace BrikBotCore.Services
{
	public sealed class StartupService : BaseService
	{
		private static readonly List<int> ReadyShards = new List<int>();

		public StartupService(DiscordShardedClient discord, CommandService commands, Logger logger, InteractionService interactions)
		{
			Client = discord;
			Commands = commands;
			Client.JoinedGuild += OnJoin;
			Client.LeftGuild += OnLeave;
			Client.ShardReady += ShardReadyAsync;
			Client.ShardConnected += ShardConnectedAsync;
			Client.Log += OnLogAsync;
			Logger = logger;
			Interaction = interactions;
		}

		private static async Task OnLogAsync(LogMessage message)
		{
			var severity = message.Severity switch
			{
				LogSeverity.Critical => LogEventLevel.Fatal,
				LogSeverity.Error => LogEventLevel.Error,
				LogSeverity.Warning => LogEventLevel.Warning,
				LogSeverity.Info => LogEventLevel.Information,
				LogSeverity.Verbose => LogEventLevel.Verbose,
				LogSeverity.Debug => LogEventLevel.Debug,
				_ => LogEventLevel.Information
			};
			
			Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
			await Task.CompletedTask;
		}
		
		private async Task ShardReadyAsync(DiscordSocketClient socketClient)
		{
			try
			{
				Logger.Nice("Shard", ConsoleColor.Green, $"Shard: {socketClient.ShardId} Ready with {socketClient.Guilds.Count} Guilds and {socketClient.Guilds.Sum(x => x.MemberCount)} Users");

				Logger.Notify($"Running guild checks on Shard #{socketClient.ShardId}");
				foreach (var guild in socketClient.Guilds)
				{
					//Check for bot hells
					if (guild.IsBothell())
					{
						var bots = guild.Users.Count(u => u.IsBot);
						var humans = guild.Users.Count(u => !u.IsBot);
						Logger.Nice("Event", ConsoleColor.Red, $"Startup Check - Error: Bot hell - Users: {humans} - Bots: {bots}");
						try
						{
							await guild.FindUserAsync(Client, guild.OwnerId).Result.CreateDMChannelAsync().Result.SendMessageAsync("Opps, It seems you have more bots than humans in your guild. BrikBot has been removed from your guild to prevent abuse, you may re-invite BrikBot once you have more humans than bots.");
						}
						catch (Exception)
						{
							Logger.Nice("Event", ConsoleColor.Red, "Gateway Guild Join - Error: Failed to send bothell DM");
						}

						await guild.LeaveAsync();
					}

					//Check for blacklisted servers
					if (guild.IsBlacklisted())
					{
						Logger.Nice("Event", ConsoleColor.Red, $"Gateway Guild Join - Error: Blacklisted Guild - Owner ID: {guild.Id} - Guild ID: {guild.Id}");
						try
						{
							await guild.FindUserAsync(Client, guild.OwnerId).Result.CreateDMChannelAsync().Result.SendMessageAsync("Opps, It seems your guild has been blacklisted from this bot. BrikBot has been removed from your guild to prevent abuse.\n" +
							                                                                                                       $"For more information or to get un-blacklisted, please join our support discord here: {Config.Instance.URIs.DiscordURL}");
						}
						catch (Exception)
						{
							Logger.Nice("Event", ConsoleColor.Red, "Gateway Guild Join - Error: Failed to send blacklist DM");
						}

						await guild.LeaveAsync();
					}

					//Download guild owner
					//Client.Rest.GetGuildUserAsync(guild.Id, guild.OwnerId);
				}

				ReadyShards.Add(socketClient.ShardId);

				if (await AllShardsReady())
				{
					Log.Debug("Registering interactions...");
					var cmds = Interaction.RegisterCommandsGloballyAsync().Result;
					Log.Debug("Total commands: {Count}", cmds.Count);
					Log.Debug("Total slash commands: {Count}", cmds.Count(x => x.Type == ApplicationCommandType.Slash));
					Log.Debug("Total user context commands: {Count}", cmds.Count(x => x.Type == ApplicationCommandType.User));
					Log.Debug("Total message context commands: {Count}", cmds.Count(x => x.Type == ApplicationCommandType.Message));
				}
			}
			catch (Exception ex)
			{
				Logger.ErrorException(ex);
			}
		}

		private Task ShardConnectedAsync(DiscordSocketClient socketClient)
		{
			try
			{
				Logger.Nice("Shard", ConsoleColor.Green, $"Shard: {socketClient.ShardId} Connected with {socketClient.Guilds.Count} Guilds and {socketClient.Guilds.Sum(x => x.MemberCount)} Users");
			}
			catch (Exception ex)
			{
				Logger.ErrorException(ex);
			}

			return Task.CompletedTask;
		}

		public async Task StartAsync(IServiceProvider services, IServiceCollection serviceCollection)
		{
			try
			{
				//Authorize bot using bot token
				await Client.LoginAsync(TokenType.Bot, Config.Instance.Bot.Token);
				Logger.Notify("Bot Starting");

				//Start the bot
				await Client.StartAsync();

				//Add commands/modules
				await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

				//Set game presence
				//await Client.SetGameAsync("!help | https://brikbot.app/ <3");

				//Clear out temp diretory
				Logger.Notify("Clearing /Temp/ directory...");
				var di = new DirectoryInfo("Temp/");
				foreach (FileInfo file in di.GetFiles())
				{
					file.Delete(); 
				}
			}
			catch (Exception ex)
			{
				Logger.ErrorException(ex);
			}
		}

		private async Task OnJoin(SocketGuild guild)
		{
			try
			{
				Logger.Nice("Event", ConsoleColor.Yellow, "Gateway Guild Join - Start");
				var bots = guild.Users.Count(u => u.IsBot);
				var humans = guild.Users.Count(u => !u.IsBot);
				const int botLimit = 40;

				if (guild.Id == 454933217666007052 || guild.Id == 450100127256936458)
					return;

				var owner = await guild.FindUserAsync(Client, guild.OwnerId);
				if (guild.IsBothell())
				{
					Logger.Nice("Event", ConsoleColor.Red, $"Gateway Guild Join - Error: Bot hell - Users: {humans} - Bots: {bots} - Bot limit: {botLimit}");
					try
					{
						if (owner != null)
						{
							try
							{
								var dm = owner.CreateDMChannelAsync().Result;
								dm?.SendMessageAsync("Opps, It seems you have more bots than humans in your guild. BrikBot has been removed from your guild to prevent abuse, you may re-invite BrikBot once you have more humans than bots.");
							}
							catch
							{
								//Nothing
							}
						}
					}
					catch (Exception)
					{
						Logger.Nice("Event", ConsoleColor.Red, "Gateway Guild Join - Error: Failed to send bothell DM");
					}

					await guild.LeaveAsync();
				}

				if (guild.IsBlacklisted())
				{
					Logger.Nice("Event", ConsoleColor.Red, $"Gateway Guild Join - Error: Blacklisted Guild - Owner ID: {guild.Id} - Guild ID: {guild.Id}");
					try
					{
						if (owner != null)
						{
							try
							{
								var dm = owner.CreateDMChannelAsync().Result;
								dm?.SendMessageAsync("Opps, It seems your guild has been blacklisted from this bot. BrikBot has been removed from your guild to prevent abuse.\n" +
								                     $"For more information or to get un-blacklisted, please join our support discord here: {Config.Instance.URIs.DiscordURL}");
							}
							catch
							{
								//Nothing
							}
						}
					}
					catch (Exception)
					{
						Logger.Nice("Event", ConsoleColor.Red, "Gateway Guild Join - Error: Failed to send blacklist DM");
					}

					await guild.LeaveAsync();

					return;
				}

				try
				{
					await using var db = new Database();

					var success = await db.AddGuildIfDoesntExist(guild, Client);
					if (success == 1 || success == 2)
					{
						Logger.Nice("Event", ConsoleColor.Yellow, "Gateway Guild Join - Database insert success.");

						//Send server message
						try
						{
							var defaultChannel = guild.TextChannels.Where(c => guild.FindUserAsync(Client, Config.Instance.Bot.BotID).Result.GetPermissions(c).ViewChannel && guild.FindUserAsync(Client, Config.Instance.Bot.BotID).Result.GetPermissions(c).SendMessages).OrderBy(c => c.Position).FirstOrDefault();
							var builder = new EmbedBuilder()
								.WithColorType(EmbedColor.Ok)
								.WithLimitedTitle("Thanks for adding me to your server!")
								.WithLimitedDescription("To get started, you can check out our commands [here](https://brikbot.fun/commands.html).\n\n" +
								                        $"If you have any questions or need help with BrikBot, [click here]({Config.Instance.URIs.WebsiteURL}/support) to talk to our support team!");
							await Message.SendAsync(defaultChannel, embed: builder);
						}
						catch
						{
							//Nothing
						}

						//DM user
						if (owner != null)
						{
							try
							{
								var dm = owner.CreateDMChannelAsync().Result;
								dm?.SendMessageAsync(
									"Hello! Thank you for trying out BrikBot!\n" +
									"The default prefix of BrikBot is `/` (Slash Commands).\n" +
									"**Other Useful Links:**\n" +
									"<:CB_Commands:591728832508199074> **Bot Commands:** <https://brikbot.fun/commands.html>\n" +
									$"<:CB_DiscordV2:591729052562227212> **Support Discord:** <{Config.Instance.URIs.DiscordURL}>");
							}
							catch
							{
								//Nothing
							}
						}

						//Check if BrikBot has minimum require perms
						var user = await guild.FindUserAsync(Client, Config.Instance.Bot.BotID);
						if (user == null) return;
						var guildPerms = user.GuildPermissions;
						var perms = new bool[4];

						if (user.IsAdministrator())
						{
							perms[0] = true;
							perms[1] = true;
							perms[2] = true;
							perms[3] = true;
						}

						if (guildPerms.SendMessages) perms[0] = true;
						if (guildPerms.ViewChannel) perms[1] = true;
						if (guildPerms.UseExternalEmojis) perms[2] = true;
						if (guildPerms.AddReactions) perms[3] = true;

						//Lacks full perms, send DM
						if (perms.Contains(false) && guild.Owner != null)
						{
							try
							{
								var newDm = guild.FindUserAsync(Client, guild.OwnerId).Result.CreateDMChannelAsync().Result;
								newDm?.SendMessageAsync(
									$"Opps! It looks like BrikBot may be missing some required permissions in `{guild.Name?.UnescapeOnly() ?? "Unknown"}`. Some commands or functions in BrikBot may not work as expected without these permissions. You can view a list of required permissions below.\n\n" +
									"**Required Permissions:**\n" +
									$"{(perms[0] ? "<:CB_Check:437024001596981248>" : "<:CB_X:437024001785724929>")} Send Messages\n" +
									$"{(perms[1] ? "<:CB_Check:437024001596981248>" : "<:CB_X:437024001785724929>")} View Channels\n" +
									$"{(perms[2] ? "<:CB_Check:437024001596981248>" : "<:CB_X:437024001785724929>")} Use External Emoji\n" +
									$"{(perms[3] ? "<:CB_Check:437024001596981248>" : "<:CB_X:437024001785724929>")} Add Reactions");
							}
							catch
							{
								//Nothing
							}
						}
					}
					else
					{
						Logger.Nice("Event", ConsoleColor.Red, "Gateway Guild Join - Error: Failed to insert to database.");
					}
				}
				catch (Exception ex)
				{
					Logger.ErrorException(ex, guild.Id);
					Logger.Nice("Event", ConsoleColor.Red, "Gateway Guild Join - Error: Failed to send welcome DM");
				}

				Logger.Nice("Event", ConsoleColor.Green, $"Gateway Guild Join - Success - {guild.Name} ({guild.Owner?.Username ?? "Error" + "#" + guild.Owner?.Discriminator ?? "Error"}) - {guild.MemberCount} members");
				
				var builder2 = new EmbedBuilder()
					.WithLimitedTitle($"Joined {guild.Name} ({guild.Id})")
					.WithUrl(Config.Instance.URIs.WebsiteURL)
					.WithLimitedField("Owner:", $"{guild.Owner?.Username ?? "Error" + "#" + guild.Owner?.Discriminator ?? "Error"} ({guild.OwnerId})")
					.WithLimitedField("Members:", guild.MemberCount)
					.WithLimitedField("Creation Time:", guild.CreatedAt)
					.WithColorType(EmbedColor.Good);
				await Client.GetGuild(Config.Instance.Bot.MainGuildID).GetTextChannel(1066929989154508840).SendMessageAsync(embed: builder2.Build());
			}
			catch (Exception ex)
			{
				Logger.ErrorException(ex, guild.Id);
			}
		}

#pragma warning disable 1998
		private async Task OnLeave(SocketGuild guild)
#pragma warning restore 1998
		{
			try
			{
				var savedGuildData = guild;
				Logger.Nice("Event", ConsoleColor.Red, $"Gateway Guild Left - {savedGuildData.Name} ({savedGuildData.Id}) {savedGuildData.Owner?.Username ?? "null"} ({savedGuildData.OwnerId})");

				//await using var db = new Database();
				//await db.RemoveGuild(savedGuildData.Id);

				//Try to DM owner
				bool sentFeedback = false;

				try
				{
					var owner = await Client.Rest.GetUserAsync(guild.OwnerId);
					var dm = owner.CreateDMChannelAsync().Result;
					var buttons = new ComponentBuilder()
						.WithButton("Support Discord", null, ButtonStyle.Link, url: Config.Instance.URIs.DiscordURL)
						.WithSelectMenu(new SelectMenuBuilder()
							.WithCustomId("cb_kick_" + guild.Id)
							.WithPlaceholder("Select removal reason please. :)")
							.WithMinValues(1)
							.WithMaxValues(1)
							.WithOptions(new List<SelectMenuOptionBuilder>
							{
								new SelectMenuOptionBuilder()
									.WithLabel("Bot wasn't responding to commands")
									.WithValue("fb_0"),
								new SelectMenuOptionBuilder()
									.WithLabel("Missing feature(s)")
									.WithValue("fb_1"),
								new SelectMenuOptionBuilder()
									.WithLabel("Found alternative bot")
									.WithValue("fb_2"),
								new SelectMenuOptionBuilder()
									.WithLabel("Too difficult to configure/setup")
									.WithValue("fb_3"),
								new SelectMenuOptionBuilder()
									.WithLabel("Just re-inviting")
									.WithValue("fb_4"),
								new SelectMenuOptionBuilder()
									.WithLabel("Bot was spammy")
									.WithValue("fb_5"),
								new SelectMenuOptionBuilder()
									.WithLabel("Another not-listed reason")
									.WithValue("fb_6")
							})).Build();
					dm?.SendMessageAsync("```Feedback Questionnaire```\n" +
					                     $"Hello {owner.Username},\n\n" +
					                     $"You recently removed BrikBot from your server. ({savedGuildData.Name})\n\n" +
					                     "If you are simply re-inviting BrikBot you can ignore this message.\n" +
					                     "However, if you removed BrikBot due to bugs/issues or due to lack of features we'd love to hear your feedback!\n\n" +
					                     "If you'd like to provide feedback you can select an option from the list below or join our support server!", components: buttons);
					sentFeedback = true;
				}
				catch (Exception ex)
				{
					//Nothing
					Log.Debug(ex.Message + "\n" + ex.StackTrace);
				}

				if (guild.MemberCount == 0) return;
				var builder = new EmbedBuilder()
					.WithLimitedTitle($"Left {guild.Name} ({guild.Id})")
					.WithUrl(Config.Instance.URIs.WebsiteURL)
					.WithLimitedField("Owner:", $"{guild.Owner?.Username ?? "Error" + "#" + guild.Owner?.Discriminator ?? "Error"} ({guild.OwnerId})")
					.WithLimitedField("Members:", guild.MemberCount)
					.WithLimitedField("Creation Time:", guild.CreatedAt)
					.WithLimitedField("Sent Feedback:", sentFeedback)
					.WithColorType(EmbedColor.Error);
				await Client.GetGuild(Config.Instance.Bot.MainGuildID).GetTextChannel(1066929989154508840).SendMessageAsync(embed: builder.Build());
			}
			catch (Exception ex)
			{
				Logger.ErrorException(ex, guild.Id, guild.OwnerId, null, guild.Owner?.ToString());
			}
		}

		private async Task<bool> AllShardsReady()
		{
			return ReadyShards.Count == await Client.GetRecommendedShardCountAsync();;
		}
	}
}