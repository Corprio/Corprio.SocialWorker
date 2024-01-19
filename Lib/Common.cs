using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;

namespace Corprio.SocialWorker
{
    public static class Common
    {
        public const string LogoImageKey = "Logo";

        /// <summary>
        /// Create a MS-Word-ribbon-like tool bar
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static HtmlEditorToolbarBuilder StandardToolbar(this HtmlEditorToolbarBuilder builder)
        {

            builder.Items(items => {

                items.Add().Name(HtmlEditorToolbarItem.Undo);
                items.Add().Name(HtmlEditorToolbarItem.Redo);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add()
                    .Name("size")
                    .AcceptedValues(new[] { "0.8em", "1em", "1.2em", "1.5em", "2em", "2.5em", "3em", "4em", "5em" });
                items.Add()
                    .Name("font")
                    .AcceptedValues(new[] { "Arial", "Calibri", "Cambria", "Candara", "Courier New", "Garamond", "Georgia", "Helvetica", "Impact", "Lucida Console", "Tahoma", "Times New Roman", "Trebuchet MS", "Verdana" });
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.Bold);
                items.Add().Name(HtmlEditorToolbarItem.Italic);
                items.Add().Name(HtmlEditorToolbarItem.Underline);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.AlignLeft);
                items.Add().Name(HtmlEditorToolbarItem.AlignCenter);
                items.Add().Name(HtmlEditorToolbarItem.AlignJustify);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.Color);
                items.Add().Name(HtmlEditorToolbarItem.Background);
                items.Add().Name(HtmlEditorToolbarItem.Link);
                items.Add().Name(HtmlEditorToolbarItem.Image);
                items.Add().Name(HtmlEditorToolbarItem.OrderedList);
                items.Add().Name(HtmlEditorToolbarItem.BulletList);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.InsertTable);
                items.Add().Name(HtmlEditorToolbarItem.InsertHeaderRow);
                items.Add().Name(HtmlEditorToolbarItem.InsertRowAbove);
                items.Add().Name(HtmlEditorToolbarItem.InsertRowBelow);
                items.Add().Name(HtmlEditorToolbarItem.InsertColumnLeft);
                items.Add().Name(HtmlEditorToolbarItem.InsertColumnRight);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.DeleteColumn);
                items.Add().Name(HtmlEditorToolbarItem.DeleteRow);
                items.Add().Name(HtmlEditorToolbarItem.DeleteTable);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.CellProperties);
                items.Add().Name(HtmlEditorToolbarItem.TableProperties);
                items.Add().Name(HtmlEditorToolbarItem.Separator);
                items.Add().Name(HtmlEditorToolbarItem.Clear);
            });

            builder.Multiline(false);
            return builder;
        }
    }
}
