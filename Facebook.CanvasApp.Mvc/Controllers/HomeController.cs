using System.Web.Mvc;
using Facebook.CanvasApp.Mvc.Models.ActionFilters;
using Facebook.CanvasApp.Mvc.Models.Facebook;

namespace Facebook.CanvasApp.Mvc.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		[FacebookAppAuthRequired(RedirectAction = "LogOn", RedirectController = "Account", ReturnUrl="/Home/AuthorizedAccessOnly")]
		public ActionResult AuthorizedAccessOnly()
		{
			ViewBag.UserName = CurrentUser.Get().Name;

			return View();
		}
	}
}
