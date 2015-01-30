using System;
using System.Runtime.Serialization;

namespace Auction.Models {
	[DataContract]
	public class AuctionBid {
		[DataMember]
		public string UserId { get; set; }
		[DataMember]
		public string AuctionItemId { get; set; }
		[DataMember]
		public int Bid { get; set; }
		[DataMember]
		public DateTimeOffset Timestamp { get; set; }
	}
}