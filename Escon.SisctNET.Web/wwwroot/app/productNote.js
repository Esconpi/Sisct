
function GenerateBillet() {

    try {

        var dueDate = null;

        var isSend = confirm('O boleto do DAR será enviado para o cliente. Deseja continuar:');
        if (!isSend) return;

        if (new Date().getDate() > 15) {
            var promptResponse = prompt('Informe a data de vencimento do boleto');

            if (!isDDMMYYYY(promptResponse)) {
                alert('Data com o formato inválido.');
                return;
            }

            if (!ValidateDate(promptResponse)) {
                return;
            }

            dueDate = promptResponse;
        }

        $('#responseBarCodeGenerateBillet').empty();

        var recipeCode = [];

        recipeCode.push({ RecipeCode: $('#DarSTCO').data('code'), Value: $('#DarSTCO').val(), St: $('#DarSTCO').data('substituicao') });
        recipeCode.push({ RecipeCode: $('#DarFecop').data('code'), Value: $('#DarFecop').val(), St: $('#DarFecop').data('substituicao') });
        recipeCode.push({ RecipeCode: $('#DarIcms').data('code'), Value: $('#DarIcms').val(), St: $('#DarIcms').data('substituicao') });
        recipeCode.push({ RecipeCode: $('#DarFecopEI').data('code'), Value: $('#DarFecopEI').val(), St: $('#DarFecopEI').data('substituicao') });
        recipeCode.push({ RecipeCode: $('#DarFunef').data('code'), Value: $('#DarFunef').val(), St: $('#DarFunef').data('substituicao') });
        recipeCode.push({ RecipeCode: $('#DarAp').data('code'), Value: $('#DarAp').val(), St: $('#DarAp').data('substituicao') });
        recipeCode.push({ RecipeCode: $('#DarIm').data('code'), Value: $('#DarIm').val(), St: $('#DarIm').data('substituicao')});

        var payload = {
            TypeTaxation: $('#TypeTaxation').val(),
            CpfCnpjIE: $('#CpfCnpjIE').val(),
            ValorTotal: $('#ValorTotal').val(),
            PeriodoReferencia: $('#PeriodoReferencia').val(),
            RecipeCodeValues: recipeCode,
            Vencimento: dueDate === null ? null : ConvertDateToJson(dueDate)
        }

        $.ajax({
            url: '/ProductNote/GenerateBillet',
            dataType: 'json',
            type: 'post',
            method: 'post',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            beforeSend: function () {
                $('#btnSendBillet').attr('disabled', 'disable');
                $('#responseMessageGenerateBillet').css('color', 'orange');
                $('#responseMessageGenerateBillet').text('Requisição enviada para o servidor, por favor aguarde.');
                $('#loaderBillet').css('display', 'block');
            },
            complete: function () {
                $('#btnSendBillet').removeAttr('disabled');
                $('#loaderBillet').css('display', 'none');
            },
            success: function (data, textStatus, jQxhr) {

                for (var i = 0; i < data.response.length; i++) {

                    if (data.response[i].code === 200) {
                        $('#responseBarCodeGenerateBillet').append("<div class='row'><div class='col-md-12'><div class='box box-success' style='padding:10px;'><i class='fa fa-check-square-o'></i> <span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span> &nbsp;<a href='/Billets/" + data.response[i].download + "' target='blank'><i class='fa fa-download'></i></a> <br><span>Código de barras: </span><span style='font-size:40px;' class='barcode'>" + data.response[i].barcode + "</span><br><span>Linha digitável: </span><span style='font-size:20px;'>" + data.response[i].line + "</span></div></div></div>");
                    } else {
                        $('#responseBarCodeGenerateBillet').append("<div class='row'><div class='col-md-12'><div class='box box-danger' style='padding:10px;'><i class='fa fa-warning'></i><span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span><br><span>" + data.response[i].message + "</span></div></div></div>");
                    }
                }

                $('#responseMessageGenerateBillet').css('color', 'green');
                $('#responseMessageGenerateBillet').text("Processo finalizado com sucesso!");
            },
            error: function (jqXhr, textStatus, errorThrown) {
                $('#responseMessageGenerateBillet').css('color', 'red');
                $('#responseMessageGenerateBillet').text(jqXhr.responseText);
            }
        });

    } catch (e) {
        console.log(e);
    }

}

function LoadBillets() {

    var companyId = $('#hddCompanyId').val();
    var periodReference = $('#PeriodoReferencia').val();

    $.ajax({
        url: '/ProductNote/GetDocumentsDar?companyId=' + companyId + "&periodReference=" + periodReference,
        dataType: 'json',
        type: 'get',
        method: 'get',
        contentType: 'application/json',
        beforeSend: function () {

            $('#btnSendBillet').attr('disabled', 'disable');
            $('#responseMessageGenerateBillet').css('color', 'orange');
            $('#responseMessageGenerateBillet').text('Requisição enviada para o servidor, por favor aguarde.');
            $('#loaderBillet').css('display', 'block');
        },
        complete: function () {
            $('#btnSendBillet').removeAttr('disabled');
            $('#loaderBillet').css('display', 'none');
        },
        success: function (data, textStatus, jQxhr) {

            for (var i = 0; i < data.response.length; i++) {

                if (data.response[i].code === 200) {
                    $('#responseBarCodeGenerateBillet').append("<div class='row'><div class='col-md-12'><div class='box box-success' style='padding:10px;'><i class='fa fa-check-square-o'></i> <span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span> &nbsp;<a href='/Billets/" + data.response[i].download + "' target='blank'><i class='fa fa-download'></i></a> <br><span>Código de barras: </span><span style='font-size:40px;' class='barcode'>" + data.response[i].barcode + "</span><br><span>Linha digitável: </span><span style='font-size:20px;'>" + data.response[i].line + "</span></div></div></div>");
                } else {
                    $('#responseBarCodeGenerateBillet').append("<div class='row'><div class='col-md-12'><div class='box box-danger' style='padding:10px;'><i class='fa fa-warning'></i><span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span><br><span>" + data.response[i].message + "</span></div></div></div>");
                }
            }
        },
        error: function (jqXhr, textStatus, errorThrown) {
            $('#responseMessageGenerateBillet').css('color', 'red');
            $('#responseMessageGenerateBillet').text(jqXhr.responseText);
        }
    });
}

$(document).ready(function () {

    $('#btnSendBillet').on('click', function () {
        GenerateBillet();
    });

    setTimeout(LoadBillets(), 2000);

});

function isDDMMYYYY(str) {
    var date_regex = /^(0?[1-9]|[12][0-9]|3[01])[\/\-](0?[1-9]|1[012])[\/\-]\d{4}$/;
    return date_regex.test(str);
}

function ConvertDateToJson(dtString) {

    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var yyyy = today.getFullYear();

    if (dtString !== null) {
        var partDate = dtString.split('/');
        dd = partDate[0];
        mm = partDate[1];
        yyyy = partDate[2];
    }

    today = yyyy + '-' + mm + '-' + dd;
    return today;
}

function ValidateDate(strDate) {

    var dd = parseInt(strDate.split('/')[0]);
    var mm = parseInt(strDate.split('/')[1]);
    var yy = parseInt(strDate.split('/')[2]);

    var currentDate = new Date();
    var settedDate = new Date(yy, (mm - 1), dd);
    var dueDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), 15);

    if (settedDate < dueDate) {
        alert('A data de vencimento não pode ser inferior ao dia 15 do mês corrente');
        return false;
    }

    return true;
}