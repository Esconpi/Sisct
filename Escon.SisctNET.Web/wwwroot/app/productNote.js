
function GenerateBillet() {

    try {

        var isSend = confirm('O boleto do DAR será enviado para o cliente. Deseja continuar:');
        if (!isSend) return;

        $('#responseBarCodeGenerateBillet').empty();

        var recipeCode = [];

        recipeCode.push({ RecipeCode: $('#DarSTCO').data('code'), Value: $('#DarSTCO').val() });
        recipeCode.push({ RecipeCode: $('#DarFecop').data('code'), Value: $('#DarFecop').val() });
        recipeCode.push({ RecipeCode: $('#DarIcms').data('code'), Value: $('#DarIcms').val() });
        recipeCode.push({ RecipeCode: $('#DarFecopEI').data('code'), Value: $('#DarFecopEI').val() });
        recipeCode.push({ RecipeCode: $('#DarFunef').data('code'), Value: $('#DarFunef').val() });
        recipeCode.push({ RecipeCode: $('#DarAp').data('code'), Value: $('#DarAp').val() });
        recipeCode.push({ RecipeCode: $('#DarIm').data('code'), Value: $('#DarIm').val() });

        var payload = {
            TypeTaxation: $('#TypeTaxation').val(),
            CpfCnpjIE: $('#CpfCnpjIE').val(),
            ValorTotal: $('#ValorTotal').val(),
            PeriodoReferencia: $('#PeriodoReferencia').val(),
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
                        $('#responseBarCodeGenerateBillet').append("<div class='row'><div class='col-md-12'><div class='box box-success' style='padding:10px;'><i class='fa fa-check-square-o'></i><span style='font-weight:bold'> " + data.response[i].recipedesc + ":</span> <span>" + data.response[i].recipecode + "</span><br><span>Código de barras: </span><span style='font-size:40px;' class='barcode'>" + data.response[i].barcode + "</span><br><span>Linha digitável: </span><span style='font-size:20px;'>" + data.response[i].line + "</span></div></div></div>");
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

$(document).ready(function () {

    $('#btnSendBillet').on('click', function () {
        GenerateBillet();
    });

});