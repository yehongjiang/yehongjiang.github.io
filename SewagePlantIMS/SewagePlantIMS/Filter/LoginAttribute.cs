using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SewagePlantIMS.Filter
{
    public class LoginAttribute : ActionFilterAttribute
    {
        // GET: LoginAttribute
        private bool _isNeed = true;
        public bool isNeed
        {
            get { return _isNeed; }
            set { _isNeed = value; }
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (!isNeed)
            {
                return;
            }
            var username = filterContext.HttpContext.Session["username"];
            if (username == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Index", area = string.Empty }));
            }
            else
            {
                new RedirectToRouteResult(new RouteValueDictionary(new { controller = "ElectricManage", action = "Index", area = string.Empty }));
            }
        }
    }
}