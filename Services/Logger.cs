using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrikBotCore.Enums;
using BrikBotCore.Extensions;
using BrikBotCore.Models.Internal;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace BrikBotCore.Services
{
	public sealed class Logger : BaseService
	{
		private const string Prefix = "> "; 
		
		private static string FormattedTime()
		{
			var hour = DateTime.Now.TimeOfDay.Hours;
			var minute = DateTime.Now.TimeOfDay.Minutes;
			var nicehour = hour < 10 ? "0" + hour : hour.ToString();
			var nicemin = minute < 10 ? "0" + minute : minute.ToString();
			return $"{nicehour}:{nicemin} - ";
		}

		private static void AppendPrefix()
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write(Prefix);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(FormattedTime());
		}

		public void Nice(string head, ConsoleColor col, string content)
		{
			content = Regex.Replace(content, @"[^\u0000-\u007F]+", string.Empty);

			lock (this)
			{
				AppendPrefix();
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(@"[");
				Console.ForegroundColor = col;
				Console.Write(head.ToUpper());
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(@"] >> ");
				Console.WriteLine(content);
				Log.Verbose($"[{head.ToUpper()}] >> {content}");
			}
		}

		public async void ErrorException(Exception ex, ulong guildId = 0, ulong userId = 0, ISocketMessageChannel channel = null, string notes = null)
		{
			if (ex.Message == "The server responded with error 50001: Missing Access") return;
			lock (this)
			{
				if (ex.InnerException != null) ex = ex.InnerException;

				AppendPrefix();
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(@"[");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write(@"ERROR");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(@"] >> ");
				Console.WriteLine(ex.Message);
				AppendPrefix();
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(@"[");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write(@"ERROR");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(@"] >> ");
				Console.WriteLine(ex.StackTrace);
				Log.Error($"[ERROR] >> {ex.Message}\n[ERROR] >> {ex.StackTrace}");
			}

			switch (ex.Message)
			{
				case "The server responded with error 10003: Unknown Channel":
					return;
				case "The operation has timed out.":
				{
					if (channel != null) await SendErrorAsync(channel, "Core | Command", "Timed out while trying to run the command.", guildId);
					break;
				}
			}
		}

		private async Task SendErrorAsync(ISocketMessageChannel channel, string title, string message, ulong guildId = 0) 
		{ 
			try 
			{ 
				if (message.IsNull() || title.IsNull()) return; 
 
				if (!(channel is IMessageChannel c) || !CanSendMessage(c)) return; 
 
				await using var db = new Database();

				var builder = new EmbedBuilder() 
					.WithLimitedTitle(title) 
					.WithUrl(Config.Instance.URIs.WebsiteURL) 
					.WithColorType(EmbedColor.Error) 
					.WithLimitedField("Error: ", message); 
 
				await channel.SendMessageAsync("", false, builder.Build()); 
			} 
			catch (Exception ex) 
			{ 
				ErrorException(ex, 0, 0, channel); 
			} 
		} 
 
		private bool CanSendMessage(IChannel channel) 
		{ 
			try 
			{ 
				var guild = ((SocketGuildChannel) channel)?.Guild; 
 
				if (guild?.CurrentUser == null) return false; 
 
				var chanPerms = guild.CurrentUser.GetPermissions((IGuildChannel) channel); 
 
				if (guild.CurrentUser.IsAdministrator()) return true; 
 
				return chanPerms.ViewChannel && chanPerms.ReadMessageHistory && chanPerms.SendMessages && chanPerms.EmbedLinks; 
			} 
			catch (Exception ex) 
			{ 
				ErrorException(ex, 0, 0, channel as ISocketMessageChannel); 
				return false; 
			} 
		} 
		
		public void Notify(string msg)
		{
			Log.Verbose($@"
	---------\\\\ {msg} ////---------
");
		}
	}
}