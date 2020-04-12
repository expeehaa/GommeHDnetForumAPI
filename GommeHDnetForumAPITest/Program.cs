using System;
using System.Collections.Generic;
using System.Linq;
using GommeHDnetForumAPI;
using GommeHDnetForumAPI.Models;
using GommeHDnetForumAPI.Models.Collections;
using GommeHDnetForumAPI.Models.Entities;

namespace GommeHDnetForumAPITest {
	public class Program {
		public static void Main(string[] args) {
			var creds = CredentialsLoader.GetCredentials();
			var forum = new Forum(creds.Username, creds.Password);

			Console.WriteLine(forum.GetOnlineUserCount().GetAwaiter().GetResult());
			TestFunctionalities(forum);
			//TestConversations(forum);
			//AllUserAccountsBeforeId(forum, 102);

			Console.ReadKey();
		}


		public static void TestFunctionalities(Forum forum) {
			//TestUserInfoParsing(forum);
			//TestConversations(forum);
			//TestForumThreads(forum);
			//TestMembersLists(forum);
			TestConversationReply(forum);
		}

		private static void TestConversationReply(Forum forum) {
			var conversations = forum.GetConversations(1, 1).GetAwaiter().GetResult();
			var con           = conversations.First(c => c.Title.Equals("Betaverify"));
			var success       = con.Reply("Testnachricht").GetAwaiter().GetResult();

			Console.ReadLine();
		}

		private static void TestForumThreads(Forum forum) {
			const string forumname = "off-topic";
			var someForum = forum.MasterForum.SubForums.FirstOrDefault(sf => string.Equals(sf.Title, forumname, StringComparison.OrdinalIgnoreCase));
			
			if(someForum != null) {
				someForum.DownloadDataAsync().GetAwaiter().GetResult();
				Console.WriteLine(someForum.Threads.Take(5).ThreadsToString());
				var t1 = someForum.Threads.First();
				t1.DownloadMessagesAsync().GetAwaiter().GetResult();
				Console.WriteLine(string.Join(", ", t1.Messages.Select(m => $"({m})")));
			} else {
				Console.WriteLine($"No forum with name '{forumname}' found\n");
			}

			var forums = forum.MasterForum.SubForums.ToList();
			//foreach (var sf in forums)
			//{
			//    Console.WriteLine($"Downloading Forumdata {sf.Title} ...");
			//    sf.DownloadDataAsync(1, 1).GetAwaiter().GetResult();
			//}

			var allSubForums = forums.Concat(forums.SelectMany(sf => sf.AllRealSubForums)).ToList();
			Console.WriteLine($"\n\nAll Forums ({allSubForums.Count}):");
			Console.WriteLine(string.Join("\n", allSubForums.Select(sf => sf.ToString())));

			Console.ReadLine();
		}

		private static void TestConversations(Forum forum) {
			var cons = forum.GetConversations().GetAwaiter().GetResult();
			var con1 = cons.First();
			
			con1.DownloadMessagesAsync().GetAwaiter().GetResult();
			Console.WriteLine(con1);

			var username = Console.ReadLine();
			var user     = forum.GetUserInfo(username).GetAwaiter().GetResult();

			if(user == null) {
				Console.WriteLine("User not found!");
			} else {
				//var con2 = cons.FirstOrDefault(c => c.Title.Equals("test", StringComparison.OrdinalIgnoreCase) && c.Members.Any(m => m.Id == user.Id));
				//if (con2 == null)
				//    con2 = forum.CreateConversation(new[] { user }.ToUserCollection(), "test", "A simple test message.").GetAwaiter().GetResult();
				//else
				//    con2.Reply("Testantwort").GetAwaiter().GetResult();

				Console.ReadLine();
			}
		}

		public static void TestUserInfoParsing(Forum forum) {
			var user  = forum.SelfUser;
			var user2 = forum.GetUserInfo("Klaus").GetAwaiter().GetResult();

			Console.ReadLine();
		}

		public static void TestMembersLists(Forum forum) {
			var lists = new List<UserCollection>();
			
			foreach(var typeName in Enum.GetNames(typeof(MembersListType))) {
				var list = forum.GetMembersList(Enum.Parse<MembersListType>(typeName)).GetAwaiter().GetResult();
				Console.WriteLine($"{typeName} list ({list.Count}):\n{list.ToString("\n")}\n");
				lists.Add(list);
			}

			Console.ReadLine();
		}

		/// <summary>
		/// Downloads and prints all accessible and verified forum users before the given id.
		/// </summary>
		/// <param name="forum">The Forum instance to download the users with.</param>
		/// <param name="maxId">Exclusive maximum user ID</param>
		public static void AllUserAccountsBeforeId(Forum forum, int maxId) {
			var userinfos = new List<UserInfo>();
			
			for(var i = 1; i < maxId; i++) {
				var text = "added";
				try {
					var ui = forum.GetUserInfo(i).GetAwaiter().GetResult();
					if(ui == null)
						text = "null";
					else if(!ui.Verified.HasValue || !ui.Verified.Value)
						text = "not verified";
					if(ui?.Verified != null && ui.Verified.Value)
						userinfos.Add(ui);
				} catch(Exception e) {
					text = $"exception: {e.GetType().Name}";
				}
				Console.WriteLine($"{i}: {text}");
			}
			Console.WriteLine("\n\n");

			Console.WriteLine($"Abrufbare und verifizierte Nutzer ({userinfos.Count}):");
			Console.WriteLine(string.Join("\n", userinfos.Select(u => $"Id {u.Id,3} | Name {u.Username,-16} | Link {u.UrlPath}").ToArray()));
			Console.WriteLine($"\n\n{string.Join("\n", userinfos.Select(u => $"Id {u.Id,3} | Name @{u.Username,-16}{(u.TimeRegistered.HasValue ? $" | Registriert seit {u.TimeRegistered.Value}" : "")}").ToArray())}");
			Console.WriteLine($"\n\n{string.Join("\n", userinfos.Select(u => $"Id {u.Id,3} | Name @{u.Username,-16}").ToArray())}");
		}
	}
}