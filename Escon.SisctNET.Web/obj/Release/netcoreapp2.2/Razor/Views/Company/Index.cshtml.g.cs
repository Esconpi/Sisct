#pragma checksum "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "edf366649b16b20e4264a3e1fe3b3af53e47360f"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Company_Index), @"mvc.1.0.view", @"/Views/Company/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Company/Index.cshtml", typeof(AspNetCore.Views_Company_Index))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"edf366649b16b20e4264a3e1fe3b3af53e47360f", @"/Views/Company/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0eeffcfdd239a83d2018ec7ff43fcdb329b5b600", @"/Views/_ViewImports.cshtml")]
    public class Views_Company_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<Escon.SisctNET.Model.Company>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Sincronize", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("btn btn-primary"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Create", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
            BeginContext(50, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 3 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
  
    ViewData["Title"] = "Index";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(140, 195, true);
            WriteLiteral("\r\n<div class=\"box box-primary\">\r\n    <div class=\"box-header\">\r\n        <h3 class=\"box-title\"><strong>Empresas Cadastradas</strong></h3>\r\n        <div class=\"btn-group pull-right\">\r\n\r\n            ");
            EndContext();
            BeginContext(335, 145, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "edf366649b16b20e4264a3e1fe3b3af53e47360f4538", async() => {
                BeginContext(386, 90, true);
                WriteLiteral("\r\n                <i class=\"fa fa-refresh\"></i>\r\n                Sincronizar\r\n            ");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_0.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(480, 14, true);
            WriteLiteral("\r\n            ");
            EndContext();
            BeginContext(494, 139, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "edf366649b16b20e4264a3e1fe3b3af53e47360f6097", async() => {
                BeginContext(541, 88, true);
                WriteLiteral("\r\n                <i class=\"fa fa-plus\"></i>\r\n                Nova Empresa\r\n            ");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_2.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_2);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(633, 357, true);
            WriteLiteral(@"


        </div>

    </div>
    <div class=""box-body"">
        <div style=""width:100%; overflow:hidden; overflow-x:scroll; padding:20px;"">

            <table class=""table table-striped table-bordered"" id=""tblIndex"" style=""width:100%"">
                <thead>
                    <tr>
                        <th>
                            ");
            EndContext();
            BeginContext(991, 40, false);
#line 33 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Code));

#line default
#line hidden
            EndContext();
            BeginContext(1031, 91, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(1123, 46, false);
#line 36 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.SocialName));

#line default
#line hidden
            EndContext();
            BeginContext(1169, 91, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(1261, 47, false);
#line 39 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.FantasyName));

#line default
#line hidden
            EndContext();
            BeginContext(1308, 93, true);
            WriteLiteral("\r\n                        </th>\r\n\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(1402, 44, false);
#line 43 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Document));

#line default
#line hidden
            EndContext();
            BeginContext(1446, 118, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th style=\"text-align:center;\">\r\n                            ");
            EndContext();
            BeginContext(1565, 42, false);
#line 46 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Active));

#line default
#line hidden
            EndContext();
            BeginContext(1607, 146, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th></th>\r\n                    </tr>\r\n                </thead>\r\n                <tbody>\r\n");
            EndContext();
#line 52 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                     foreach (var item in Model)
                    {

#line default
#line hidden
            BeginContext(1826, 96, true);
            WriteLiteral("                        <tr>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1923, 39, false);
#line 56 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.Code));

#line default
#line hidden
            EndContext();
            BeginContext(1962, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(2066, 45, false);
#line 59 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.SocialName));

#line default
#line hidden
            EndContext();
            BeginContext(2111, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(2215, 46, false);
#line 62 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.FantasyName));

#line default
#line hidden
            EndContext();
            BeginContext(2261, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(2365, 43, false);
#line 65 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.Document));

#line default
#line hidden
            EndContext();
            BeginContext(2408, 130, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td style=\"text-align:center;\">\r\n                                ");
            EndContext();
            BeginContext(2539, 227, false);
#line 68 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.CheckBoxFor(modelItem => item.Active, new { @data_off = "Todas", @data_on = "Fora", @data_size = "small", @data_toggle = "toggle", @data_onstyle = "primary", @readonly = "true", @id = item.Id, onclick = "return alert()" }));

#line default
#line hidden
            EndContext();
            BeginContext(2766, 139, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td style=\"width:110px;\">\r\n\r\n\r\n                                <a href=\"#\"");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 2905, "\"", 2918, 1);
#line 73 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
WriteAttributeValue("", 2910, item.Id, 2910, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2919, 311, true);
            WriteLiteral(@" class=""showCompareNfe"">
                                    <i class=""glyphicon glyphicon-duplicate pull-left"" style=""margin-left:10px; margin-bottom:10px;"" data-toggle=""tooltip"" data-placement=""top"" title=""Compara NFe""></i>
                                </a>

                                <a href=""#""");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3230, "\"", 3243, 1);
#line 77 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
WriteAttributeValue("", 3235, item.Id, 3235, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3244, 323, true);
            WriteLiteral(@" class=""showCompareCte"">
                                    <i class=""glyphicon glyphicon-duplicate pull-left"" style=""margin-left:10px; margin-bottom:10px; color:gray;"" data-toggle=""tooltip"" data-placement=""top"" title=""Compara CTe""></i>
                                </a>

                                <a href=""#""");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3567, "\"", 3580, 1);
#line 81 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
WriteAttributeValue("", 3572, item.Id, 3572, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3581, 293, true);
            WriteLiteral(@" class=""showTaxation"">
                                    <i class=""fa fa-calculator pull-left"" style=""margin-left:10px; margin-bottom:10px;"" data-toggle=""tooltip"" data-placement=""top"" title=""Tributar""></i>
                                </a>

                                <a href=""#""");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3874, "\"", 3887, 1);
#line 85 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
WriteAttributeValue("", 3879, item.Id, 3879, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3888, 289, true);
            WriteLiteral(@" class=""showDetailRelatory"">
                                    <i class=""fa fa-file pull-left"" style=""margin-left:10px; margin-bottom:10px;"" data-toggle=""tooltip"" data-placement=""top"" title=""Gerar Relatorio""></i>
                                </a>

                                ");
            EndContext();
            BeginContext(4178, 236, false);
#line 89 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.ActionLink(" ", "Edit", new { id = item.Id }, new { @class = " glyphicon glyphicon-pencil pull-left", @style = "margin-left:10px; margin-bottom:10px;", @data_toggle = "tooltip", @data_placement = "top", @title = "Editar Empresa" }));

#line default
#line hidden
            EndContext();
            BeginContext(4414, 36, true);
            WriteLiteral("\r\n\r\n                                ");
            EndContext();
            BeginContext(4451, 335, false);
#line 91 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                           Write(Html.ActionLink(" ", "Delete", new { id = item.Id },
                                    new { onclick = "return confirm('Deseja exluir esse registro?');", @class = "glyphicon glyphicon-trash pull-left", @style = "margin-left:10px; margin-bottom:10px;", @data_toggle = "tooltip", @data_placement = "top", @title = "Excluir Empresa" }));

#line default
#line hidden
            EndContext();
            BeginContext(4786, 68, true);
            WriteLiteral("\r\n                            </td>\r\n                        </tr>\r\n");
            EndContext();
#line 95 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                    }

#line default
#line hidden
            BeginContext(4877, 1079, true);
            WriteLiteral(@"                </tbody>
            </table>

        </div>
    </div>
    <div class=""box-footer""></div>
</div>
<div class=""modal modal-primary fade in"" id=""formShow"" tabindex=""-1"" role=""dialog"" aria-labelledby=""exampleModalLabel"" aria-hidden=""true"">
    <div class=""modal-dialog"" role=""document"">
        <div class=""modal-content"">
            <div class=""modal-header"">
                <button type=""button"" class=""close"" data-dismiss=""modal"" aria-label=""Close"">
                    <span aria-hidden=""true"">&times;</span>
                </button>
            </div>
            <div class=""modal-body"">

            </div>
            <div class=""modal-footer"">
                <button type=""button"" class=""btn btn-primary"" data-dismiss=""modal"">Fechar</button>
            </div>
        </div>
    </div>
</div>
<script type=""text/javascript"">
    $(document).ready(function () {
        

        $(""#tblIndex"").on(""change"", ""input[type='checkbox']"", function(){
            try {

 ");
            WriteLiteral("                   $.ajax({\r\n                    url: \'");
            EndContext();
            BeginContext(5958, 37, false);
#line 128 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Company\Index.cshtml"
                      Write(Url.Action("UpdateStatus", "Company"));

#line default
#line hidden
            EndContext();
            BeginContext(5996, 1959, true);
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
       
        $('.showCompareNfe').click(function () {
            $.get('Company/CompareNfe/' + $(this).attr('id'), function (html) {
                $('#formShow .modal-body').empty();
                $('#formShow .modal-body').append($(html));
                $('#formShow').modal('show');
            });

        });

        $('.showCompareCte').click(function () {
          ");
            WriteLiteral(@"  $.get('Company/CompareCte/' + $(this).attr('id'), function (html) {
                $('#formShow .modal-body').empty();
                $('#formShow .modal-body').append($(html));
                $('#formShow').modal('show');
            });

        });

        $('.showTaxation').click(function () {
            $.get('Company/Taxation/' + $(this).attr('id'), function (html) {
                $('#formShow .modal-body').empty();
                $('#formShow .modal-body').append($(html));
                $('#formShow').modal('show');
            });

        });

        $('.showDetailRelatory').click(function () {
            $.get('Company/Relatory/' + $(this).attr('id'), function (html) {
                $('#formShow .modal-body').empty();
                $('#formShow .modal-body').append($(html));
                $('#formShow').modal('show');
            });

        });

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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<Escon.SisctNET.Model.Company>> Html { get; private set; }
    }
}
#pragma warning restore 1591
