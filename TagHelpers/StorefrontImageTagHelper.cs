using FoodMart.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FoodMart.TagHelpers;

[HtmlTargetElement("img", Attributes = "src", TagStructure = TagStructure.WithoutEndTag)]
public sealed class StorefrontImageTagHelper(IWebHostEnvironment environment, IUrlHelperFactory urls) : TagHelper
{
    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var url = urls.GetUrlHelper(ViewContext);
        var source = output.Attributes["src"].Value?.ToString();
        var fallback = url.Content("~/images/image-unavailable.svg");
        if (!FormRules.SafeUrl(source)) source = fallback;
        else if (source!.StartsWith('/') || source.StartsWith("~/"))
        {
            var path = source.StartsWith("~/") ? source[2..] : source.TrimStart('/');
            var basePath = ViewContext.HttpContext.Request.PathBase.Value?.Trim('/');
            if (!string.IsNullOrEmpty(basePath) && path.StartsWith(basePath + "/")) path = path[(basePath.Length + 1)..];
            path = path.Split('?', '#')[0];
            if (!environment.WebRootFileProvider.GetFileInfo(path).Exists) source = fallback;
        }
        output.Attributes.SetAttribute("src", url.Content(source!));
        output.Attributes.SetAttribute("decoding", "async");
        output.Attributes.SetAttribute("data-fallback", fallback);
    }
}
