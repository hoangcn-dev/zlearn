using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ZLearn.Web.Extensions
{
    public static class BreadcrumbExtensions
    {
        public static void SetBreadcrumbs(this ViewDataDictionary viewData, params (string Text, string Url)[] items)
        {
            viewData["Breadcrumbs"] = items.ToList();
        }

        public static void AddBreadcrumb(this ViewDataDictionary viewData, string text, string url = "")
        {
            var breadcrumbs = viewData["Breadcrumbs"] as List<(string Text, string Url)> 
                ?? new List<(string Text, string Url)>();
            breadcrumbs.Add((text, url));
            viewData["Breadcrumbs"] = breadcrumbs;
        }
    }
}
