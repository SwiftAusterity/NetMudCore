using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NetMudCore.Data.Architectural.EntityBase;
using NetMudCore.Utility;

namespace NetMudCore.Models
{
    public static class MvcHtmlHelpers
    {
        public static HtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata metadata = self.MetadataProvider.GetMetadataForType(typeof(TModel));

            var provider = self.MetadataProvider.GetModelExplorerForType(typeof(TModel), self).GetExplorerForExpression(typeof(TModel), expression);

            if (metadata != null)
            {
                TagBuilder? description = GetDescriptionHtml(description: provider.Metadata.Description ?? string.Empty);

                if (description != null)
                {
                    return new HtmlString(description.ToString());
                }
            }

            return HtmlString.Empty;
        }

        public static HtmlString DescriptiveLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return DescriptiveLabelFor(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static HtmlString DescriptiveLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object?> htmlAttributes)
        {
            var provider = html.MetadataProvider.GetModelExplorerForType(typeof(TModel), html).GetExplorerForExpression(typeof(TModel), expression);

            TagBuilder? description = GetDescriptionHtml(provider.Metadata.Description ?? string.Empty);

            string labelText = provider.Metadata.DisplayName ?? provider.Metadata.PropertyName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(labelText))
            {
                return HtmlString.Empty;
            }

            TagBuilder tag = new("label");
            tag.MergeAttributes(htmlAttributes);
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(labelText));

            if (description == null)
            {
                tag.InnerHtml.Append(labelText);
                return new HtmlString(tag.RenderBody()?.ToString());
            }
            else
            {
                TagBuilder labelSpan = new("span");
                labelSpan.InnerHtml.Append(labelText);

                string outputHtml = tag.RenderStartTag().ToString()
                                    + labelSpan.RenderBody()?.ToString()
                                    + description?.RenderBody()?.ToString()
                                    + tag.RenderEndTag().ToString();

                return new HtmlString(outputHtml);
            }
        }

        public static HtmlString EditorForMany<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<TValue>>> expression, object additionalViewData, int currentCount = 0, string templateName = "")
        {
            string fieldName = html.NameFor(expression).ToString();
            IEnumerable<TValue> items = expression.Compile()(html.ViewData.Model);

            if (!items.Any())
            {
                items = new List<TValue>()
                {
                    DataUtility.InsantiateThing<TValue>(typeof(EntityTemplatePartial).Assembly)
                };
            }

            string? templateNameOverride = string.IsNullOrWhiteSpace(templateName) ? html.ViewData.ModelMetadata.TemplateHint : templateName;
            return new HtmlString(string.Concat(items.Select((item, i) =>
                html.EditorFor(m => item, templateNameOverride, string.Format("[{0}]", i + currentCount), additionalViewData))).Replace(Environment.NewLine, "").Replace('\u000A', ' ').Trim());
        }

        public static HtmlString EmptyEditorForMany<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<TValue>>> expression, object additionalViewData, int currentCount = 0, string templateName = "")
        {
            string fieldName = html.NameFor(expression).ToString();
            List<TValue> plusOne = new()
            {
                DataUtility.InsantiateThing<TValue>(typeof(EntityTemplatePartial).Assembly)
            };

            string templateNameOverride = string.IsNullOrWhiteSpace(templateName) ? html.ViewData.ModelMetadata.TemplateHint ?? templateName : templateName;
            return new HtmlString(string.Concat(plusOne.Select((item, i) =>
                html.EditorFor(m => item, templateNameOverride, string.Format("[{0}]", i + currentCount), additionalViewData))).Replace(Environment.NewLine, "").Replace('\u000A', ' ').Trim());
        }

        private static TagBuilder? GetDescriptionHtml(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            TagBuilder descTag = new("span");
            descTag.AddCssClass("glyphicon glyphicon-question-sign helpTip");
            descTag.Attributes.Add(new KeyValuePair<string, string?>("title", description));

            return descTag;
        }
    }
}