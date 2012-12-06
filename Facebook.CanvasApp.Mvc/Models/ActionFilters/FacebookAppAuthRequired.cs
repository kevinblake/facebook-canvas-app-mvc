using System.Web.Mvc;
using System.Web.Routing;
using Facebook.CanvasApp.Mvc.Models.Facebook;

namespace Facebook.CanvasApp.Mvc.Models.ActionFilters
{
	public class FacebookAppAuthRequired : ActionFilterAttribute
	{
		public string RedirectController { get; set; }

		public string RedirectAction { get; set; }

		public string ReturnUrl { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			if (!CurrentUser.Get().IsAuthorised())
			{
				RedirectToRoute(filterContext, new { controller = RedirectController, action = RedirectAction, returnUrl = ReturnUrl });
			}
		}

		private void RedirectToRoute(ActionExecutingContext context, object routeValues)
		{
			var rc = new RequestContext(context.HttpContext, context.RouteData);
			var url = RouteTable.Routes.GetVirtualPath(rc, new RouteValueDictionary(routeValues)).VirtualPath;
			context.HttpContext.Response.Redirect(url, true);
		}
	}
}