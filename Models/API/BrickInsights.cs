using System.Collections.Generic;

namespace BrikBotCore.Models.API
{
	public class ImageUrls
	{
		public string small { get; set; }
		public string medium { get; set; }
		public string large { get; set; }
		public string teaser { get; set; }
		public string teaser_compact { get; set; }
		public string generic140xn { get; set; }
		public string generic200xn { get; set; }
		public string genericnx400 { get; set; }
		public string original { get; set; }
	}

	public class Review
	{
		public string review_url { get; set; }
		public string snippet { get; set; }
		public string review_amount { get; set; }
		public string rating_original { get; set; }
		public string rating_converted { get; set; }
		public string author_name { get; set; }
		public object embed { get; set; }
	}

	public class BrickInsights
	{
		public int id { get; set; }
		public string name { get; set; }
		public string year { get; set; }
		public string image { get; set; }
		public string average_rating { get; set; }
		public string review_count { get; set; }
		public string url { get; set; }
		public ImageUrls image_urls { get; set; }
		public List<Review> reviews { get; set; }
	}
}