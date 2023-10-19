using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using BrikBotCore.Extensions;
using Discord.WebSocket;

namespace BrikBotCore.Services
{
	public sealed class EventService : BaseService
	{
		private readonly BackgroundWorker _worker1;

		public EventService(DiscordShardedClient discord, Logger logger)
		{
			Client = discord;
			Logger = logger;

			//Update bot "playing" status
			_worker1 = new BackgroundWorker();
			_worker1.DoWork += UpdateBotStatus;
			var timer1 = new Timer(30.SecondsToMilliseconds()); //30s
			timer1.Elapsed += timer1_Elapsed;
			timer1.Start();
		}

		private void timer1_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (!_worker1.IsBusy)
				_worker1.RunWorkerAsync();
		}

		private void UpdateBotStatus(object sender, DoWorkEventArgs e)
		{
			try
			{
				var rng = new Random();
				var names = new List<string> {"CakeWasHere", "BrikMaster", "ForTheLulz", "BuildIt!", "NewStuffz!", "BrikCity"};
				var index = rng.Next(names.Count);
				var name = names[index];

				Client.SetGameAsync($"/help | {name} ❤");
			}
			catch (Exception ex)
			{
				Logger.ErrorException(ex);
			}
		}
	}
}