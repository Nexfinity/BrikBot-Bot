using System.Collections.Generic;

namespace BrikBotCore.Models.API
{
	public class ExternalIds
	{
		public List<string> BrickLink { get; set; }
		public List<string> BrickOwl { get; set; }
		public List<string> Brickset { get; set; }
		public List<string> LDraw { get; set; }
		public List<string> LEGO { get; set; }
	}

	public class RebrickablePart
	{
		public string part_num { get; set; }
		public string name { get; set; }
		public int part_cat_id { get; set; }
		public int year_from { get; set; }
		public int year_to { get; set; }
		public string part_url { get; set; }
		public string part_img_url { get; set; }
		public List<object> prints { get; set; }
		public List<string> molds { get; set; }
		public List<object> alternates { get; set; }
		public ExternalIds external_ids { get; set; }
		public object print_of { get; set; }
	}
}