using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LibiadaWeb.Helpers
{
    public static class Helper
    {
        private static readonly String Br = Environment.NewLine;

        public static MvcHtmlString InputElement(this HtmlHelper helper, SelectListItem info, String name,
                                                 String type, IDictionary<string, object> htmlAttributes)
        {
            var inputElement = new TagBuilder("input");
            if (info.Selected)
            {
                inputElement.MergeAttribute("checked", "checked");
            }
            inputElement.MergeAttributes(htmlAttributes);
            inputElement.MergeAttribute("type", type);
            inputElement.MergeAttribute("value", info.Value);
            inputElement.MergeAttribute("name", name);
            inputElement.InnerHtml = info.Text;

            return MvcHtmlString.Create(inputElement.ToString(TagRenderMode.Normal) + Br);
        }

        /// <summary>
        /// Костыль чтобы чекбоксы правильно передавались в контроллер.
        /// без него при неотмеченном чекбоксе в контроллер придёт null и он умрёт.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="name">Имя и идентификатор</param>
        /// <param name="label">Текст рядом с галочкой</param>
        /// <param name="htmlAttributes">дополнительные аттрибуты</param>
        /// <returns></returns>
        public static MvcHtmlString CheckBox(this HtmlHelper helper, String name, String label,
                                             IDictionary<string, object> htmlAttributes)
        {
            var checkBoxElement = new TagBuilder("input");
            checkBoxElement.MergeAttributes(htmlAttributes);
            checkBoxElement.MergeAttribute("type", "checkbox");
            checkBoxElement.MergeAttribute("value", "true");
            checkBoxElement.MergeAttribute("name", name);
            checkBoxElement.MergeAttribute("id", name);

            var labelElement = new TagBuilder("label");
            labelElement.MergeAttribute("for", name);
            labelElement.InnerHtml = label;

            var hiddenElement = new TagBuilder("input");
            hiddenElement.MergeAttribute("type", "hidden");
            hiddenElement.MergeAttribute("value", "false");
            hiddenElement.MergeAttribute("name", name);

            return
                MvcHtmlString.Create(checkBoxElement.ToString(TagRenderMode.Normal) + 
                                    labelElement.ToString(TagRenderMode.Normal) +
                                    hiddenElement.ToString(TagRenderMode.Normal) + Br);
        }

        
    }
}