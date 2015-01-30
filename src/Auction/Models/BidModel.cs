using System.Runtime.Serialization;

namespace Auction.Models
{
	[DataContract]
	public class BidModel {
		[DataMember]
		public string UserId { get; set; }
		[DataMember]
		public string AuctionItemId { get; set; }
		[DataMember]
		public int Bid { get; set; }
		[DataMember]
		public bool HighBid { get; set; }
	}
}