using System;
using System.Collections.Generic;
using System.Linq;

namespace BrikBotCore.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DataNamesAttribute : Attribute
	{
		public DataNamesAttribute()
		{
			_valueNames = new List<string>();
		}

		public DataNamesAttribute(params string[] valueNames)
		{
			_valueNames = valueNames.ToList();
		}

		private List<string> _valueNames { get; set; }

		public List<string> ValueNames
		{
			get => _valueNames;
			set => _valueNames = value;
		}
	}
}