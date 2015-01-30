using System;
using System.Runtime.Serialization;

namespace Auction.Models
{
	[DataContract]
	public class AuctionItemModel
    {
		[DataMember]
		public string Id { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public string ImageUrl { get; set; }
		[DataMember]
		public int HighBid { get; set; }
		[DataMember]
		public int Increment { get; set; }
		[DataMember]
		public bool Processing { get; set; }
		[DataMember]
		public bool TimedOut { get; set; }
		[DataMember]
		public bool Outbid { get; set; }
		[DataMember]
		public DateTimeOffset Closed { get; set; }
	}
}