using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrikBotCore.Attributes;
using BrikBotCore.Models.Database;
using BrikBotCore.Models.Internal;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Serilog;

#pragma warning disable 1998

namespace BrikBotCore.Services
{
	public class Database : DbContext
	{
		private readonly MySqlConnection _conn = new MySqlConnection(Config.Instance.DbConnectionString);

		private void OpenConnection()
		{
			try
			{
				if (_conn.State == ConnectionState.Closed)
					_conn.Open();
			}
			catch (MySqlException e)
			{
				switch (e.Number)
				{
					case 0:
						Log.Debug($"[MySQL Error] [{e.Number}] [{GetExceptionNumber(e)}] Cannot connect to server.", "Red");
						break;

					case 1045:
						Log.Debug($"[MySQL Error] [{e.Number}] Invalid username/password.", "Red");
						break;

					default:
						Log.Debug($"[MySQL Error] [{e.Number}] " + e.Message + "\n" + e.StackTrace, "Red");
						break;
				}
			}
		}

		private static int GetExceptionNumber(MySqlException ex)
		{
			if (ex == null) return -1;
			var number = ex.Number;
			if (number == 0 && (ex = ex.InnerException as MySqlException) != null) number = ex.Number;
			return number;
		}

		private void CloseConnection()
		{
			try
			{
				if (_conn.State == ConnectionState.Open)
					_conn.Close();
			}
			catch (MySqlException e)
			{
				Log.Error("[MySQL Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
			}
		}

		public async Task<int> GuildExists(ulong guildId)
		{
			try
			{
				OpenConnection();

				const string sql = "SELECT GuildID FROM Guilds WHERE GuildID=@GuildID";
				var cmd = new MySqlCommand(sql, _conn);

				cmd.Parameters.AddWithValue("@GuildID", guildId);

				var dt = new DataTable();
				dt.Load(cmd.ExecuteReader());
				var data = dt.AsEnumerable().ToArray();

				return data.Length > 0 ? 1 : 0;
			}
			catch (MySqlException e)
			{
				Log.Error("[MySQL Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
				return 0;
			}
			catch (Exception e)
			{
				Log.Error("[Database Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
				return 0;
			}
			finally
			{
				CloseConnection();
			}
		}

		public async Task<int> AddGuildIfDoesntExist(SocketGuild guild, DiscordShardedClient client)
		{
			try
			{
				if (await GuildExists(guild.Id) == 1)
				{
					Log.Debug("DEBUG4: Guild exists in DB, Updating values");
					OpenConnection();
					string ownerName;
					if (guild.Owner == null)
					{
						//guild.FindUserAsync(guild.OwnerId);
						Log.Debug($"DEBUG4: Owner null... ({guild.DownloadedMemberCount}/{guild.MemberCount}) - ({guild.HasAllMembers})");
						//var restOwner = client.Rest.GetUserAsync(guild.OwnerId)?.Result?.Username ?? "Error";
						//Log.Debug($"DEBUG4: Owner null... {restOwner}");
						//await Task.Delay(TimeSpan.FromSeconds(3));
						//ownerName = restOwner;
						ownerName = "Error";
					}
					else
					{
						ownerName = guild.Owner?.ToString() ?? "Error";
					}
					
					var ownerId = guild.OwnerId;

					var plain = Encoding.UTF8.GetBytes(ownerName);
					var owner = Convert.ToBase64String(plain);

					var plain2 = Encoding.UTF8.GetBytes(guild.Name);
					var name = Convert.ToBase64String(plain2);
					const string sql = "UPDATE Guilds SET GuildName=@GuildName, OwnerID=@OwnerID, OwnerName=@OwnerName, MemberCount=@MemberCount WHERE GuildID=@GuildID";
					var cmd = new MySqlCommand(sql, _conn);

					cmd.Parameters.AddWithValue("@GuildID", guild.Id);
					cmd.Parameters.AddWithValue("@GuildName", name);
					cmd.Parameters.AddWithValue("@OwnerID", ownerId);
					cmd.Parameters.AddWithValue("@OwnerName", owner);
					cmd.Parameters.AddWithValue("@MemberCount", guild.MemberCount);

					if (cmd.ExecuteNonQuery() == 1) CloseConnection();
					return 2;
				}
				else
				{
					Log.Debug("DEBUG4: Guild missing, Adding guild");
					OpenConnection();
					//Guild didn't exist, add it
					string ownerName;
					if (guild.Owner == null)
					{
						//guild.FindUserAsync(guild.OwnerId);
						Log.Debug($"DEBUG4: Owner null... ({guild.DownloadedMemberCount}/{guild.MemberCount}) - ({guild.HasAllMembers})");
						//var restOwner = client.Rest.GetUserAsync(guild.OwnerId)?.Result?.Username ?? "Error";
						//Log.Debug($"DEBUG4: Owner null... {restOwner}");
						//await Task.Delay(TimeSpan.FromSeconds(3));
						//ownerName = restOwner;
						ownerName = "Error";
					}
					else
					{
						ownerName = guild.Owner?.ToString() ?? "Error";
					}
					
					var ownerId = guild.OwnerId;

					var plain = Encoding.UTF8.GetBytes(ownerName);
					var owner = Convert.ToBase64String(plain);

					var plain2 = Encoding.UTF8.GetBytes(guild.Name);
					var name = Convert.ToBase64String(plain2);
					const string sql = "INSERT INTO Guilds (GuildID, GuildName, OwnerID, Prefix, OwnerName, MemberCount, GuildCreateTime) VALUES (@GuildID, @GuildName, @OwnerID, @Prefix, @OwnerName, @MemberCount, @GuildCreateTime)";
					var cmd = new MySqlCommand(sql, _conn);

					cmd.Parameters.AddWithValue("@GuildID", guild.Id);
					cmd.Parameters.AddWithValue("@GuildName", name);
					cmd.Parameters.AddWithValue("@OwnerID", ownerId);
					cmd.Parameters.AddWithValue("@Prefix", "!");
					cmd.Parameters.AddWithValue("@OwnerName", owner);
					cmd.Parameters.AddWithValue("@MemberCount", guild.MemberCount);
					cmd.Parameters.AddWithValue("@GuildCreateTime", guild.CreatedAt.ToString());

					var success = 0;
					if (cmd.ExecuteNonQuery() == 1)
						success = 1;

					CloseConnection();

					return success;
				}
			}
			catch (MySqlException e)
			{
				Log.Error("[MySQL Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
				return 0;
			}
			catch (Exception e)
			{
				Log.Error("[Database Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
				return 0;
			}
		}

		//Used for cache
		public async Task<GuildData> GetGuildData(ulong guildId)
		{
			try
			{
				OpenConnection();

				const string sql = "SELECT * FROM Guilds WHERE GuildID=@GuildID LIMIT 1;";
				var cmd = new MySqlCommand(sql, _conn);

				cmd.Parameters.AddWithValue("@GuildID", guildId);

				var dt = new DataTable();
				dt.Load(cmd.ExecuteReader());
				var data = dt.AsEnumerable().ToArray();

				CloseConnection();

				if (data.Length == 0)
				{
					//TODO, if null, add guild to database
					return null;
				}

				var mapper = new DataMapper<GuildData>();
				var responseData = mapper.Map(data[0]);
				return responseData;
			}
			catch (MySqlException e)
			{
				Log.Error("[MySQL Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
				return null;
			}
			catch (Exception e)
			{
				Log.Error("[Database Error] {Message}\n{StackTrace}", e.Message, e.StackTrace);
				return null;
			}
		}
	}
}
#pragma warning restore 1998