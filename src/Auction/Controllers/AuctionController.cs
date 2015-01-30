using Auction.Models;
using Auction.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace Auction.Controllers {
    public class AuctionController : Controller
    {
		private readonly IAuctionDAL _auctionDAL;

		public AuctionController(IAuctionDAL auctionDAL) {
			_auctionDAL = auctionDAL;
		}

		[HttpGet]
		public IActionResult Index() {
			return View();
		}

		[HttpGet]
        public async Task<IEnumerable<AuctionItemModel>> GetItems()
        {
			await _auctionDAL.ResetAuctionItemsAsync();
			var items = await _auctionDAL.GetAuctionItemsAsync();
			items = items.Select(item => { item.HighBid += item.Increment; return item; }).ToList();

			var modelList = items.Select(item => {
				var model = new AuctionItemModel {
					Id = item.Id,
					Description = item.Description,
					ImageUrl = item.ImageUrl,
					HighBid = item.HighBid,
					Increment = item.Increment,
					Closed = item.Closed
				};
				return model;
			}).ToList();


			return modelList;
        }

		[HttpPost]
		public async Task<IActionResult> SaveBid([FromBody] BidModel model) {
			await _auctionDAL.SaveAuctionBidAsync(model);
			return new ObjectResult(model);
		}

		[HttpGet]
		public async Task<IEnumerable<User>> GetUsers() {
			var users = await _auctionDAL.GetUsersAsync();
			return users;
		}

		[HttpGet]
		public async Task<IActionResult> ResetAuctionItems() {
			await _auctionDAL.ResetAuctionItemsAsync();
			return new EmptyResult();
		}

		[HttpGet]
		public async Task<List<BidModel>> GetAuctionBids(string id) {
			var item = await _auctionDAL.GetAuctionItemAsync(id);

			var modelList = item.AuctionBids.Select(bid => {
				var model = new BidModel {
					UserId = bid.UserId,
					AuctionItemId = bid.AuctionItemId,
					Bid = bid.Bid
				};
				return model;
			}).ToList();

			return modelList;
		}
    }
}
