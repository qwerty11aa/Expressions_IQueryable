﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Sample03.E3SClient
{

    public class E3SQueryClient
	{
		private string UserName;
		private string Password;
		private Uri BaseAddress = new Uri("https://telescope.epam.com/eco/rest/e3s-eco-scripting-impl/0.1.0");


		public E3SQueryClient(string user, string password)
		{
			UserName = user;
			Password = password;
		}

		public IEnumerable<T> SearchFTS<T>(string query, int start = 0, int limit = 0) where T : E3SEntity
		{
			HttpClient client = CreateClient();
			var requestGenerator = new FTSRequestGenerator(BaseAddress);

		    var querystrings = query.Split(Convert.ToChar(";"));

            Uri request = requestGenerator.GenerateRequestUrl<T>(querystrings, start, limit);

			var resultString = client.GetStringAsync(request).Result;
            var result = JsonConvert.DeserializeObject<FTSResponse<T>>(resultString).items.Select(t => t.data);

            return result;
		}


		public IEnumerable SearchFTS(Type type, string query, int start = 0, int limit = 0)
		{
			HttpClient client = CreateClient();
			var requestGenerator = new FTSRequestGenerator(BaseAddress);

		    var querystrings = query.Split(Convert.ToChar(";"));

			Uri request = requestGenerator.GenerateRequestUrl(type, querystrings, start, limit);

			var resultString = client.GetStringAsync(request).Result;
			var endType = typeof(FTSResponse<>).MakeGenericType(type);

		    var result = JsonConvert.DeserializeObject(resultString, endType);


			var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;

			foreach (object item in (IEnumerable)endType.GetProperty("items").GetValue(result))
			{
				list.Add(item.GetType().GetProperty("data").GetValue(item));
			}

			return list;
		}


		private HttpClient CreateClient()
		{

            var client = new HttpClient(new HttpClientHandler
			{
                AllowAutoRedirect = true,
				PreAuthenticate = true
			});

            var encoding = new ASCIIEncoding();
			var authHeader = new AuthenticationHeaderValue("Basic",
				Convert.ToBase64String(encoding.GetBytes(string.Format("{0}:{1}", UserName, Password))));
			client.DefaultRequestHeaders.Authorization = authHeader;

            return client;
		}
	}
}
