﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Forms.Data.Storage;
using PerplexUmbraco.Forms.Code;
using System.Web;
using System.Web.Caching;

namespace PerplexUmbraco.Forms
{
    public class MvcApplication : IApplicationEventHandler
    {
        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var treeService = ApplicationContext.Current.Services.ApplicationTreeService;

            // Hide default Umbraco Forms folder, we use our own to display folders
            var umbFormTree = treeService.GetByAlias("form");
            if (umbFormTree != null && umbFormTree.Initialize)
            {
                umbFormTree.Initialize = false;
                treeService.SaveTree(umbFormTree);
            }

            // Add our own tree if it's not there yet
            var pplxFormTree = treeService.GetByAlias("perplexForms");
            if (pplxFormTree == null)
            {
                treeService.MakeNew(true, 1, "forms", "perplexForms", "Forms", "icon-folder", "icon-folder-open", "PerplexUmbraco.Forms.Controllers.PerplexFormTreeController, Perplex.Umbraco.Forms");
            }

            FormStorage.Created += FormStorage_Created;
            FormStorage.Deleted += FormStorage_Deleted;
        }

        void FormStorage_Created(object sender, Umbraco.Forms.Core.FormEventArgs e)
        {
            var form = e.Form;

            // Was this form created in a folder?
            var sessionId = UmbracoContext.Current.Security.GetSessionId();
            var folderId = HttpContext.Current.Cache[sessionId + "_folderId"];
            if (folderId == null) return;

            var folder = PerplexFolder.Get(folderId.ToString());
            if (folder == null) return;

            folder.forms.Add(form.Id.ToString());
            PerplexFolder.SaveAll();
        }

        void FormStorage_Deleted(object sender, Umbraco.Forms.Core.FormEventArgs e)
        {
            // If this Form was stored in a Folder, remove it.
            var form = e.Form;
            var folder = PerplexFolder.Get(f => f.forms.Any(fid => fid == form.Id.ToString()));
            if (folder != null)
            {
                folder.forms.Remove(form.Id.ToString());
                PerplexFolder.SaveAll();
            }
        }

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }
    }
}
