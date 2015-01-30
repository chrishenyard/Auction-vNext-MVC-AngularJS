using System.Runtime.Serialization;

namespace Auction.Models
{
	[DataContract]
	public class User
    {
		[DataMember]
		public string Id { get; set; }
		[DataMember]
		public string FirstName { get; set; }
		[DataMember]
		public string LastName { get; set; }
	}
}