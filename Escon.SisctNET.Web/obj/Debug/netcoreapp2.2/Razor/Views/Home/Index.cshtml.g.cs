#pragma checksum "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "a0cbcabeb684eb55f77ac6c46749ce744a69e0fa"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Home_Index), @"mvc.1.0.view", @"/Views/Home/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Home/Index.cshtml", typeof(AspNetCore.Views_Home_Index))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a0cbcabeb684eb55f77ac6c46749ce744a69e0fa", @"/Views/Home/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0eeffcfdd239a83d2018ec7ff43fcdb329b5b600", @"/Views/_ViewImports.cshtml")]
    public class Views_Home_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<Escon.SisctNET.Model.Company>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(50, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 3 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
  
    ViewData["Title"] = "Index";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(140, 354, true);
            WriteLiteral(@"
<div class=""box box-primary"">
    <div class=""box-body"">
        <div style=""width:100%; overflow:hidden; overflow-x:scroll; padding:20px;"">

            <table class=""table table-striped table-bordered"" id=""tblIndex"" style=""width:100%"">
                <thead>
                    <tr>
                        <th>
                            ");
            EndContext();
            BeginContext(495, 40, false);
#line 16 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Code));

#line default
#line hidden
            EndContext();
            BeginContext(535, 91, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(627, 46, false);
#line 19 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.SocialName));

#line default
#line hidden
            EndContext();
            BeginContext(673, 91, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(765, 47, false);
#line 22 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.FantasyName));

#line default
#line hidden
            EndContext();
            BeginContext(812, 93, true);
            WriteLiteral("\r\n                        </th>\r\n\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(906, 44, false);
#line 26 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Document));

#line default
#line hidden
            EndContext();
            BeginContext(950, 91, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(1042, 42, false);
#line 29 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Status));

#line default
#line hidden
            EndContext();
            BeginContext(1084, 91, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th>\r\n                            ");
            EndContext();
            BeginContext(1176, 45, false);
#line 32 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                       Write(Html.DisplayNameFor(model => model.Incentive));

#line default
#line hidden
            EndContext();
            BeginContext(1221, 146, true);
            WriteLiteral("\r\n                        </th>\r\n                        <th></th>\r\n                    </tr>\r\n                </thead>\r\n                <tbody>\r\n");
            EndContext();
#line 38 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                     foreach (var item in Model)
                    {

#line default
#line hidden
            BeginContext(1440, 96, true);
            WriteLiteral("                        <tr>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1537, 39, false);
#line 42 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.Code));

#line default
#line hidden
            EndContext();
            BeginContext(1576, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1680, 45, false);
#line 45 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.SocialName));

#line default
#line hidden
            EndContext();
            BeginContext(1725, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1829, 46, false);
#line 48 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.FantasyName));

#line default
#line hidden
            EndContext();
            BeginContext(1875, 103, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n                                ");
            EndContext();
            BeginContext(1979, 43, false);
#line 51 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                           Write(Html.DisplayFor(modelItem => item.Document));

#line default
#line hidden
            EndContext();
            BeginContext(2022, 105, true);
            WriteLiteral("\r\n                            </td>\r\n                            <td>\r\n\r\n                                ");
            EndContext();
            BeginContext(2128, 274, false);
#line 55 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                           Write(Html.CheckBoxFor(modelItem => item.Status, new { @data_off = "Todas", @data_on = "Fora", @data_size = "small", @data_toggle = "toggle",@data_offstyle = "warning", @data_onstyle = "primary", @readonly = "true", @id = item.Id, onclick = "return alert()", @disabled = "true" }));

#line default
#line hidden
            EndContext();
            BeginContext(2402, 107, true);
            WriteLiteral("\r\n\r\n                            </td>\r\n                            <td>\r\n\r\n                                ");
            EndContext();
            BeginContext(2510, 273, false);
#line 60 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                           Write(Html.CheckBoxFor(modelItem => item.Incentive, new { @data_off = "Não", @data_on = "Sim", @data_size = "small", @data_toggle = "toggle",@data_offstyle = "danger", @data_onstyle = "success", @readonly = "true", @id = item.Id, onclick = "return alert()", @disabled = "true" }));

#line default
#line hidden
            EndContext();
            BeginContext(2783, 141, true);
            WriteLiteral("\r\n\r\n                            </td>\r\n                            <td style=\"width:110px;\">\r\n\r\n\r\n                                <a href=\"#\"");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 2924, "\"", 2937, 1);
#line 66 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
WriteAttributeValue("", 2929, item.Id, 2929, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2938, 311, true);
            WriteLiteral(@" class=""showCompareNfe"">
                                    <i class=""glyphicon glyphicon-duplicate pull-left"" style=""margin-left:10px; margin-bottom:10px;"" data-toggle=""tooltip"" data-placement=""top"" title=""Compara NFe""></i>
                                </a>

                                <a href=""#""");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3249, "\"", 3262, 1);
#line 70 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
WriteAttributeValue("", 3254, item.Id, 3254, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3263, 323, true);
            WriteLiteral(@" class=""showCompareCte"">
                                    <i class=""glyphicon glyphicon-duplicate pull-left"" style=""margin-left:10px; margin-bottom:10px; color:gray;"" data-toggle=""tooltip"" data-placement=""top"" title=""Compara CTe""></i>
                                </a>

                                <a href=""#""");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3586, "\"", 3599, 1);
#line 74 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
WriteAttributeValue("", 3591, item.Id, 3591, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3600, 293, true);
            WriteLiteral(@" class=""showTaxation"">
                                    <i class=""fa fa-calculator pull-left"" style=""margin-left:10px; margin-bottom:10px;"" data-toggle=""tooltip"" data-placement=""top"" title=""Tributar""></i>
                                </a>

                                <a href=""#""");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3893, "\"", 3906, 1);
#line 78 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
WriteAttributeValue("", 3898, item.Id, 3898, 8, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3907, 323, true);
            WriteLiteral(@" class=""showDetailRelatory"">
                                    <i class=""fa fa-file pull-left"" style=""margin-left:10px; margin-bottom:10px;"" data-toggle=""tooltip"" data-placement=""top"" title=""Gerar Relatorio""></i>
                                </a>

                            </td>
                        </tr>
");
            EndContext();
#line 84 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                    }

#line default
#line hidden
            BeginContext(4253, 1071, true);
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
            WriteLiteral("           $.ajax({\r\n                    url: \'");
            EndContext();
            BeginContext(5326, 34, false);
#line 117 "C:\SVN\branches\SisctNET\Escon.SisctNET.Web\Views\Home\Index.cshtml"
                      Write(Url.Action("UpdateStatus", "Home"));

#line default
#line hidden
            EndContext();
            BeginContext(5361, 1952, true);
            WriteLiteral(@"',
                    dataType: 'json',
                    type: 'post',
                    contentType: 'application/json',
                    data: JSON.stringify({ ""Id"": $(this).prop('id'), ""Status"": $(this).prop('checked') }),
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
            $.get");
            WriteLiteral(@"('Company/CompareCte/' + $(this).attr('id'), function (html) {
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
