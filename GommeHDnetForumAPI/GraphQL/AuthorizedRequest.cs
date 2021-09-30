using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using System.Net.Http;

namespace GommeHDnetForumAPI.GraphQL {
	public class AuthorizedRequest : GraphQLHttpRequest {
		public string Authorization { get; set; }

		public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer) {
			var r = base.ToHttpRequestMessage(options, serializer);
			r.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Authorization);
			return r;
		}
	}
}
