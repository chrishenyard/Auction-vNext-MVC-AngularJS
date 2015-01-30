using Auction.Models;
using Auction.Serializers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;

namespace Auction.Repositories {
	public interface IAuctionFileRepository {
		Task<AuctionItem> GetAuctionItemAsync(string fileName);
		Task SaveAuctionItemAsync(AuctionItem item, string fileName);
		Task SaveAuctionBidAsync(AuctionBid bid, string fileName);
		Task<List<User>> GetUsersAsync(string fileName);
		Task TruncateFileAsync(string fileName);
		Task<List<AuctionBid>> GetAuctionBidsAsync(string fileName);
	}

	public class AuctionFileRepository : IAuctionFileRepository {

		public async Task<AuctionItem> GetAuctionItemAsync(string fileName) {
			AuctionItem auctionItem = null;

			using (var reader = File.OpenText(fileName)) {
				var text = await reader.ReadToEndAsync();
				auctionItem = Json.Deserialize<AuctionItem>(text);
			}
			return auctionItem;
		}

		public async Task SaveAuctionItemAsync(AuctionItem item, string fileName) {
			var text = Json.Serialize(item);

			using (var writer = File.CreateText(fileName)) {
				await writer.WriteAsync(text);
			}
		}

		public async Task SaveAuctionBidAsync(AuctionBid bid, string fileName) {
			var text = Json.Serialize(bid);
			byte[] encodedText = Encoding.UTF8.GetBytes(text);

			using (var stream = new FileStream(fileName, FileMode.Append, 
				FileAccess.Write, FileShare.None, bufferSize: 1048576, useAsync: true)) {
				await stream.WriteAsync(encodedText, 0, encodedText.Length);
			}
		}

		public async Task<List<User>> GetUsersAsync(string fileName) {
			var users = new List<User>();

			using (var reader = File.OpenText(fileName)) {
				var text = await reader.ReadToEndAsync();
				users = Json.Deserialize<List<User>>(text);
			}

			return users;
		}

		public async Task TruncateFileAsync(string fileName) {
			byte[] encodedText = Encoding.UTF8.GetBytes(string.Empty);

			using (var stream = new FileStream(fileName, FileMode.Truncate,
				FileAccess.Write, FileShare.None, bufferSize: 1048576, useAsync: true)) {
				await stream.WriteAsync(encodedText, 0, encodedText.Length);
			}
		}

		public async Task<List<AuctionBid>> GetAuctionBidsAsync(string fileName) {
			var bids = new List<AuctionBid>();

			using (var reader = File.OpenText(fileName)) {
				var text = await reader.ReadToEndAsync();
				if (text.Length == 0) return bids;
				text = text.Replace("}{", "},{");
				text = "[" + text + "]"; 
				bids = Json.Deserialize<List<AuctionBid>>(text);
			}

			return bids;
		}
	}
}
