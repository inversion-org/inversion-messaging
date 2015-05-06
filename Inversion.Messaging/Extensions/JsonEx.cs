namespace Inversion.Messaging.Extensions
{
	public static class JsonEx {
		public static string ToJSON(this object source) {
			return Newtonsoft.Json.JsonConvert.SerializeObject(source);
		}

		public static T FromJSON<T>(this string source) {
			if (string.IsNullOrEmpty(source)) {
				return default(T);
			}
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(source);
		}
	}
}