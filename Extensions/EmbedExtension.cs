using System;
using BrikBotCore.Enums;
using Discord;
using Discord.WebSocket;

namespace BrikBotCore.Extensions
{
	public static class EmbedExtension
	{
		/// <summary>
		///     Trim a title to 253 characters if it's longer than 256 characters.
		/// </summary>
		/// <param name="builder">The <see cref="EmbedBuilder" />.</param>
		/// <param name="title">The title to parse.</param>
		/// <returns>The trimmed title.</returns>
		public static EmbedBuilder WithLimitedTitle(this EmbedBuilder builder, string title)
		{
			return title.IsNull() ? builder : builder.WithTitle(title.Length > 256 ? $"{title.Substring(0, 253)}..." : title);
		}

		/// <summary>
		///     Trim a description to 4093 characters if it's longer than 4096 characters.
		/// </summary>
		/// <param name="builder">The <see cref="EmbedBuilder" />.</param>
		/// <param name="description">The description to parse.</param>
		/// <returns>The trimmed description.</returns>
		public static EmbedBuilder WithLimitedDescription(this EmbedBuilder builder, string description)
		{
			return description.IsNull() ? builder : builder.WithDescription(description.Length > 4096 ? $"{description.Substring(0, 4093)}..." : description);
		}

		/// <summary>
		///     Trim the value of a field if it's longer than 1024 characters.
		/// </summary>
		/// <param name="builder">The <see cref="EmbedBuilder" />.</param>
		/// <param name="title">The field title.</param>
		/// <param name="value">The value to be trimmed.</param>
		/// <param name="inline">Should the field be in-line?</param>
		/// <returns></returns>
		public static EmbedBuilder WithLimitedField(this EmbedBuilder builder, string title, object value, bool inline = true)
		{
			if (title.IsNull()) return builder;

			if (value == null) return builder;
			var val = value.ToString();
			if (val.IsNull()) return builder;

			var fieldBuilder = new EmbedFieldBuilder();
			fieldBuilder
				.WithIsInline(inline)
				.WithName(title)
				.WithValue(val.Length > 1024 ? $"{val.Substring(0, 1021)}..." : val);

			return builder.WithFields(fieldBuilder);
		}

		/// <summary>
		///     Get BrikBot internal color as <see cref="Color" /> from given type.
		/// </summary>
		/// <param name="builder">The <see cref="EmbedBuilder" />.</param>
		/// <param name="colorType">The color type to convert.</param>
		/// <returns>
		///     <see cref="EmbedBuilder" />
		/// </returns>
		public static EmbedBuilder WithColorType(this EmbedBuilder builder, EmbedColor colorType)
		{
			switch (colorType)
			{
				case EmbedColor.Good:
					return builder.WithColor(new Color(Convert.ToUInt32("00ff00", 16)));
				case EmbedColor.Warning:
					return builder.WithColor(new Color(Convert.ToUInt32("FF8C00", 16)));
				case EmbedColor.Error:
					return builder.WithColor(new Color(Convert.ToUInt32("ff0000", 16)));
				case EmbedColor.Ok:
					return builder.WithColor(new Color(Convert.ToUInt32("f9c80a", 16)));
				default:
					return builder.WithColor(new Color(Convert.ToUInt32("f9c80a", 16)));
			}
		}

		/// <summary>
		///     Generate a formatted footer from the provided data.
		/// </summary>
		/// <param name="builder">The <see cref="EmbedBuilder" />.</param>
		/// <param name="user">The user to include in the footer.</param>
		/// <param name="guild">The guild to include in the footer.</param>
		/// <param name="extraData">Any extra data include in the footer, if any.</param>
		/// <returns>
		///     <see cref="EmbedBuilder" />
		/// </returns>
		public static EmbedBuilder RequestedBy(this EmbedBuilder builder, SocketUser user, SocketGuild guild, string extraData = null)
		{
			return builder.WithFooter(RequestedByFormat(user, guild, extraData));
		}

		/// <summary>
		///     Generate a formatted footer from the provided data.
		/// </summary>
		/// <param name="user">The user to include in the footer.</param>
		/// <param name="guild">The guild to include in the footer.</param>
		/// <param name="extraData">Any extra data include in the footer, if any.</param>
		/// <returns>
		///     <see cref="EmbedFooterBuilder" />
		/// </returns>
		private static EmbedFooterBuilder RequestedByFormat(SocketUser user, SocketGuild guild, string extraData)
		{
			return new EmbedFooterBuilder
			{
				Text = $"Requested by {user?.Username}#{user?.DiscriminatorValue.ToString("D4") ?? "0000"}" + extraData,
				IconUrl = user.GetAvatar()
			};
		}
	}
}