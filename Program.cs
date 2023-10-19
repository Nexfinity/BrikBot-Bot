using System;
using System.Threading.Tasks;
using BrikBotCore.Cache;
using BrikBotCore.Extensions;
using BrikBotCore.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Filters;
using RunMode = Discord.Commands.RunMode;

namespace BrikBotCore
{
	public class Program
	{
		public static void Main()
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
				new Program().MainAsync().GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				Log.Debug($"!!! POSSIBLE UNHANDLED EXCEPTION !!!\n{e.Message} \n {e.StackTrace}", "Red");
			}
		}

		private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Console.WriteLine(e.ExceptionObject.ToString());
			}
			catch (Exception ex)
			{
				Log.Debug($"!!! UNHANDLED EXCEPTION !!!\n{ex.Message} \n {ex.StackTrace}", "Red");
			}
		}

		private async Task MainAsync()
		{
			try
			{
				Log.Logger = new LoggerConfiguration()
					.MinimumLevel.Verbose()
					.Enrich.FromLogContext()
					.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}")
					.WriteTo.File("logs/log-.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10, retainedFileTimeLimit: TimeSpan.FromDays(7))
					.CreateLogger();
				
				var services = new ServiceCollection();
				ConfigureServices(services);
				var provider = services.BuildServiceProvider();
				await provider.GetRequiredService<CommandHandler>().InitializeAsync();
				provider.GetRequiredService<DiscordShardedClient>();
				provider.GetRequiredService<InteractionService>();
				provider.GetRequiredService<EventService>();
				await provider.GetRequiredService<StartupService>().StartAsync(provider, services);

				await Task.Delay(-1);
			}
			catch (Exception e)
			{
				Log.Debug("{Message} \n {StackTrace}", e.Message, e.StackTrace);
			}
		}

		private void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(new DiscordShardedClient(new DiscordSocketConfig
			{
				LogLevel = LogSeverity.Verbose,
				MessageCacheSize = 0,
				TotalShards = null,
				DefaultRetryMode = RetryMode.AlwaysFail,
				AlwaysDownloadUsers = true,
				HandlerTimeout = 3000,
				ConnectionTimeout = int.MaxValue,
				IdentifyMaxConcurrency = 1,
				MaxWaitBetweenGuildAvailablesBeforeReady = 10000,
				LargeThreshold = 250,
				GatewayIntents = GatewayIntents.Guilds | GatewayIntents.DirectMessages,
				UseInteractionSnowflakeDate = true,
				UseSystemClock = true,
				AlwaysResolveStickers = false,
				AlwaysDownloadDefaultStickers = false,
				LogGatewayIntentWarnings = true,
				DefaultRatelimitCallback = x =>
				{
					//Log.Debug("[Ratelimit] Endpoint: {Endpoint} - Limit: {Limit} - Remaining: {Remaining} - ResetAfter: {ResetAfter} - isGlobal? {IsGlobal} - Bucket: {Bucket}", x?.Endpoint ?? "Unknown", x?.Limit ?? -1, x?.Remaining ?? -1, x?.ResetAfter ?? TimeSpan.Zero, x?.IsGlobal ?? false, x?.Bucket ?? "Unknown");
					return Task.CompletedTask;
				}
			}));
			services.AddSingleton(new CommandService(new CommandServiceConfig
			{
				DefaultRunMode = RunMode.Async,
				LogLevel = LogSeverity.Verbose,
				IgnoreExtraArgs = true,
				CaseSensitiveCommands = false,
				ThrowOnError = true,
				SeparatorChar = ' '
			}));
			services.AddSingleton<EventService>();
			services.AddSingleton<StartupService>();
			services.AddLazyCache();
			services.AddSingleton<Logger>();
			services.AddSingleton<MessageSender>();
			services.AddSingleton<DataCache>();
			services.AddSingleton<InteractionService>();
			services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>()));
			services.AddSingleton<CommandHandler>();
		}
	}
}