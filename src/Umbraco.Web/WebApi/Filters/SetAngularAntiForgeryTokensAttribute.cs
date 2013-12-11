﻿using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Helpers;
using System.Web.Http.Filters;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A filter to set the csrf cookie token based on angular conventions
    /// </summary>
    public sealed class SetAngularAntiForgeryTokensAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response == null) return;

            //don't need to set the cookie if they already exist
            if (context.Request.Headers.GetCookies(AngularAntiForgeryHelper.AngularCookieName).Any()
                && context.Request.Headers.GetCookies(AngularAntiForgeryHelper.CsrfValidationCookieName).Any())
            {
                return;
            }

            string cookieToken, headerToken;
            AngularAntiForgeryHelper.GetTokens(out cookieToken, out headerToken);

            //We need to set 2 cookies: one is the cookie value that angular will use to set a header value on each request,
            // the 2nd is the validation value generated by the anti-forgery helper that we use to validate the header token against.

            var angularCookie = new CookieHeaderValue(AngularAntiForgeryHelper.AngularCookieName, headerToken)
                {
                    Path = "/",
                    //must be js readable
                    HttpOnly = false,
                    Secure = GlobalSettings.UseSSL
                };

            var validationCookie = new CookieHeaderValue(AngularAntiForgeryHelper.CsrfValidationCookieName, cookieToken)
            {
                Path = "/",
                HttpOnly = true,
                Secure = GlobalSettings.UseSSL
            };

            context.Response.Headers.AddCookies(new[] { angularCookie, validationCookie });
        }
    }
}