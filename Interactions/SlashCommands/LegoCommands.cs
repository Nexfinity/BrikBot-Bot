using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BrikBotCore.Enums;
using BrikBotCore.Extensions;
using BrikBotCore.Models.API;
using BrikBotCore.Models.Internal;
using BrikBotCore.Services;
using Discord;
using Discord.Interactions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Serilog;

namespace BrikBotCore.Interactions.SlashCommands
{
	public class LegoCommands : InteractionModuleBase<ShardedInteractionContext>
	{
		private readonly Logger _log;
		
		public LegoCommands(Logger log)
		{
			_log = log;
		}

		public enum LookupType
		{
			SetID,
			SetNumber
		}

		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("set", "Displays various information about the given set ID.")]
		public async Task Set(LookupType lookupType, string query)
		{
			try
			{
				var client = new HttpClient();
				client.Timeout = TimeSpan.FromSeconds(10);
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri($"https://brickset.com/api/v3.asmx/getSets?apiKey={Config.Instance.Bot.BrikSetApiKey}&userHash="),
					Headers =
					{
						{ "Accept", "application/json" }
					}
				};

				object jsonParams;
				if (lookupType == LookupType.SetID)
				{
					jsonParams = new
					{
						setID = query,
						orderBy = "Pieces",
						PageSize = "1"
					};
				}
				else
				{
					if (!query.Contains("-")) query += "-1";
					jsonParams = new
					{
						setNumber = query,
						orderBy = "Pieces",
						PageSize = "1"
					};
				}

				var jsonParamsString = JsonConvert.SerializeObject(jsonParams);
				request.RequestUri = new Uri(request.RequestUri + "&params=" + jsonParamsString);
				
				using var response = client.SendAsync(request).Result;
				var jsonResponse = await response.Content.ReadAsStringAsync();
				
				Log.Debug(jsonResponse);
				
				var data = JsonConvert.DeserializeObject<BrickSetGetSets>(jsonResponse);

				if (data == null || data.matches == 0 || data.sets.Count == 0)
				{
					await RespondAsync("Failed to find any sets with the given ID or number.", ephemeral: true);
					return;
				}
				
				// var priceGuide = await _bricklink.GetPriceGuideAsync(ItemType.Set, data.sets[0].number, colorId: 1, priceGuideType: PriceGuideType.Stock, condition: Condition.New);
				// Log.Debug(priceGuide.ToJson());
				var usdPrice = $"{data.sets[0]?.LEGOCom?.US?.retailPrice.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}";
				var ukPrice = $"{data.sets[0]?.LEGOCom?.UK?.retailPrice.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}";

				if (usdPrice == "0") usdPrice = "Unknown";
				if (ukPrice == "0") ukPrice = "Unknown";

				EmbedBuilder builder = new EmbedBuilder()
					.WithLimitedTitle($"{data.sets[0].setID} | {data.sets[0].number}-{data.sets[0].numberVariant} | {data.sets[0].name}")
					.WithUrl(data.sets[0].bricksetURL)
					.WithColorType(EmbedColor.Ok)
					.WithThumbnailUrl(data.sets[0].image.thumbnailURL)
					.WithLimitedField("General:", $"Released in **{data.sets[0].year}**, belongs to the **{data.sets[0].category}** category.", false)
					.WithLimitedField("Pieces:", $"Made of **{data.sets[0].pieces}** parts.", true)
					.WithLimitedField("Minifigures:", $"Contains **{data.sets[0].minifigs}** minifigures.", true)
					.WithLimitedField("Price:", $"**${usdPrice}** / **£{ukPrice}**", false)
					.WithFooter($"Source: Brickset • Last updated: {data.sets[0].lastUpdated}");
				builder.WithLimitedField("Links:", $"[Brickset]({data.sets[0].bricksetURL}) • [Bricklink](https://www.bricklink.com/v2/catalog/catalogitem.page?S={data.sets[0].number}-{data.sets[0].numberVariant}) • [BrickInsight](https://brickinsights.com/sets/{data.sets[0].number}-{data.sets[0].numberVariant})", false);
				if (data.sets[0].extendedData != null && data.sets[0].extendedData.notes != null) builder.WithLimitedField("Notes:", data.sets[0].extendedData.notes);

				if (data.sets[0].instructionsCount > 0)
				{
					//Get instructions
					var client2 = new HttpClient();
					client2.Timeout = TimeSpan.FromSeconds(10);
					var request2 = new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = new Uri($"https://brickset.com/api/v3.asmx/getInstructions?apiKey={Config.Instance.Bot.BrikSetApiKey}&userHash=&setID=" + data.sets[0].setID),
						Headers =
						{
							{ "Accept", "application/json" }
						}
					};
				
					using var response2 = client2.SendAsync(request2).Result;
					var jsonResponse2 = await response2.Content.ReadAsStringAsync();
				
					Log.Debug(jsonResponse2);
				
					var data2 = JsonConvert.DeserializeObject<BrickSetGetInstructions>(jsonResponse2);
					if (data2 != null && data2.matches > 0)
					{
						var formatted = data2.instructions.Aggregate("", (current, instruction) => current + $"[{instruction.description}]({instruction.URL})\n");
						builder.WithLimitedField("Instructions:", formatted);
					}
				}

				await RespondAsync("", new[] { builder.Build() });
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("part", "Displays various information about the given part ID.")]
		public async Task Part(string partid)
		{
			try
			{
				await DeferAsync();
				var client = new HttpClient();
				client.Timeout = TimeSpan.FromSeconds(10);
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("https://rebrickable.com/api/v3/lego/parts/" + partid + "?key=bfb0da100cfa57881ceb859baa6020c8"),
					Headers =
					{
						{ "Accept", "application/json"}
					}
				};

				using var response2 = client.SendAsync(request).Result;
				var jsonResponse2 = await response2.Content.ReadAsStringAsync();

				if (jsonResponse2.Contains("Not found."))
				{
					await FollowupAsync("Failed to find any parts with the given ID.");
					return;
				}

				Log.Debug(jsonResponse2);
				
				var data = JsonConvert.DeserializeObject<RebrickablePart>(jsonResponse2);
				
				if (data == null)
				{
					await FollowupAsync("Failed to find any parts with the given ID.");
					return;
				}

				var status = data.year_to == DateTime.Now.Year ? "🟢 Still in Production" : "🔴 No Longer in Production";

				var similars = "";
				similars = data.alternates.Any() ? data.alternates.Aggregate(similars, (current, alternate) => current + $"[{alternate}](https://rebrickable.com/parts/{alternate})") : "No similar parts.";

				EmbedBuilder builder = new EmbedBuilder()
					.WithLimitedTitle($"{data.part_num} | {data.name}")
					.WithUrl("https://rebrickable.com/parts/" + data.part_num)
					.WithColorType(EmbedColor.Ok)
					.WithThumbnailUrl(data.part_img_url)
					.WithLimitedField("Status:", status, false)
					.WithLimitedField("Production:", $"Released in **{data.year_from}** and produced until atleast **{data.year_to}**.", false)
					.WithLimitedField("Colors:", $"[Bricklink Color List](https://www.bricklink.com/catalogItemIn.asp?P={data.part_num}&v=3&in=S)", false)
					.WithLimitedField("Similar To:", similars, false)
					.WithLimitedField("Shop:", $"[Brickset](https://brickset.com/parts/{data.external_ids.Brickset[0]}) • [Bricklink](https://www.bricklink.com/v2/catalog/catalogitem.page?P={data.external_ids.BrickLink[0]}) • [BrickOwl](https://www.brickowl.com/search/catalog?query={data.external_ids.BrickOwl[0]}) • [Lego PaB](https://www.lego.com/en-us/pick-and-build/pick-a-brick?query={data.external_ids.LEGO[0]})", false)
					.WithFooter($"Source: Rebrickable");

				await FollowupAsync("", new[] { builder.Build() });
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
		
		public enum SearchType
		{
			Part,
			Set
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("search", "Search for a set or part by name instead of ID.")]
		public async Task Search(SearchType type, string query)
		{
			try
			{
				EmbedBuilder builder;
				switch (type)
				{
					case SearchType.Part:
						await RespondAsync("This command is coming soon.", ephemeral: true);
						break;
					case SearchType.Set:
						await RespondAsync("This command is coming soon.", ephemeral: true);
						break;
					default:
						await RespondAsync("Unknown search type. Please try again.");
						break;
				}
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("setoftheday", "Displays Brickset's random set of the day.")]
		public async Task SetOfTheDay()
		{
			try
			{
				await DeferAsync();
				var url = "https://brickset.com/news/category-Random-set";
				var web = new HtmlWeb();
				var doc = web.Load(url);
				
				//Log.Debug("Node count: " + doc?.DocumentNode?.SelectNodes(@"//*[@id='body']/div[1]/div/div/section/div[2]/article[1]/p[2]/a[1]")?.Count);
				//Log.Debug("Node count: " + doc?.DocumentNode?.SelectSingleNode(@"//*[@id='body']/div[1]/div/div/section/div[2]/article[1]/p[2]/a[1]")?.Attributes?["href"]?.Value);
				var rawSetValue = doc?.DocumentNode?.SelectSingleNode(@"//*[@id='body']/div[1]/div/div/section/div[2]/article[1]/p[2]/a[1]")?.Attributes?["href"]?.Value;
				var setNumber2 = rawSetValue.Replace("/sets/", string.Empty);
				
				var client = new HttpClient();
				client.Timeout = TimeSpan.FromSeconds(10);
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri($"https://brickset.com/api/v3.asmx/getSets?apiKey={Config.Instance.Bot.BrikSetApiKey}&userHash="),
					Headers =
					{
						{ "Accept", "application/json" }
					}
				};

				object jsonParams = new
				{
					setNumber = setNumber2,
					orderBy = "Pieces",
					PageSize = "1"
				};

				var jsonParamsString = JsonConvert.SerializeObject(jsonParams);
				request.RequestUri = new Uri(request.RequestUri + "&params=" + jsonParamsString);
				
				using var response2 = client.SendAsync(request).Result;
				var jsonResponse2 = await response2.Content.ReadAsStringAsync();
				
				Log.Debug(jsonResponse2);
				
				var data = JsonConvert.DeserializeObject<BrickSetGetSets>(jsonResponse2);

				if (data == null || data.matches == 0 || data.sets.Count == 0)
				{
					await FollowupAsync("Failed to find the set of the day.");
					return;
				}
				
				// var priceGuide = await _bricklink.GetPriceGuideAsync(ItemType.Set, data.sets[0].number, colorId: 1, priceGuideType: PriceGuideType.Stock, condition: Condition.New);
				// Log.Debug(priceGuide.ToJson());
				var usdPrice = $"{data.sets[0]?.LEGOCom?.US?.retailPrice.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}";
				var ukPrice = $"{data.sets[0]?.LEGOCom?.UK?.retailPrice.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}";

				if (usdPrice == "0") usdPrice = "Unknown";
				if (ukPrice == "0") ukPrice = "Unknown";

				EmbedBuilder builder = new EmbedBuilder()
					.WithLimitedTitle($"{data.sets[0].setID} | {data.sets[0].number}-{data.sets[0].numberVariant} | {data.sets[0].name}")
					.WithUrl(data.sets[0].bricksetURL)
					.WithColorType(EmbedColor.Ok)
					.WithThumbnailUrl(data.sets[0].image.thumbnailURL)
					.WithLimitedField("General:", $"Released in **{data.sets[0].year}**, belongs to the **{data.sets[0].category}** category.", false)
					.WithLimitedField("Pieces:", $"Made of **{data.sets[0].pieces}** parts.", true)
					.WithLimitedField("Minifigures:", $"Contains **{data.sets[0].minifigs}** minifigures.", true)
					.WithLimitedField("Price:", $"**${usdPrice}** / **£{ukPrice}**", false)
					.WithFooter($"Source: Brickset • Last updated: {data.sets[0].lastUpdated}");
				builder.WithLimitedField("Links:", $"[Brickset]({data.sets[0].bricksetURL}) • [Bricklink](https://www.bricklink.com/v2/catalog/catalogitem.page?S={data.sets[0].number}-{data.sets[0].numberVariant}) • [BrickInsight](https://brickinsights.com/sets/{data.sets[0].number}-{data.sets[0].numberVariant})", false);
				if (data.sets[0].extendedData != null && data.sets[0].extendedData.notes != null) builder.WithLimitedField("Notes:", data.sets[0].extendedData.notes);
				
				await FollowupAsync("", new[] { builder.Build() });
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("brickset", "Get a direct link to the Brickset page for the set via ID or number.")]
		public async Task Brickset(LookupType lookupType, string query)
		{
			try
			{
				if (lookupType == LookupType.SetID)
				{
					//Get set number from ID
					var client = new HttpClient();
					client.Timeout = TimeSpan.FromSeconds(10);
					var request = new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = new Uri($"https://brickset.com/api/v3.asmx/getSets?apiKey={Config.Instance.Bot.BrikSetApiKey}&userHash="),
						Headers =
						{
							{ "Accept", "application/json" }
						}
					};

					object jsonParams = new
					{
						setID = query,
						orderBy = "Pieces",
						PageSize = "1"
					};

					var jsonParamsString = JsonConvert.SerializeObject(jsonParams);
					request.RequestUri = new Uri(request.RequestUri + "&params=" + jsonParamsString);
				
					using var response2 = client.SendAsync(request).Result;
					var jsonResponse2 = await response2.Content.ReadAsStringAsync();

					var data = JsonConvert.DeserializeObject<BrickSetGetSets>(jsonResponse2);

					if (data == null || data.matches == 0 || data.sets.Count == 0)
					{
						await RespondAsync("Failed to find any sets with the given ID.", ephemeral: true);
						return;
					}

					query = $"{data.sets[0].number}-{data.sets[0].numberVariant}";
				}

				if (!query.Contains("-")) query += "-1";
				var buttons = new ComponentBuilder().WithButton("Open Link", null, ButtonStyle.Link, url: "https://brickset.com/sets/" + query);
				await RespondAsync("Click the button below to open the Brickset website.", components: buttons.Build());
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("bricklink", "Get a direct link to the Bricklink page for the set.")]
		public async Task Bricklink(LookupType lookupType, string query)
		{
			try
			{
				if (lookupType == LookupType.SetID)
				{
					//Get set number from ID
					var client = new HttpClient();
					client.Timeout = TimeSpan.FromSeconds(10);
					var request = new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = new Uri($"https://brickset.com/api/v3.asmx/getSets?apiKey={Config.Instance.Bot.BrikSetApiKey}&userHash="),
						Headers =
						{
							{ "Accept", "application/json" }
						}
					};

					object jsonParams = new
					{
						setID = query,
						orderBy = "Pieces",
						PageSize = "1"
					};

					var jsonParamsString = JsonConvert.SerializeObject(jsonParams);
					request.RequestUri = new Uri(request.RequestUri + "&params=" + jsonParamsString);
				
					using var response2 = client.SendAsync(request).Result;
					var jsonResponse2 = await response2.Content.ReadAsStringAsync();

					var data = JsonConvert.DeserializeObject<BrickSetGetSets>(jsonResponse2);

					if (data == null || data.matches == 0 || data.sets.Count == 0)
					{
						await RespondAsync("Failed to find any sets with the given ID.", ephemeral: true);
						return;
					}

					query = $"{data.sets[0].number}-{data.sets[0].numberVariant}";
				}
				
				if (!query.Contains("-")) query += "-1";
				var buttons = new ComponentBuilder().WithButton("Open Link", null, ButtonStyle.Link, url: "https://www.bricklink.com/v2/catalog/catalogitem.page?S=" + query);
				await RespondAsync("Click the button below to open the Bricklink website.", components: buttons.Build());
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
		
		[DefaultMemberPermissions(GuildPermission.ViewChannel)]
		[SlashCommand("review", "Displays the set score based on BrickInsights review.")]
		public async Task Review(LookupType lookupType, string query)
		{
			try
			{
				await DeferAsync();
				if (lookupType == LookupType.SetID)
				{
					//Get set number from ID
					var client = new HttpClient();
					client.Timeout = TimeSpan.FromSeconds(10);
					var request = new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = new Uri($"https://brickset.com/api/v3.asmx/getSets?apiKey={Config.Instance.Bot.BrikSetApiKey}&userHash="),
						Headers =
						{
							{ "Accept", "application/json" }
						}
					};

					object jsonParams = new
					{
						setID = query,
						orderBy = "Pieces",
						PageSize = "1"
					};

					var jsonParamsString = JsonConvert.SerializeObject(jsonParams);
					request.RequestUri = new Uri(request.RequestUri + "&params=" + jsonParamsString);
				
					using var response = client.SendAsync(request).Result;
					var jsonResponse = await response.Content.ReadAsStringAsync();

					var data = JsonConvert.DeserializeObject<BrickSetGetSets>(jsonResponse);

					if (data == null || data.matches == 0 || data.sets.Count == 0)
					{
						await FollowupAsync("Failed to find any sets with the given ID.", ephemeral: true);
						return;
					}

					query = $"{data.sets[0].number}-{data.sets[0].numberVariant}";
				}

				if (!query.Contains("-")) query += "-1";
				//Get review data from BrickInsight
				var client2 = new HttpClient();
				client2.Timeout = TimeSpan.FromSeconds(10);
				var request2 = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("https://brickinsights.com/api/sets/" + query),
					Headers =
					{
						{ "Accept", "application/json" }
					}
				};

				using var response2 = client2.SendAsync(request2).Result;
				var jsonResponse2 = await response2.Content.ReadAsStringAsync();

				var data2 = JsonConvert.DeserializeObject<BrickInsights>(jsonResponse2);

				if (data2 == null)
				{
					await FollowupAsync("Failed to find any sets with the given set number.", ephemeral: true);
					return;
				}
				
				EmbedBuilder builder = new EmbedBuilder()
					.WithLimitedTitle($"{data2.id} | {query} | {data2.name}")
					.WithUrl($"https://brickinsights.com/sets/{query}")
					.WithColorType(EmbedColor.Ok)
					.WithThumbnailUrl("https://brickinsights.com" + data2.image_urls.teaser)
					.WithLimitedField("Rated", $"{data2.name} is rated **{data2.average_rating}/100**", false)
					.WithLimitedField("Review Count", data2.review_count, false)
					.WithLimitedField("Links", $"More reviews at [BrickInsight](https://brickinsights.com/sets/{query})", false)
					.WithFooter("Source: BrightInsight");
				
				await FollowupAsync("", new[] { builder.Build() });
			}
			catch (Exception ex)
			{
				_log.ErrorException(ex, Context.Guild.Id, Context.User.Id, Context.Channel);
			}
		}
	}
}