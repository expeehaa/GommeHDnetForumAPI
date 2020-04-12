using System.IO;
using Newtonsoft.Json;

namespace GommeHDnetForumAPITest {
	public class CredentialsLoader {
		public string Username { get; set; } = "";
		public string Password { get; set; } = "";

		public static CredentialsLoader GetCredentials() {
			var ass       = new FileInfo(typeof(CredentialsLoader).Assembly.Location);
			var credsFile = new FileInfo(Path.Combine(ass.DirectoryName, "creds.json"));
			
			if(!credsFile.Exists) {
				using var writer = credsFile.CreateText();
				writer.WriteLine(JsonConvert.SerializeObject(new CredentialsLoader(), Formatting.Indented));
				writer.Close();
			}

			using var sr = new StreamReader(credsFile.OpenRead());
			var text = sr.ReadToEnd();
			sr.Close();
			return JsonConvert.DeserializeObject<CredentialsLoader>(text);
		}
	}
}