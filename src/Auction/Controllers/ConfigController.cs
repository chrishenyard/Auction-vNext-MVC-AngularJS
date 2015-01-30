using Microsoft.AspNet.Mvc;
using Auction.Models;

namespace Auction.Controllers {
	public class ConfigController : Controller {
		// TODO: Return environment configuration
		[HttpGet]
		public ConfigModel Get() {
			return new ConfigModel();
		}
	}
}
