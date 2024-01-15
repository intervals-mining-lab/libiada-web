﻿namespace Libiada.Web.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The calculation controller.
/// </summary>
[Authorize]
public class TaskManagerController : Controller
{
    /// <summary>
    /// The index.
    /// </summary>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Index(int? id)
    {
        if (TempData["ErrorMessage"] != null)
        {
            ViewBag.Error = true;
            ViewBag.ErrorMessage = ViewBag.UserError = TempData["ErrorMessage"];
        }
        
        return View();
    }
}
