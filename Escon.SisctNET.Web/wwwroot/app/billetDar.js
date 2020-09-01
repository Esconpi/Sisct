

$(document).ready(function () {

    $('.btnSendBillet').click(function () {

        $.ajax({
            url: '/BilletDar/SendBillet?id=' + $(this).data('billet'),
            dataType: 'json',
            type: 'get',
            method: 'get',
            contentType: 'application/json',
            beforeSend: function () {
                $(this).attr('disabled', 'disable');
                $('#loaderBillet').css('display', 'block');
            },
            complete: function () {
                $(this).removeAttr('disabled');
                $('#loaderBillet').css('display', 'none');
            },
            success: function (data, textStatus, jQxhr) {
                alert('Boleto enviado com sucesso.')
            },
            error: function (jqXhr, textStatus, errorThrown) {
                alert('Falha ao enviar o boleto');
            }
        });

    });

});