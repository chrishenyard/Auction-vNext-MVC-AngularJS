using Auction.Models;
using Auction.Serializers;
using Auction.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace Auction.Repositories {
	class AuctionAzureStorageRepository : IAuctionFileRepository {
		private CloudBlobContainer _container;

		public AuctionAzureStorageRepository() {
			var config = new Configuration();
			var account = CloudStorageAccount.Parse(config.Get("AzureConnectionString"));
			var client = account.CreateCloudBlobClient();
			_container = client.GetContainerReference(config.Get("AzureContainer"));
		}

		public async Task<AuctionItem> GetAuctionItemAsync(string fileName) {
			AuctionItem auctionItem = null;

			var blob = _container.GetBlockBlobReference(fileName.ReplaceWithUnixPathSeparator());
			var text = await blob.DownloadTextAsync();

			auctionItem = Json.Deserialize<AuctionItem>(text);
			return auctionItem;
		}

		public async Task SaveAuctionItemAsync(AuctionItem item, string fileName) {
			var text = Json.Serialize(item);
			var bytes = Encoding.UTF8.GetBytes(text);
			var stream = new MemoryStream(bytes, 0, bytes.Length);
			stream.Position = 0;

			var blob = _container.GetBlockBlobReference(fileName.ReplaceWithUnixPathSeparator());
			await blob.UploadFromStreamAsync(stream);
		}

		public async Task SaveAuctionBidAsync(AuctionBid bid, string fileName) {
			var text = Json.Serialize(bid);
			var bytes = Encoding.UTF8.GetBytes(text);
			var stream = new MemoryStream(bytes, 0, bytes.Length);
			stream.Position = 0;

			var blob = _container.GetBlockBlobReference(fileName.ReplaceWithUnixPathSeparator());
			await blob.UploadFromStreamAsync(stream);
		}

		public async Task<List<User>> GetUsersAsync(string fileName) {
			var users = new List<User>();

			var blob = _container.GetBlockBlobReference(fileName.ReplaceWithUnixPathSeparator());
			var text = await blob.DownloadTextAsync();

			users = Json.Deserialize<List<User>>(text);
			return users;
		}

		public async Task TruncateFileAsync(string fileName) {
			var bytes = Encoding.UTF8.GetBytes(string.Empty);
			var stream = new MemoryStream(bytes, 0, bytes.Length);
			stream.Position = 0;

			var blob = _container.GetBlockBlobReference(fileName.ReplaceWithUnixPathSeparator());
			await blob.UploadFromStreamAsync(stream);
		}

		public async Task<List<AuctionBid>> GetAuctionBidsAsync(string fileName) {
			var bids = new List<AuctionBid>();

			var blob = _container.GetBlockBlobReference(fileName.ReplaceWithUnixPathSeparator());
			var text = await blob.DownloadTextAsync();

			if (text.Length == 0) return bids;

			text = text.Replace("}{", "},{");
			text = "[" + text + "]";
			bids = Json.Deserialize<List<AuctionBid>>(text);
			return bids;
		}

		public async Task<List<string>> GetAuctionItemFileNamesAsync(string basePath) {
			var fileNames = new List<string>();

			await Task.Factory.StartNew(() => {
				var directoryContainer = _container.GetDirectoryReference(basePath.ReplaceWithUnixPathSeparator());

				foreach (var item in directoryContainer.ListBlobs(useFlatBlobListing: false)) {
					if (item is CloudBlockBlob) {
						var name = ((CloudBlockBlob)item).Name;
						var fileName = Path.GetFileName(name);
						fileNames.Add(fileName);
					}
				}
			});

			return fileNames;
		}
	}
}
