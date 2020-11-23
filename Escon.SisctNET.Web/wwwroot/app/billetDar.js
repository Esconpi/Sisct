

$(document).ready(function () {

   

    $('.btnSendBillet').click(function () {
        var id = $(this).data('billet');
        $.ajax({
            url: '/BilletDar/SendBillet?id=' + id,
            dataType: 'json',
            type: 'get',
            method: 'get',
            contentType: 'application/json',
            beforeSend: function () {
                $("#remove_" + id).css('display', 'none');
                $('#loader_' + id).css('display', 'block');
                $('#loaderBillet').css('display', 'block');
            },
            complete: function () {
                $("#remove_" + id).css('display', 'block');
                $('#loader_' + id).css('display', 'none');
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