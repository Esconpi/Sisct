$(document).ready(function () {

    $("#tblGenerated").DataTable();

    $("#tblNotGenerated").DataTable();

    $("#tblPaidOut").DataTable();

    $("#tblNotPaidOut").DataTable();

    $("#tblAfterDueDate").DataTable({
        "searching": false,
        "info": false,
        "pageLength":3
    });

    $("#tblBeforeDueDate").DataTable({
        "searching": false,
        "pageLength": 3,
        "info": false
    });

    $("#tblDueDate").DataTable({
        "searching": false,
        "info": false,
        "pageLength": 3
    });

    $('[data-toggle="tooltip"]').tooltip();

    $('#btnReload').click(function () {

        $('#btnFiltrar').submit();
    })

    $('#btnUpdatePaidOut').click(function () {

        var period = $("#PeriodId option:selected").val();
        $.ajax({
            url: `/BilletDar/SyncPaidOut?periodReference=${period}`,
            dataType: 'json',
            type: 'get',
            method: 'get',
            contentType: 'application/json',
            beforeSend: function () {
                $(this).attr('disabled', 'disable');
                $("#loader_syncpaidout").css('display', 'block');
            },
            complete: function () {
                $(this).removeAttr('disabled');
                $("#loader_syncpaidout").css('display', 'none');
            },
            success: function (data, textStatus, jQxhr) {

                if (data.code === 200) {
                    alert('Os pagamentos referentes ao período selecionado foram sincronizados');
                    $('#btnFiltrar').trigger('click');
                } else if (data.code === 201) {
                    alert(data.message);
                }
                
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert('Falha ao tentar sincronizar pagamentos');
            }
        });
    });

    $(".btnGenerateDar").click(function () {

        var year = $('#periodYear').val();
        var month = $('#periodMonth').val();
        var companyId = $(this).data("company");

        $.ajax({
            url: `/ProductNote/Relatory?id=${companyId}&typeTaxation=${1}&type=${1}&year=${year}&month=${month}&nota=`,
            dataType: 'html',
            type: 'post',
            method: 'post',
            contentType: 'html',
            beforeSend: function () {
                $('#btnProccess_' + companyId).attr('disabled', 'disable');
                $("#loader_" + companyId).css('display', 'block');
            },
            complete: function () {

            },
            success: function (data, textStatus, jQxhr) {

                var html = $(data);

                var recipeCode = [];

                recipeCode.push({ RecipeCode: html.find('#DarSTCO').data('code'), Value: html.find('#DarSTCO').val() });
                recipeCode.push({ RecipeCode: html.find('#DarFecop').data('code'), Value: html.find('#DarFecop').val() });
                recipeCode.push({ RecipeCode: html.find('#DarIcms').data('code'), Value: html.find('#DarIcms').val() });
                recipeCode.push({ RecipeCode: html.find('#DarFecopEI').data('code'), Value: html.find('#DarFecopEI').val() });
                recipeCode.push({ RecipeCode: html.find('#DarFunef').data('code'), Value: html.find('#DarFunef').val() });
                recipeCode.push({ RecipeCode: html.find('#DarAp').data('code'), Value: html.find('#DarAp').val() });
                recipeCode.push({ RecipeCode: html.find('#DarIm').data('code'), Value: html.find('#DarIm').val() });

                var payload = {
                    TypeTaxation: html.find('#TypeTaxation').val(),
                    CpfCnpjIE: html.find('#CpfCnpjIE').val(),
                    ValorTotal: html.find('#ValorTotal').val(),
                    PeriodoReferencia: html.find('#PeriodoReferencia').val(),
                    RecipeCodeValues: recipeCode
                }

                $.ajax({
                    url: '/ProductNote/GenerateBillet',
                    dataType: 'json',
                    type: 'post',
                    method: 'post',
                    contentType: 'application/json',
                    data: JSON.stringify(payload),
                    beforeSend: function () {

                    },
                    complete: function () {
                        $("#loader_" + companyId).css('display', 'none');
                    },
                    success: function (data, textStatus, jQxhr) {

                        for (var i = 0; i < data.response.length; i++) {

                            if (data.response[i].code === 200) {
                                $('#divResponseProcess').prepend("<div class='row'><div class='col-md-12'><div class='box box-success' style='padding:10px;'><i class='fa fa-check-square-o'></i> <span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span> &nbsp;<a href='/Billets/" + data.response[i].download + "' target='blank'><i class='fa fa-download'></i></a> <br><span>Código de barras: </span><span style='font-size:30px;' class='barcode'>" + data.response[i].barcode + "</span><br><span>Linha digitável: </span><span style='font-size:16px;'>" + data.response[i].line + "</span></div></div></div>");
                                $("#loader_" + companyId).closest('tr').css('background-color', '#EAFFC8');
                            } else {
                                $('#divResponseProcess').prepend("<div class='row'><div class='col-md-12'><div class='box box-danger' style='padding:10px;'><i class='fa fa-warning'></i><span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span><br><span>" + data.response[i].message + "</span></div></div></div>");
                                $("#loader_" + companyId).closest('tr').css('background-color', '#FFC8C8');
                            }
                        }

                        $('#formResponse').modal('show');
                    },
                    error: function (jqXhr, textStatus, errorThrown) {
                        $('#btnProccess_' + companyId).removeAttr('disabled');
                        alert(jqXhr.responseText);
                    }
                });


            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert('Falha ao enviar o boleto');
            }
        });

    });


});