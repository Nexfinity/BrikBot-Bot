using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using BrikBotCore.Extensions;
using Serilog;

namespace BrikBotCore.Services
{
	public class CommandHandler
	{
		private readonly DiscordShardedClient _client;
		private readonly InteractionService _commands;
		private readonly IServiceProvider _services;
		private readonly Logger _log;

		public CommandHandler(DiscordShardedClient client, InteractionService commands, IServiceProvider services, Logger log)
		{
			_client = client;
			_commands = commands;
			_services = services;
			_log = log;
			//_commands.Log += DebugErrorHandler;
		}

		// private async Task DebugErrorHandler(LogMessage arg)
		// {
		// 	Log.Debug(arg.Exception.Message + "\n" + arg.Exception.StackTrace);
		// }

		public async Task InitializeAsync()
		{
			try
			{
				// Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
				//Log.Debug("[Interactions] Loading modules...");
				await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
				// Another approach to get the assembly of a specific type is:
				// typeof(CommandHandler).Assembly


				// Process the InteractionCreated payloads to execute Interactions commands
				//Log.Debug("[Interactions] Registering event handlers...");
				_client.InteractionCreated += HandleInteraction;

				// Process the command execution results 
				_commands.SlashCommandExecuted += SlashCommandExecuted;
				_commands.ContextCommandExecuted += ContextCommandExecuted;
				_commands.ComponentCommandExecuted += ComponentCommandExecuted;
				//_commands.AutocompleteHandlerExecuted += AutocompleteHandlerExecuted;
				//_commands.AutocompleteCommandExecuted += AutocompleteCommandExecuted;
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex);
			}
		}

		# region Error Handling

		private Task ComponentCommandExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result)
		{
			try
			{
				Log.Information("[ComponentCommand] {Username} ({UserId}) ran {Command} in {GuildName} ({GuildId}) - {ChannelName} ({ChannelId})", context.User?.Username ?? "Unknown", context.User?.Id ?? 0, commandInfo?.Name ?? "Unknown", context.Guild?.Name ?? "Unknown", context.Guild?.Id ?? 0, context.Channel?.Name ?? "Unknown", context.Channel?.Id ?? 0);
				if (result.IsSuccess) return Task.CompletedTask;
				switch (result.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						context.Interaction.RespondAsync("**Error:**\nUnmet precondition: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.UnknownCommand:
						//context.Interaction.RespondAsync("**Error:**\nUnknown command: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.BadArgs:
						context.Interaction.RespondAsync("**Error:**\n" + "Has incorrect number of parameters.", ephemeral: true);
						break;
					case InteractionCommandError.Exception:
						context.Interaction.RespondAsync("**Error:**\nException: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.Unsuccessful:
						context.Interaction.RespondAsync("**Error:**\nUnsuccessful: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.ConvertFailed:
						context.Interaction.RespondAsync("**Error:**\nConvsion failed.", ephemeral: true);
						break;
					case InteractionCommandError.ParseFailed:
						context.Interaction.RespondAsync("**Error:**\n" + "You must provide a valid number.", ephemeral: true);
						break;
					default:
						context.Interaction.RespondAsync("**Error:**\n" + "Unknown error." + result.Error, ephemeral: true);
						break;
				}
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex);
			}

			return Task.CompletedTask;
		}

		private Task ContextCommandExecuted(ContextCommandInfo commandInfo, IInteractionContext context, IResult result)
		{
			try
			{
				Log.Information("[ContextCommand] {Username} ({UserId}) ran {Command} in {GuildName} ({GuildId}) - {ChannelName} ({ChannelId})", context.User?.Username ?? "Unknown", context.User?.Id ?? 0, commandInfo?.Name ?? "Unknown", context.Guild?.Name ?? "Unknown", context.Guild?.Id ?? 0, context.Channel?.Name ?? "Unknown", context.Channel?.Id ?? 0);
				if (result.IsSuccess) return Task.CompletedTask;
				switch (result.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						context.Interaction.RespondAsync("**Error:**\nUnmet precondition: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.UnknownCommand:
						//context.Interaction.RespondAsync("**Error:**\nUnknown command: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.BadArgs:
						context.Interaction.RespondAsync("**Error:**\n" + "Has incorrect number of parameters.", ephemeral: true);
						break;
					case InteractionCommandError.Exception:
						context.Interaction.RespondAsync("**Error:**\nException: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.Unsuccessful:
						context.Interaction.RespondAsync("**Error:**\nUnsuccessful: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.ConvertFailed:
						context.Interaction.RespondAsync("**Error:**\nConvsion failed.", ephemeral: true);
						break;
					case InteractionCommandError.ParseFailed:
						context.Interaction.RespondAsync("**Error:**\n" + "You must provide a valid number.", ephemeral: true);
						break;
					default:
						context.Interaction.RespondAsync("**Error:**\n" + "Unknown error." + result.Error, ephemeral: true);
						break;
				}
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex);
			}

			return Task.CompletedTask;
		}

		private Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
		{
			try
			{
				
				Log.Information("[SlashCommand] {Username} ({UserId}) ran /{Command} in {GuildName} ({GuildId}) - {ChannelName} ({ChannelId})", context.User?.Username ?? "Unknown", context.User?.Id ?? 0, commandInfo?.Name ?? "Unknown", context.Guild?.Name ?? "Unknown", context.Guild?.Id ?? 0, context.Channel?.Name ?? "Unknown", context.Channel?.Id ?? 0);
				if (result.IsSuccess) return Task.CompletedTask;
				switch (result.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						context.Interaction.RespondAsync("**Error:**\nUnmet precondition: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.UnknownCommand:
						//context.Interaction.RespondAsync("**Error:**\nUnknown command: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.BadArgs:
						context.Interaction.RespondAsync("**Error:**\n" + "Has incorrect number of parameters.", ephemeral: true);
						break;
					case InteractionCommandError.Exception:
						context.Interaction.RespondAsync("**Error:**\nException: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.Unsuccessful:
						context.Interaction.RespondAsync("**Error:**\nUnsuccessful: " + result.ErrorReason, ephemeral: true);
						break;
					case InteractionCommandError.ConvertFailed:
						context.Interaction.RespondAsync("**Error:**\nConvsion failed.", ephemeral: true);
						break;
					case InteractionCommandError.ParseFailed:
						context.Interaction.RespondAsync("**Error:**\n" + "You must provide a valid number.", ephemeral: true);
						break;
					default:
						context.Interaction.RespondAsync("**Error:**\n" + "Unknown error." + result.Error, ephemeral: true);
						break;
				}
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex);
			}

			return Task.CompletedTask;
		}

		# endregion

		# region Execution
		private async Task HandleInteraction(SocketInteraction arg)
		{
			try
			{
				// Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
				var ctx = new ShardedInteractionContext(_client, arg);
				//Log.Debug("[Interactions] Context created");
	
				//Check if it's an archived thread
				if (ctx.Interaction.Channel is SocketThreadChannel { IsArchived: true }) return;

				if(ctx.User.IsCommandBlacklisted())
				{
					await ctx.Interaction.RespondAsync("You have been blacklisted from this command. :(", null, false, true, AllowedMentions.None);
					return;
				}
				
				//Log.Debug("[Interactions] Executing");
				var result = await _commands.ExecuteCommandAsync(ctx, _services);
				//Log.Debug("[Interactions] Executed");
				//Get command length

				//Log.Debug($"[Interactions] Result: {result.IsSuccess} / {result.ErrorReason} / {(result.Error.HasValue ? result.Error.Value.ToString() : "No error")}");
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex);

				// If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
				// response, or at least let the user know that something went wrong during the command execution.
				if (arg.Type == InteractionType.ApplicationCommand)
				{
					var msg = await arg.GetOriginalResponseAsync();
					await msg.DeleteAsync();
				}
			}
		}

		# endregion
	}
}