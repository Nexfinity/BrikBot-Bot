using System;
using BrikBotCore.Cache;
using BrikBotCore.Extensions;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace BrikBotCore.Services
{
	[Inject]
	public class BaseService
	{
		//C#
		internal virtual IServiceProvider ServiceProvider { get; set; }

		//DNet
		internal virtual DiscordShardedClient Client { get; set; }
		internal virtual CommandService Commands { get; set; }
		internal virtual InteractionService Interaction { get; set; }

		//Brik - Services
		internal virtual Logger Logger { get; set; }

		//Brik - Extensions
		internal virtual MessageSender Message { get; set; }

		//Brik - Cache
		internal virtual DataCache Cache { get; set; }
	}

	public class InjectAttribute : Attribute
	{
	}
}