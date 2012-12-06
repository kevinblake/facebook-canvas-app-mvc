using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Facebook.CanvasApp.Mvc.Models;
using Facebook.CanvasApp.Mvc.Models.Facebook;

namespace Facebook.CanvasApp.Mvc.Controllers
{
	public class AccountController : Controller
	{
		private const string AppId = FacebookConstants.AppId;
		private const string Appsecret = FacebookConstants.AppSecret;
		private const string Scope = "user_about_me";
		private readonly string _redirectUri;

		private readonly FacebookClient _fb;

		public AccountController()
			: this(new FacebookClient())
		{
		}

		public AccountController(FacebookClient fb)
		{
			_fb = fb;

			var context = System.Web.HttpContext.Current;
			var url = context.Request.Url.Scheme + System.Uri.SchemeDelimiter + context.Request.Url.Host + (context.Request.Url.Port != 80 ? ":" + context.Request.Url.Port : "");

			_redirectUri = url + "/Account/FacebookCallback";
		}

		public ActionResult LogOn(string returnUrl)
		{
			var csrfToken = Guid.NewGuid().ToString();
			Session["fb_csrf_token"] = csrfToken;

			var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(_fb.SerializeJson(new { returnUrl = HttpContext.Server.UrlDecode(returnUrl), csrf = csrfToken })));

			var fbLoginUrl = _fb.GetLoginUrl(
				new
					{
						client_id = AppId,
						client_secret = Appsecret,
						redirect_uri = _redirectUri,
						response_type = "code",
						scope = Scope,
						state = state
					});

			return Redirect(fbLoginUrl.AbsoluteUri);
		}

		public ActionResult FacebookCallback(string code, string state)
		{
			if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
				return RedirectToAction("Index", "Home");

			// first validate the csrf token
			dynamic decodedState;
			try
			{
				decodedState = _fb.DeserializeJson(Encoding.UTF8.GetString(Convert.FromBase64String(state)), null);
				var exepectedCsrfToken = Session["fb_csrf_token"] as string;
				// make the fb_csrf_token invalid
				Session["fb_csrf_token"] = null;

				if (!(decodedState is IDictionary<string, object>) || !decodedState.ContainsKey("csrf") || string.IsNullOrWhiteSpace(exepectedCsrfToken) || exepectedCsrfToken != decodedState.csrf)
				{
					return RedirectToAction("Index", "Home");
				}
			}
			catch
			{
				// log exception
				return RedirectToAction("Index", "Home");
			}

			try
			{
				dynamic result = _fb.Post("oauth/access_token",
										  new
											  {
												  client_id = AppId,
												  client_secret = Appsecret,
												  redirect_uri = _redirectUri,
												  code = code
											  });

				CurrentUser.Get().AccessToken = result.access_token;

				if (result.ContainsKey("expires"))
					Session["fb_expires_in"] = DateTime.Now.AddSeconds(result.expires);

				if (decodedState.ContainsKey("returnUrl"))
				{
					if (Url.IsLocalUrl(decodedState.returnUrl))
						return Redirect(decodedState.returnUrl);
				}

				return RedirectToAction("Index", "Home");
			}
			catch
			{
				// log exception
				return RedirectToAction("Index", "Home");
			}
		}
	}
}