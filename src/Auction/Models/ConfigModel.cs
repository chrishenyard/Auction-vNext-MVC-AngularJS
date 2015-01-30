using System;
using Microsoft.AspNet.Mvc.Rendering;

namespace Auction.Models
{
    public class ConfigModel
    {
		[Flags]
		public enum LogLevel {
			Debug = 1,
			Info = 2,
			Warn = 4,
			Error = 8
		};

		public int MessageLevel {
			get {
#if (DEBUG)
				return (int)(LogLevel.Debug & LogLevel.Info & LogLevel.Warn & LogLevel.Error);
#else
				return 0;
#endif
			}

			set {}
		}
    }

	public static class HtmlHelpers {
		public static bool Debug(this IHtmlHelper helper) {
#if DEBUG
			return true;
#else
          return false;
#endif
		}
	}
}