using System.Collections.Generic;

namespace BrikBotCore.Models.API
{
	public class Instruction
	{
		public string URL { get; set; }
		public string description { get; set; }
	}

	public class BrickSetGetInstructions
	{
		public string status { get; set; }
		public int matches { get; set; }
		public List<Instruction> instructions { get; set; }
	}
}