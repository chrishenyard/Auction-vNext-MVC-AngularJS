﻿using Microsoft.AspNet.Mvc;

namespace Auction.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}