using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auction.Models;
using Auction.Synchronization;

namespace Auction.Repositories
{
	public interface IAuctionDAL {
		Task<List<AuctionItem>> GetAuctionItemsAsync();
		Task SaveAuctionBidAsync(BidModel model);
		Task ResetAuctionItemsAsync();
		Task<List<User>> GetUsersAsync();
		Task<AuctionItem> GetAuctionItemAsync(string itemId);
	}

	public class AuctionDAL : IAuctionDAL {
		private readonly IAuctionFileRepository _auctionFileRepository;
		private static readonly AsyncLock _asyncLock = new AsyncLock();
		private readonly string _basePath;
		private readonly string _itemsBasePath;
		private readonly string _bidsBasePath;
		private readonly string _usersBasePath;

		private const string ItemsFileSuffix = ".json";
		private const string BidsFileSuffix = "_bid.json";

        public AuctionDAL(IAuctionFileRepository auctionFileRepository, string basePath) {
			_auctionFileRepository = auctionFileRepository;
			_basePath = Path.Combine(basePath, "app_data");
			_itemsBasePath = Path.Combine(_basePath, "items");
			_bidsBasePath = Path.Combine(_basePath, "bids");
			_usersBasePath = Path.Combine(_basePath, "users");
		}

		public async Task<List<AuctionItem>> GetAuctionItemsAsync() {
			var fileNames = await _auctionFileRepository.GetAuctionItemFileNamesAsync(_itemsBasePath);
			var auctionItems = new List<AuctionItem>();
			
			foreach (var fileName in fileNames) {
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
				var fullFileName = Path.Combine(_itemsBasePath, fileName);
				using (var releaser = await _asyncLock.LockAsync(fileNameWithoutExtension)) {
					var item = await _auctionFileRepository.GetAuctionItemAsync(fullFileName);
					auctionItems.Add(item);
				}
			}

			return auctionItems;
		}

		public async Task SaveAuctionBidAsync(BidModel model) {
			AuctionBid bid = new AuctionBid {
				UserId = model.UserId,
				AuctionItemId = model.AuctionItemId,
				Bid = model.Bid,
				Timestamp = DateTimeOffset.Now,
			};

			var itemFileName = Path.Combine(_itemsBasePath, bid.AuctionItemId + ItemsFileSuffix);
			var bidFileName = Path.Combine(_bidsBasePath, bid.AuctionItemId + BidsFileSuffix);

			using (var releaser = await _asyncLock.LockAsync(bid.AuctionItemId)) {
				var item = await _auctionFileRepository.GetAuctionItemAsync(itemFileName);

				if (model.Bid > item.HighBid) {
					item.HighBid = model.Bid;
					model.HighBid = true;
					model.Bid += item.Increment;
					await _auctionFileRepository.SaveAuctionItemAsync(item, itemFileName);
				} else {
					model.Bid = item.HighBid + item.Increment;
				}

				await _auctionFileRepository.SaveAuctionBidAsync(bid, bidFileName);
			}
		}

		public async Task ResetAuctionItemsAsync() {
			var fileNames = await _auctionFileRepository.GetAuctionItemFileNamesAsync(_itemsBasePath);
			var now = DateTimeOffset.Now;
			var i = 1;

			foreach (var fileName in fileNames) {
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
				using (var releaser = await _asyncLock.LockAsync(fileNameWithoutExtension)) {
					var itemFileName = Path.Combine(_itemsBasePath, fileName);
					var item = await _auctionFileRepository.GetAuctionItemAsync(itemFileName);

					item.HighBid = 0;
					item.Closed = now.AddMinutes(i++ * 15).AddSeconds(i * 3);
					await _auctionFileRepository.SaveAuctionItemAsync(item, itemFileName);

					var bidFileName = Path.Combine(_bidsBasePath, item.Id + BidsFileSuffix);
					await _auctionFileRepository.TruncateFileAsync(bidFileName);
                }
			}
		}

		public async Task<List<User>> GetUsersAsync() {
			var fileName = Path.Combine(_usersBasePath, "users.json");
			var users = await _auctionFileRepository.GetUsersAsync(fileName);
			return users;
		}

		public async Task<AuctionItem> GetAuctionItemAsync(string itemId) {
			var item = new AuctionItem();
			var itemFileName = Path.Combine(_itemsBasePath, itemId + ItemsFileSuffix);
			var bidFileName = Path.Combine(_bidsBasePath, itemId + BidsFileSuffix);

			using (var releaser = await _asyncLock.LockAsync(itemId)) {
				item = await _auctionFileRepository.GetAuctionItemAsync(itemFileName);
				item.AuctionBids = await _auctionFileRepository.GetAuctionBidsAsync(bidFileName);
			}

			item.AuctionBids = item.AuctionBids.OrderByDescending(b => b.Bid).ToList();

			return item;
		}
	}
}