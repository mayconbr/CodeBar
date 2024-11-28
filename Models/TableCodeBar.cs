using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Estoque.Models
{
	[DataContract]
	public class TableCodeBar
	{
		[Key]
		public long Id { get; set; }
		[DataMember]
		public string CodeBar { get; set; }
		[DataMember]
		public DateTime Date { get; set; }
    }
}
