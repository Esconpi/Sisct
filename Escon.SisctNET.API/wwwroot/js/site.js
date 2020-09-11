$(document).ready(function () {

    document.addEventListener("keydown", function (zEvent) {
        if (zEvent.ctrlKey && zEvent.altKey && zEvent.key === "e") {  // case sensitive
            alert(zEvent);
        }
    });

    $.fn.dataTable.ext.errMode = 'none';

    var base64Photo = "";

    $('#Profile').click(function () {

        $.ajax(
            {
                type: 'GET',
                url: '/User/PersonById/' + $('#HiddenPersonId').val(),
                dataType: 'json',
                cache: false,
                async: true,
                success: function (data) {

                    $('#firstName').val(data.firstName);
                    $('#lastName').val(data.lastName);
                    $('#document').val(data.document);
                    $('#email').val(data.email);
                    $('#company').val(data.company.socialName);
                    $('#profile').val(data.profile.name);
                    $('#imgProfilePhoto').attr('src', data.photo);

                    $('#editProfile').modal('show');

                },
                error: function (jqXhr, textStatus, errorThrown) {
                    console.log(errorThrown.message);
                }
            });

    });

    $('#updateProfile').click(function () {
        $('.loader').css('display', 'block');
        $.ajax(
            {
                url: '/User/UpdatePartial/',
                dataType: 'json',
                type: 'post',
                contentType: 'application/json',
                processData: false,
                cache: false,
                async: true,
                data: JSON.stringify({ "Id": $('#HiddenPersonId').val() , "FirstName": "" + $('#firstName').val() + "", "LastName": "" + $('#lastName').val() + "", "Document":""+ $('#document').val() + "", "Password":"" + $('#password').val() + "", "Photo": "" + base64Photo + "" }),
                success: function (data) {
                    $('.loader').css('display', 'none');

                    if (data.requestcode === 200) {
                        $('#spanMessage').find('.pull-right').text(data.message);
                        $('#spanMessage').css('display', 'block');
                        
                    }
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    console.log(errorThrown.message);
                }
            });

    });

    $(document).keypress(function (event) {
        if (event.keyCode === 13) {
            $('.modal.in').modal('hide');
        }
    });

    function base64ProfilePhoto(elementId) {

        var reader = new FileReader();
        var file = document.getElementById(elementId).files[0];
        reader.readAsDataURL(file);
        reader.onload = function () {
            base64Photo = reader.result;
            $('#imgProfilePhoto').attr('src', base64Photo);
        };

        reader.onerror = function (error) {
            console.log('Error: ', error);
        };
    }

    $('#profilePhoto').change(function () {
        base64ProfilePhoto('profilePhoto');
    });

    $('[data-toggle="tooltip"]').tooltip();

    $(".selectoption").select2({
        closeOnSelect: false,
        placeholder: 'Selecione uma opção',
        allowClear: true
    });

    $(".selectoption").select2({
        closeOnSelect: false,
        placeholder: 'Selecione uma opção',
        allowClear: true
    });

    $(".selectoption").on('change', function () {

        $(this).select2('close');

        var _field = $('.nextInput');
        var _index = _field.index(this);

        if (_field[_index + 1] !== null) {
            var i = 1;
            _next = _field[_index + i];
            
            while ($(_next).is(":disabled")) {
                i = i + 1;
                _next = _field[_index + i];
            }
            _next.focus();
        }
    });

    $('.cpfformatmask').inputmask("999.999.999-99");
    $('.cnpjformatmask').inputmask("99.999.999/9999-99");

    $(".moneyformatmask").inputmask('decimal', {
        radixPoint: ",",
        groupSeparator: ".",
        allowMinus: false,
        prefix: 'R$ ',
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        unmaskAsNumber: true
    });

    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        language: 'pt-BR'
    }).keydown(function (event) {

        if (event.which === 13) {
            $('.datepicker').datepicker('hide');
            $('#txtNumDocument').focus();
        }

    });

    $('#tblIndex').DataTable();

});