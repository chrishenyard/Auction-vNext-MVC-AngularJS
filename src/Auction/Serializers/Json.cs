using Newtonsoft.Json;

namespace Auction.Serializers
{
	public class Json {
		public static string Serialize<T>(T type) {
			var jsonString = JsonConvert.SerializeObject(type, typeof(T), null);
			return jsonString;
		}
		public static T Deserialize<T>(string value) {
			T result = (T)JsonConvert.DeserializeObject(value, typeof(T));
			return result;
		}
	}
}