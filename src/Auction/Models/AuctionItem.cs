using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Auction.Models {
	[DataContract]
	public class AuctionItem {
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
		public DateTimeOffset Closed { get; set; }
		public List<AuctionBid> AuctionBids { get; set; } = new List<AuctionBid>();
	}
}