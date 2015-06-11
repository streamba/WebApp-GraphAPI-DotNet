#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.Owin.Security.OpenIdConnect;
using WebAppGraphAPI.Utils;

#endregion

namespace WebAppGraphAPI.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {


        /// <summary>
        ///     Creates a view to for adding a new <see cref="Role" /> to Graph.
        /// </summary>
        /// <returns>A view with the details to add a new <see cref="Role" /> objects</returns>
        public async Task<ActionResult> Create()
        {
            return View();
        }

        /// <summary>
        ///     Creates a view to for adding a new <see cref="Role" /> to Graph.
        /// </summary>
        /// <returns>A view with the details to add a new <see cref="Role" /> objects</returns>
        [HttpPost]
        public async Task<ActionResult> Create(AppRole role)
        {
            ActiveDirectoryClient client = null;
            try
            {
                client = AuthenticationHelper.GetActiveDirectoryClient();
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            try
            {
                string appid = "CHANGEME";
                var application = GetApplication(client, appid);

                if (application != null)
                {

                    role.Id = Guid.NewGuid();
                    role.IsEnabled = true;
                    role.AllowedMemberTypes.Add("User");
                    role.DisplayName = "My Role Name";
                    role.Description = "My Role Description";
                    application.AppRoles.Add(role);

                    await application.UpdateAsync();

                }
              

                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                ModelState.AddModelError("", exception.Message);
                return View();
            }
        }

        private IApplication GetApplication(ActiveDirectoryClient client, string appId)
        {
            return GetApplications(client).SingleOrDefault(a => a.AppId == appId);
        }

        private static IEnumerable<IApplication> GetApplications(ActiveDirectoryClient client)
        {
            IPagedCollection<IApplication> applications = null;
            try
            {
                applications = client.Applications.Take(999).ExecuteAsync().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting Applications {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }

            IEnumerable<IApplication> applications2 = null;
            try
            {
                applications2 = applications.CurrentPage.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError getting Application {0} {1}", e.Message,
                    e.InnerException != null ? e.InnerException.Message : "");
            }
            return applications2;
        }


        /// <summary>
        ///     Gets a list of <see cref="Role" /> objects from Graph.
        /// </summary>
        /// <returns>A view with the list of <see cref="Role" /> objects.</returns>
        public async Task<ActionResult> Index()
        {
            IEnumerable<AppRole> roleList = null;

            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();

                var applications = GetApplications(client);
                roleList = applications.SelectMany(x => x.AppRoles).ToList();
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(roleList);
            }
            return View(roleList);
        }

        /// <summary>
        ///     Gets details of a single <see cref="Role" /> Graph.
        /// </summary>
        /// <returns>A view with the details of a single <see cref="Role" />.</returns>
        public async Task<ActionResult> Details(string objectId)
        {
            DirectoryRole role = null;
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                role = (DirectoryRole) await client.DirectoryRoles.GetByObjectId(objectId).ExecuteAsync();
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View();
            }

            return View(role);
        }
    }
}