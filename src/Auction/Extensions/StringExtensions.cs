namespace Auction.Extensions {
	static class StringExtensions {
		public static string ReplaceWithUnixPathSeparator(this string path) {
			var newPath = path.Replace(@"\", "/");
			return newPath;
		}
	}
}
