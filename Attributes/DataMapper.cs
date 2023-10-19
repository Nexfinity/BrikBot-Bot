using System.Collections.Generic;
using System.Data;
using System.Linq;
using BrikBotCore.Attributes;

namespace BrikBotCore.Attributes
{
	public class DataMapper<TEntity> where TEntity : class, new()
	{
		public TEntity Map(DataRow row)
		{
			var entity = new TEntity();
			return Map(row, entity);
		}

		public TEntity Map(DataRow row, TEntity entity)
		{
			var columnNames = row.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
			var properties = typeof(TEntity).GetProperties()
				.Where(x => x.GetCustomAttributes(typeof(DataNamesAttribute), true).Any())
				.ToList();
			foreach (var prop in properties) PropertyMapHelper.Map(typeof(TEntity), row, prop, entity);

			return entity;
		}

		public IEnumerable<TEntity> Map(DataTable table)
		{
			var entities = new List<TEntity>();
			var columnNames = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
			var properties = typeof(TEntity).GetProperties()
				.Where(x => x.GetCustomAttributes(typeof(DataNamesAttribute), true).Any())
				.ToList();
			foreach (DataRow row in table.Rows)
			{
				var entity = new TEntity();
				foreach (var prop in properties) PropertyMapHelper.Map(typeof(TEntity), row, prop, entity);
				entities.Add(entity);
			}

			return entities;
		}
	}
}