﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Using RingCentralSdk 0.1.21
using RingCentral;
using RingCentral.SDK;
using RingCentral.SDK.Helper;
using RingCentral.SDK.Http;

using DotEnv;
using HandlebarsDotNet;

namespace csharp
{
	class MainClass
	{
		public static SDK NewSDK() {
			var appKey = Environment.GetEnvironmentVariable("RC_APP_KEY");
			var appSecret = Environment.GetEnvironmentVariable("RC_APP_SECRET");
			var serverUrl = Environment.GetEnvironmentVariable("RC_APP_SERVER_URL");
			var username = Environment.GetEnvironmentVariable("RC_USER_USERNAME");
			var extension = Environment.GetEnvironmentVariable("RC_USER_EXTENSION");
			var password = Environment.GetEnvironmentVariable("RC_USER_PASSWORD");
			var appName = "Custom Fax Cover Page Text";
			var appVersion = "0.0.1";
			var sdk = new SDK(appKey, appSecret, serverUrl, appName, appVersion);
			Response response = sdk.GetPlatform().Authorize(username, extension, password, true);
			return sdk;
		}

		public static Attachment GetCoverPage() {
			var templatePath = Environment.GetEnvironmentVariable ("RC_DEMO_FAX_COVERPAGE_TEMPLATE");
			var source = File.ReadAllText (templatePath);
			var template = Handlebars.Compile (source);

			Dictionary<string, string> data = new Dictionary<string, string>()
			{
				{"fax_date", ""},
				{"fax_pages", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_PAGES")},
				{"fax_to_name", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_TO_NAME")},
				{"fax_to_fax", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_TO")},
				{"fax_from_name", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_FROM_NAME")},
				{"fax_from_phone", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_FROM")},
				{"fax_from_fax", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_FROM")},
				{"fax_coverpage_text", Environment.GetEnvironmentVariable ("RC_DEMO_FAX_COVERPAGE_TEXT")}
			};

			string html = template(data);
			byte[] coverPageBytes = Encoding.ASCII.GetBytes(html);
			var coverPage = new Attachment ("cover.htm", "text/html", coverPageBytes);

			return coverPage;
		}

		public static void SendFax(RingCentral.SDK.SDK sdk) {
			// Get Cover Page
			var coverPage = GetCoverPage ();

			// Get Fax Body
			var attachmentBytes = File.ReadAllBytes (Environment.GetEnvironmentVariable ("RC_DEMO_FAX_FILEPATH"));
			var attachment = new Attachment ("test_file.pdf", "application/pdf", attachmentBytes);

			var attachments = new List<Attachment> {coverPage, attachment};

			var to = Environment.GetEnvironmentVariable ("RC_DEMO_FAX_TO");
			var json = "{\"to\":[{\"phoneNumber\":\"" + to + "\"}],\"faxResolution\":\"High\"}";
			json = "{\"to\":[{\"phoneNumber\":\"" + to + "\"}]}";
			Console.WriteLine (json);

			var request = new Request ("/restapi/v1.0/account/~/extension/~/fax", json, attachments);

			var html = request.GetHttpContent ();
			var response = sdk.GetPlatform().Post(request);
		}

		public static void Main (string[] args)
		{
			DotEnv.DotEnvConfig.Install (DotEnv.EnvFileLoadSettings.ThrowOnInvalidFile);
			var sdk = NewSDK ();
			SendFax (sdk);
			Console.WriteLine ("DONE!");
		}
	}
}