using System;
using BrikBotCore.Extensions;
using BrikBotCore.Models.Database;
using BrikBotCore.Services;
using LazyCache;
using Serilog;

namespace BrikBotCore.Cache
{
	public class DataCache
	{
		private readonly IAppCache _cache;

		public DataCache(IAppCache cache)
		{
			_cache = cache;
		}

		public GuildData GetGuild(ulong guildId)
		{
			try
			{
				var stringId = guildId.ToString();
				Func<GuildData> loadedGuild = () => LoadGuildSettings(stringId);
				var cachedResult = _cache.GetOrAdd(stringId, loadedGuild, DateTimeOffset.Now.AddMinutes(1));
				if (cachedResult == null)
				{
					//TODO: Add guild to database
					//using var db = new Database();
					//db.AddGuildIfDoesntExist(Client.GetGuild(guildId));
					Log.Debug("Guild not found in cache or database. Adding to database...");
					return null;
				}

				return cachedResult;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}

		public bool IsCached(ulong guildId)
		{
			_cache.TryGetValue(guildId.ToString(), out GuildData val);
			return val != null;
		}

		private GuildData LoadGuildSettings(string guildId)
		{
			try
			{
				//Log.Debug($"[CACHE] Adding guild to cache: {guildId}");
				using var db = new Database();
				var response = db.GetGuildData(Convert.ToUInt64(guildId)).Result;
				return response;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}
	}
}