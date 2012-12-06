using System.Collections.Generic;
using System.Web;

namespace Facebook.CanvasApp.Mvc.Models.Facebook
{
	public class CurrentUser
	{
		private IDictionary<string, object> _facebookUserData;
		private const string GraphFields = "id,name";

		private CurrentUser()
		{
			
		}

		public bool IsAuthorised()
		{
			return this.AccessToken != null;
		}

		public string AccessToken
		{
			get
			{
				if (HttpContext.Current.Session["fb_access_token"] == null)
				{
					return null;
				}
				return HttpContext.Current.Session["fb_access_token"].ToString();
			}
			set { HttpContext.Current.Session["fb_access_token"] = value; }
		}

		private IDictionary<string, object> FacebookUserData
		{
			get
			{
				if (_facebookUserData == null)
				{
					var fb = new FacebookClient(AccessToken);
					_facebookUserData = (IDictionary<string, object>) fb.Get("/me?fields=" + GraphFields);
				}

				return _facebookUserData;
			}

		}

		public string Name
		{
			get
			{
				return (string)FacebookUserData["name"];
			}
		}	

		public long Id
		{
			get
			{
				return long.Parse((string)FacebookUserData["id"]);
			}
		}

		public static CurrentUser Get()
		{
			// TODO: Add this to the current Http Context, so we don't create a new one every time.
			// It cannot be a static class, to prevent users from accessing each others currentuser data.
			return new CurrentUser();
		}
	}
}