using System;
using System.Collections.Generic;
using System.Linq;
using BrikBotCore.Attributes;

namespace BrikBotCore.Attributes
{
	public static class AttributeHelper
	{
		public static List<string> GetDataNames(Type type, string propertyName)
		{
			var property = type.GetProperty(propertyName).GetCustomAttributes(false).FirstOrDefault(x => x.GetType().Name == "DataNamesAttribute");
			if (property != null) return ((DataNamesAttribute) property).ValueNames;
			return new List<string>();
		}
	}
}