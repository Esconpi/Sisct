#pragma checksum "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5e58c8094ce2cdccb5a980a01f79db6356bee555"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Access_Index), @"mvc.1.0.view", @"/Views/Access/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Access/Index.cshtml", typeof(AspNetCore.Views_Access_Index))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\_ViewImports.cshtml"
using Escon.SisctNET.Web;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"5e58c8094ce2cdccb5a980a01f79db6356bee555", @"/Views/Access/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0eeffcfdd239a83d2018ec7ff43fcdb329b5b600", @"/Views/_ViewImports.cshtml")]
    public class Views_Access_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<Escon.SisctNET.Model.Access>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-controller", "Profile", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Index", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("size", new global::Microsoft.AspNetCore.Html.HtmlString("large"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("glyphicon glyphicon-circle-arrow-left pull-left"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(49, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 3 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
  
    ViewData["Title"] = "Index";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(139, 71, true);
            WriteLiteral("\r\n<div class=\"box box-primary\">\r\n    <div class=\"box-header\">\r\n        ");
            EndContext();
            BeginContext(210, 120, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "5e58c8094ce2cdccb5a980a01f79db6356bee5554770", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Controller = (string)__tagHelperAttribute_0.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_1.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_1);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(330, 405, true);
            WriteLiteral(@"
        <h3 class=""box-title""><strong>Acesso do Perfil</strong></h3>
    </div>
    <div class=""box-body"">
        <div style=""width:100%; overflow:hidden; overflow-x:scroll; padding:20px;"">

            <table class=""table table-striped table-bordered"" id=""tblIndex"" style=""width:100%"">
                <thead>
                    <tr>
                        <th>
                            ");
            EndContext();
            BeginContext(736, 51, false);
#line 20 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.FunctionalityId));

#line default
#line hidden
            EndContext();
            BeginContext(787, 191, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            Descrição\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(979, 42, false);
#line 26 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Active));

#line default
#line hidden
            EndContext();
            BeginContext(1021, 135, true);
            WriteLiteral("\r\n                        </th>\r\n                      \r\n                    </tr>\r\n                </thead>\r\n                <tbody>\r\n");
            EndContext();
#line 32 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                     foreach (var item in Model)
                    {

#line default
#line hidden
            BeginContext(1229, 98, true);
            WriteLiteral("                        <tr>\r\n\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1328, 23, false);
#line 37 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                           Write(item.Functionality.Name);

#line default
#line hidden
            EndContext();
            BeginContext(1351, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1455, 30, false);
#line 40 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                           Write(item.Functionality.Description);

#line default
#line hidden
            EndContext();
            BeginContext(1485, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1589, 196, false);
#line 43 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                           Write(Html.CheckBoxFor(modelItem => item.Active, new { @data_off = "Não", @data_on = "Sim", @data_size = "small", @data_toggle = "toggle", @data_onstyle = "primary", @readonly = "true", @id = item.Id }));

#line default
#line hidden
            EndContext();
            BeginContext(1785, 93, true);
            WriteLiteral("\r\n                            </td>\r\n                       \r\n                        </tr>\r\n");
            EndContext();
#line 47 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                    }

#line default
#line hidden
            BeginContext(1901, 385, true);
            WriteLiteral(@"                </tbody>
            </table>
        </div>
    </div>
    <div class=""box-footer""></div>
</div>

<script type=""text/javascript"">
    $(document).ready(function () {
        $(function () {

             $(""#tblIndex"").on(""change"", ""input[type='checkbox']"", function(){
                try {

                     $.ajax({
                        url: '");
            EndContext();
            BeginContext(2288, 36, false);
#line 63 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Access\Index.cshtml"
                          Write(Url.Action("UpdateStatus", "Access"));

#line default
#line hidden
            EndContext();
            BeginContext(2325, 720, true);
            WriteLiteral(@"',
                        dataType: 'json',
                        type: 'post',
                        contentType: 'application/json',
                        data: JSON.stringify({ ""Id"": $(this).prop('id'), ""Active"": $(this).prop('checked') }),
                        processData: false,
                        success: function (data, textStatus, jQxhr) {

                        },
                        error: function (jqXhr, textStatus, errorThrown) {
                            alert(errorThrown);
                        }
                     });

                } catch (e) {
                    console.log(e);
                }
             });


        })
    })
</script>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<Escon.SisctNET.Model.Access>> Html { get; private set; }
    }
}
#pragma warning restore 1591
