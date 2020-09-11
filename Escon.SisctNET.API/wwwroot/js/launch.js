
$(document).ready(function () {

    $(document).bind("ajaxSend", function () {
        $('.loader').css('display', 'blick');
    }).bind("ajaxComplete", function () {
        $('.loader').css('display', 'none');
    });

    $(window).keydown(function (e) {
        //console.log(e.key);
        //

        if (e.altKey && e.key === '1') {
            $('.selectoption').select2('close');
            $("#Companies").focus();
            $("#Companies").select2('open');
        }
        else if (e.altKey && e.key === '2') {
            $('.selectoption').select2('close');
            $("#Expense").focus();
            $("#Expense").select2('open');
        } else if (e.altKey && e.key === '3') {
            $('.selectoption').select2('close');
            $("#DocumentType").focus();
            $("#DocumentType").select2('open');
        } else if (e.altKey && e.key === '4') {
            $('.selectoption').select2('close');
            $("#Historic").focus();
            $("#Historic").select2('open');
        } else if (e.altKey && e.key === '5') {
            $("#txtNumDocument").focus();
        } else if (e.altKey && e.key === 'q') {
            $("#txtValue").focus();
        } else if (e.altKey && e.key === 'w') {
            $("#btnSaveProtocol").focus();
        } else if (e.altKey && e.key === 'r') {
            $("#btnSaveLot").focus();
        }
    });

    $('span').css('font-style', 'italic');
    $('#txtValue').focus();
    $('#btnAddExpense').attr('disabled', 'disabled');
    $('#btnSaveProtocol').attr('disabled', 'disabled');
    $('.loader').css('display', 'none');

    function LoadLots() {

        $.ajax(
            {
                type: 'GET',
                url: '/Launch/ProtocolsById/' + $('#txtNumeroLote').val(),
                dataType: 'html',
                cache: false,
                async: true,
                success: function (data) {


                    $('#divLot').html(data);

                    $('#tblAllProtocols').DataTable({
                        "paging": false,
                        "searching": false,
                        "ordering": false,
                        "bLengthChange": false
                    });

                    $('#tblAllProtocols tbody').on('click', '.btnRemoveProtocol', function () {

                        var yes = confirm(`Você deseja remover o protocolo ${$(this).attr('data-value')} deste Lote?`);
                        if (!yes)
                            return;

                        $.ajax({
                            type: 'GET',
                            url: '/Launch/RemoveProtocol/' + $(this).attr('id'),
                            dataType: "json",
                            cache: false,
                            async: true,
                            success: function (data) {
                                if (data.code === 200) {
                                    ShowNotify('Protocolo removido com sucesso.', 'Remover', 'success');
                                    LoadLots();
                                }
                                else {
                                    ShowNotify(data.message, 'Remover', 'danger');
                                }
                            }
                        });

                    });
                }
            });
    }

    setTimeout(function () { LoadLots(); }, 500);

    var protocolIsValid = false;
    var Interest = parseFloat("0");
    var Penalty = parseFloat("0");
    var Rate = parseFloat("0");
    var Discount = parseFloat("0");
    var Registry = parseFloat("0");
    var subTotal = parseFloat("0");
    var base64 = "";
    var dtLaunch = "";

    var complementIsRequired = false;

    $("#Expense").on('change', function () {

        $(this).select2('close');

    });

    $('#btnAddExpense').click(function () {

        {
            if ($("#Companies option:selected").val() === "0") {

                ShowNotify('Selecione o estabelecimento', 'Estabelecimento', 'danger');
                $("#Companies").focus();
                $("#Companies").select2('open');
                $("#Companies").closest('td').css('border', '1px solid red');
                return;
            }

            if ($("#Expense option:selected").val() === "0") {
                ShowNotify('Selecione uma despesa', 'Despesa', 'danger');
                $("#Expense").focus();
                $("#Expense").select2('open');
                $("#Expense").closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtValueExpense').val() === "") {
                ShowNotify('Informe o valor da despesa', 'Despesa', 'danger');
                $("#txtValueExpense").focus();
                $("#txtValueExpense").closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#vlrTotal').text().replace("R$", "") === 0) {
                ShowNotify('Nenhuma despesa foi informada', 'Despesa', 'danger');
                $("#Expense").closest('td').css('border', '1px solid red');
                return;
            }

            if ($("#DocumentType option:selected").val() === "0") {

                ShowNotify('Selecione um tipo de documento', 'Tipo Documento', 'danger');
                $("#DocumentType").focus();
                $("#DocumentType").closest('td').css('border', '1px solid red');
                return;

            }

            if ($("#Historic option:selected").val() === "0") {

                ShowNotify('Selecione um Histórico', 'Histórico', 'danger');
                $("#Historic").focus();
                $("#Historic").closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtDtDocument').val() === "") {
                ShowNotify('Informe a data do documento', 'Data Documento', 'danger');
                $('#txtDtDocument').focus();
                $('#txtDtDocument').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtNumDocument').val() === "") {

                ShowNotify('Informe a número do documento', 'Número Documento', 'danger');
                $('#txtNumDocument').focus();
                $('#txtNumDocument').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtPartialValue').val() === "") {

                ShowNotify('Informe o valor parcial', 'Valor Parcial', 'danger');
                $('#txtPartialValue').focus();
                $('#txtPartialValue').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtValueJuros').val() === "") {

                ShowNotify('Informe o valor do juros', 'Juros', 'danger');
                $('#txtValueJuros').focus();
                $('#txtValueJuros').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtValueMulta').val() === "") {

                ShowNotify('Informe o valor da Multa', 'Multa', 'danger');
                $('#txtValueMulta').focus();
                $('#txtValueMulta').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtValueDescontos').val() === "") {

                ShowNotify('Informe o valor do desconto', 'Desconto', 'danger');
                $('#txtValueDescontos').focus();
                $('#txtValueDescontos').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtValueTaxas').val() === "") {

                ShowNotify('Informe o valor das Taxas', 'Taxas', 'danger');
                $('#txtValueTaxas').focus();
                $('#txtValueTaxas').closest('td').css('border', '1px solid red');
                return;
            }

            if ($('#txtValueCartorio').val() === "") {

                ShowNotify('Informe o valor de despesas com cartórios', 'Despesas Cartório', 'danger');
                $('#txtValueCartorio').focus();
                $('#txtValueCartorio').closest('td').css('border', '1px solid red');
                return;
            }

            if (complementIsRequired) {

                if ($('#txtComplementDescription').val().length <= 0) {
                    ShowNotify('Informe o complemento do histórico', 'Taxas', 'danger');
                    $('#txtComplementDescription').closest('td').css('border', '1px solid red');
                    $('#txtComplementDescription').focus();
                    return;
                }
                else {
                    $('#txtComplementDescription').closest('td').css('border', 'none');
                }
            }

            if (parseFloat($('#txtValueExpense').val().replace(',', '.')) !== parseFloat($('#txtTotalValue').val().replace(',', '.'))) {
                ShowNotify('O valor do documento está diferente do valor informado no lançamento', 'Valores divergentes', 'danger');
                return;
            }
        }

        const fileUpload = base64;

        if (dtLaunch === "") {
            dtLaunch = $("#txtDtDocument").val();
        }
        else {

            if (dtLaunch !== $("#txtDtDocument").val()) {
                ShowNotify("Não é permitido realizar lançamentos com datas diferentes. Apenas os lançamentos do dia " + dtLaunch + " são permitidos", "Lançamento Inválido", "danger");
                $("#txtDtDocument").focus();
                return;
            }
        }

        {
            var newRow = "<tr>";
            newRow = newRow + "<td class='company'><input type='hidden' value='" + $("#Companies option:selected").val() + "' />" + $("#Companies option:selected").val() + "</td>";
            newRow = newRow + "<td class='expense'><input type='hidden' value='" + $("#Expense option:selected").val() + "' />" + $("#Expense option:selected").text() + "</td>";
            newRow = newRow + "<td class='documenttype'><input type='hidden' value='" + $("#DocumentType option:selected").val() + "' />" + $("#DocumentType option:selected").text() + "</td>";
            newRow = newRow + "<td class='documentdate'>" + $("#txtDtDocument").val() + "</td>";
            newRow = newRow + "<td class='documentdue'>" + $("#txtDtDue").val() + "</td>";
            newRow = newRow + "<td class='documentnumber'>" + $("#txtNumDocument").val() + "</td>";
            newRow = newRow + "<td class='documentvalue'>R$ " + parseFloat($("#txtPartialValue").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";
            newRow = newRow + "<td class='interest'>R$ " + parseFloat($("#txtValueJuros").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";
            newRow = newRow + "<td class='penalty'>R$ " + parseFloat($("#txtValueMulta").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";
            newRow = newRow + "<td class='discount'>R$ " + parseFloat($("#txtValueDescontos").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";
            newRow = newRow + "<td class='rate'>R$ " + parseFloat($("#txtValueTaxas").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";
            newRow = newRow + "<td class='register'>R$ " + parseFloat($("#txtValueCartorio").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";

            newRow = newRow + "<td class='newRowValueExpense'>" + parseFloat($("#txtTotalValue").val().replace(',', '.')).toFixed(2).replace('.', ',') + "</td>";
            newRow = newRow + "<td style='text-align:center;' class='newRow'><i class='fa fa-trash'/><input type='hidden' value='0' class='idexpensetemp'/></td>";
            newRow = newRow + "<td style='text-align:center;width:0px;' class='newRowHistoric'><input type='hidden' class='documentfile' value='" + fileUpload + "'/><input type='hidden' class='historicid' value='" + $("#Historic option:selected").val() + "' /><input type='hidden' class='historiccomplementvalue' value='" + $("#txtComplementDescription").val() + "'/><input type='hidden' class='historiccomplement' value='" + $("#Historic option:selected").text() + " - " + $("#txtComplementDescription").val() + "'/><i class='fa fa-search'/></td>";
            newRow = newRow + "<td style='text-align:center;' class='editRow' data-toggle='tooltip' data-placement='top' title='Editar Despesa'><i class='fa fa-edit'/></td>";
            newRow = newRow + "</tr>";

            $('#tblAllExpensers tbody').append(newRow);

            ShowNotify("Despesa incluída na lista", "Despesa Incluída", "success");

            var expenseJson = {

                "AccountPlanId": parseInt($("#Expense option:selected").val()),
                "AccountDescription": $("#Expense option:selected").text(),
                "ProtocolId": $('#ProtocolId').val(),
                "EstablishmentId": parseInt($("#Companies option:selected").val()),
                "EstablishmentDescription": $("#Companies option:selected").val(),
                "HistoricId": parseInt($("#Historic option:selected").val()),
                "HistoricDescription": $("#Historic option:selected").text(),
                "Complement": $("#txtComplementDescription").val(),
                "DocumentTypeId": parseInt($("#DocumentType option:selected").val()),
                "DocumentTypeDescription": $("#DocumentType option:selected").text(),
                "DocumentNumber": $("#txtNumDocument").val(),
                "DocumentDate": $("#txtDtDocument").val(),
                "DocumentDue": $("#txtDtDue").val(),
                "Value": parseFloat($("#txtPartialValue").val().replace(',', '.')),
                "Interest": parseFloat($("#txtValueJuros").val().replace(',', '.')),
                "Penalty": parseFloat($("#txtValueMulta").val().replace(',', '.')),
                "Discount": parseFloat($("#txtValueDescontos").val().replace(',', '.')),
                "Rate": parseFloat($("#txtValueTaxas").val().replace(',', '.')),
                "Registry": parseFloat($("#txtValueCartorio").val().replace(',', '.')),
                "TotalValue": parseFloat($("#txtTotalValue").val().replace(',', '.')),
                "DocumentPath": fileUpload,
                "CompanyId": parseInt($('#CompanyId').val())
            };

            $.ajax({

                url: "/Launch/SaveExpenseTemporary/",
                dataType: "json",
                type: "post",
                contentType: "application/json",
                processData: false,
                async: true,
                data: JSON.stringify(expenseJson),
                success: function (dt) {
                    //console.log(dt.message);
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    ShowNotify('Error: ' + errorThrown, 'Erro', 'danger');
                }
            });
        }

        var vlTtl = 0;

        $('.newRowValueExpense').each(function () {
            var ttl = $(this).text().replace(",", ".").replace("R$", "");
            vlTtl = parseFloat(vlTtl) + parseFloat(ttl);
        });

        $('#spanTotalExpense').text(vlTtl.toFixed(2).replace('.', ','));

        $('.newRowHistoric').click(function () {
            ShowMessage($(this).find(".historiccomplement").val());
        });

        $('.newRow').click(function () {

            vlTtl = 0;

            $(this).closest('tr').remove();

            $('#tblAllExpensers .newRowValueExpense').each(function () {
                var ttl = $(this).text().replace(",", ".").replace("R$", "");
                vlTtl = parseFloat(vlTtl) + parseFloat(ttl);
            });

            if ($('#tblAllExpensers .newRowValueExpense').length <= 0) {
                dtLaunch = "";
            }

            $('#spanTotalExpense').text("R$ " + vlTtl);

            ShowNotify("Despesa removida da lista", "Despesa Removida", "success");

            ValidateProtocol();

            $.get("/Launch/DeleteExpenseTemporary/" + $(this).find('.idexpensetemp').val(), function (data) { });


        });

        $('.editRow').click(function () {

            var rowEdit = $(this).closest('tr');

            var company = rowEdit.find('.company').text();
            $('#Companies').val(company).change();

            var expense = rowEdit.find('.expense').find("input[type='hidden']").val();
            $('#Expense').val(expense).change();

            var documenttype = rowEdit.find('.documenttype').find("input[type='hidden']").val();
            $('#DocumentType').val(documenttype).change();

            var historic = rowEdit.find('.newRowHistoric').find(".historicid").val();
            $('#Historic').val(historic).change();

            var historicComplement = rowEdit.find('.newRowHistoric').find(".historiccomplementvalue").val();
            $('#txtComplementDescription').val(historicComplement);

            var documentdate = rowEdit.find('.documentdate').text();
            $('#txtDtDocument').text(documentdate);

            var documentdue = rowEdit.find('.documentdue').text();
            $('#txtDtDue').val(documentdue);

            var documentnumber = rowEdit.find('.documentnumber').text();
            $('#txtNumDocument').val(documentnumber);

            var documentvalue = rowEdit.find('.documentvalue').text();
            subTotal = parseFloat(documentvalue.replace(',', '.').replace('R$', ''));

            $('#txtPartialValue').val(documentvalue);

            var interest = rowEdit.find('.interest').text();
            Interest = parseFloat(interest.replace(',', '.').replace('R$', ''));
            $('#txtValueJuros').val(interest);

            var penalty = rowEdit.find('.penalty').text();
            Penalty = parseFloat(penalty.replace(',', '.').replace('R$', ''));
            $('#txtValueMulta').val(penalty);

            var discount = rowEdit.find('.discount').text();
            Discount = parseFloat(discount.replace(',', '.').replace('R$', ''));
            $('#txtValueDescontos').val(discount);

            var rate = rowEdit.find('.rate').text();
            Rate = parseFloat(rate.replace(',', '.').replace('R$', ''));
            $('#txtValueTaxas').val(rate);

            var register = rowEdit.find('.register').text();
            Registry = parseFloat(register.replace(',', '.').replace('R$', ''));
            $('#txtValueCartorio').val(register);

            var newRowValueExpense = rowEdit.find('.newRowValueExpense').text().replace(',', '.');
            $('#txtTotalValue').val(newRowValueExpense);
            $('#txtValueExpense').val(newRowValueExpense);

            if (ValidateResult()) {

                $('#btnAddExpense').focus();
                $(this).closest('tr').remove();

                var vlTtlN = 0;

                $('#tblAllExpensers .newRowValueExpense').each(function () {
                    var ttl = $(this).text().replace(",", ".").replace("R$", "");
                    vlTtlN = parseFloat(vlTtlN) + parseFloat(ttl);
                });

                $('#spanTotalExpense').text(vlTtlN.toFixed(2).replace('.', ','));

                $.get("/Launch/DeleteExpenseTemporary/" + rowEdit.find('.idexpensetemp').val(), function (data) { });

                if ($('#tblAllExpensers .newRowValueExpense').length <= 0) {
                    dtLaunch = "";
                }
            }

        });

        ClearLaunch();

        $('#btnAddExpense').attr('disabled', 'disabled');

        ValidateProtocol();

        if (prorocolIsValid) {
            $('#btnSaveProtocol').focus();
        }
        else {
            $("#Companies").focus();
            $("#Companies").select2('open');
        }

    });

    function getBase64() {
        var file64 = "";
        var reader = new FileReader();
        var file = document.getElementById('documentFile').files[0];
        reader.readAsDataURL(file);
        reader.onload = function () {
            base64 = reader.result;
        };

        reader.onerror = function (error) {
            ShowNotify('Error: ' + error, 'Erro', 'danger');
        };
    }

    $('#documentFile').change(function () {
        var filePath = $(this).val();

        if (filePath !== "") {

            $('#bgUploadFile').addClass('bg-green');
            getBase64();
            $('#spanNameDocument').html(filePath);
            $('#spanNameDocument').addClass('bg-green');

            $('#Historic').focus();
            $('#Historic').select2('open');
        }

    });

    $('#btnLoadFile').click(function () {

        var file = document.getElementById("documentFile").files[0];
        getBase64(document.getElementById("documentFile").files[0]);

    });

    function ShowMessage(msg, elementfocus) {

        $('#formShow .modal-body').html(msg);
        $('#formShow').modal('show');
        $(elementfocus).focus();

    }

    function ShowNotify(msg, ttl, typemsg) {

        if (typemsg === "alert") {

            $.notify({
                title: ttl,
                message: msg
            }, {
                    type: 'pastel-warning',
                    delay: 5000,
                    template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                        '<span data-notify="title">{1}</span>' +
                        '<span data-notify="message">{2}</span>' +
                        '</div>',
                    showProgressbar: true
                });

        } else if (typemsg === "danger") {

            $.notify({
                title: ttl,
                message: msg
            }, {
                    type: 'pastel-danger',
                    delay: 5000,
                    template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                        '<span data-notify="title">{1}</span>' +
                        '<span data-notify="message">{2}</span>' +
                        '</div>',
                    showProgressbar: true
                });

        } else {

            $.notify({
                title: ttl,
                message: msg
            }, {
                    type: 'pastel-success',
                    delay: 5000,
                    template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                        '<span data-notify="title">{1}</span>' +
                        '<span data-notify="message">{2}</span>' +
                        '</div>',
                    showProgressbar: true
                });

        }

    }

    $(document).on('focus', '.select2.select2-container', function (e) {

        if (e.originalEvent && $(this).find(".select2-selection--single").length > 0) {
            $(this).siblings('.selectoption').select2('open');
        }

    });

    function ClearAllFields() {

        $('#tblAllExpensers tbody tr').each(function () {
            $(this).remove();
        });

        $('#spanTotalExpense').text("0,00");

        $('#btnSaveProtocol').attr('disabled', 'disabled');

        $("#PayingSource").closest('td').css('border', 'none');

        $("#PayingSource").val("0").change();

        $('#tblSource tbody tr').each(function () {
            $(this).remove();
        });

        $('#vlrTotal').text("R$ 0,00");
        $('#vlrTotal').closest('td').css('border', 'none');

        ClearLaunch();

        $('#txtValue').focus();
    }

    function ClearLaunch() {

        base64 = "";

        $('#spanNameDocument').text('');
        $("#Companies").closest('td').css('border', 'none');
        $("#Expense").closest('td').css('border', 'none');

        $("#DocumentType").closest('td').css('border', 'none');
        $("#Historic").closest('td').css('border', 'none');
        $('#txtComplement').closest('td').css('border', 'none');
        $('#txtComplementDescription').closest('td').css('border', 'none');
        $('#txtComplement').val('');

        $("#Companies").val("0").change();
        $("#Expense").val("0").change();
        $("#DocumentType").val("0").change();
        $("#Historic").val("0").change();
        $("#Historic").prop('selectedIndex', 0).change();
        $('#txtComplementDescription').val("").change();

        $('#txtNumDocument').val('');
        $('#txtValueExpense').val('');

        $('#txtNumDocument').closest('td').css('border', 'none');
        $('#txtPartialValue').val('0,00');
        $('#txtPartialValue').closest('td').css('border', 'none');

        $('#txtValueJuros').val('0,00');
        $('#txtValueJuros').closest('td').css('border', 'none');
        $('#txtValueMulta').val('0,00');
        $('#txtValueMulta').closest('td').css('border', 'none');
        $('#txtValueDescontos').val('0,00');
        $('#txtValueDescontos').closest('td').css('border', 'none');
        $('#txtValueTaxas').val('0,00');
        $('#txtValueTaxas').closest('td').css('border', 'none');
        $('#txtValueCartorio').val('0,00');
        $('#txtValueCartorio').closest('td').css('border', 'none');
        $('#txtTotalValue').val('0,00');
        $('#txtTotalValue').closest('td').css('border', 'none');

        Interest = 0;
        Penalty = 0;
        Rate = 0;
        Discount = 0;
        Registry = 0;
        subTotal = 0;;
    }

    $('#Historic').change(function () {

        if ($(this).val() === "0" || $(this).val() === null) {
            return;
        }

        $.ajax(
            {
                type: 'GET',
                url: '/Launch/GetHistoric/' + $(this).val(),
                dataType: 'json',
                cache: false,
                async: true,
                success: function (data) {

                    $('#txtComplement').val(data.complement);
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    ShowNotify('Error: ' + errorThrown, 'Erro', 'danger');
                }
            });

    });

    var timeIntervalProccessLot = 0;
    var countIntervalProccessLot = 0;

    function UpdateStatusProccess() {

        countIntervalProccessLot += 1;

        if (countIntervalProccessLot < 5) {
            $('#messageSaveLote').text("Processando informações do lote. Por favor aguarde... Tempo de processamento: " + countIntervalProccessLot + "s");
        } else {
            $('#messageSaveLote').text("Processando informações do lote. Enviando email para ESCONPI. Por favor aguarde... Tempo de processamento: " + countIntervalProccessLot + "s");
        }
    }

    $('#btnSaveLot').click(function () {

        var countRow = $('#tblAllProtocols>tbody>tr').length;
        if (countRow === 1) {
            if ($('#tblAllProtocols>tbody>tr>td').length === 1) {
                countRow = 0;
            }
        }

        var isSave = confirm(`O lote ${$('#txtNumeroLote').val()} possui ${countRow} lançamento(s). Deseja continuar?`);

        if (!isSave)
            return;

        $('.loader').css('display', 'block');
        $('#btnSaveLot').attr('disabled', 'disabled');

        timeIntervalProccessLot = setInterval(function () { UpdateStatusProccess(); }, 1000);

        $.ajax({
            type: 'GET',
            url: '/Launch/CloseLot/' + $('#txtNumeroLote').val(),
            dataType: 'json',
            cache: false,
            async: true,
            success: function (data) {

                $('.loader').css('display', 'none');
                $('#btnSaveLot').removeAttr('disabled');
                clearInterval(timeIntervalProccessLot);
                $('#messageSaveLote').text('');
                countIntervalProccessLot = 0;
                if (data.code === 200) {

                    ShowNotify(`Lote ${$('#txtNumeroLote').val()} fechado com SUCESSO`, "Lote Fechado", "success");
                    ShowNotify("Novo lote disponibilizado: " + data.lotid, "Novo Lote", "alert");

                    $('#txtNumeroLote').val(data.lotid);
                    $('#txtNumeroProtocol').val(data.protocolid);

                    $.ajax(
                        {
                            type: 'GET',
                            url: '/Launch/ProtocolsById/' + $('#txtNumeroLote').val(),
                            dataType: 'html',
                            cache: false,
                            async: true,
                            success: function (data) {
                                $('.loader').css('display', 'none');
                                $('#divLot').html(data);
                                $('#tblAllProtocols').DataTable({
                                    "paging": false,
                                    "searching": false,
                                    "ordering": false,
                                    "bLengthChange": false
                                });
                            }
                        });

                } else if (data.code === 204) {
                    ShowNotify(data.message, "Lote", "alert");
                }

            },
            error: function () {

                $('.loader').css('display', 'none');
                $('#btnSaveLot').removeAttr('disabled');
                clearInterval(timeIntervalProccessLot);
                $('#messageSaveLote').text('');
                countIntervalProccessLot = 0;
            }

        });
    });

    $('#btnSaveProtocol').click(function () {

        var paymentSource = "[";

        var idSource = 0;
        var value = parseFloat("0");
        var establishment = "";

        var expensers = "[";
        $('#tblAllExpensers tbody tr').each(function () {

            var fieldsdocdate = $(this).find(".documentdate").text().split('/');
            var fieldsdocdue = $(this).find(".documentdue").text().split('/');

            var docdate = fieldsdocdate[1] + "/" + fieldsdocdate[0] + "/" + fieldsdocdate[2];
            var docdue = fieldsdocdue[1] + "/" + fieldsdocdue[0] + "/" + fieldsdocdue[2];

            var row = `{"AccountPlanId":${$(this).find(".expense").find("input[type='hidden']").val()}, 
                            "ProtocolId" :${$("#ProtocolId").val()}, 
                            "EstablishmentId":${parseInt($(this).find(".company").find("input[type='hidden']").val())}, 
                            "HistoricId":${$(this).find(".newRowHistoric").find(".historicid").val()}, 
                            "DocumentTypeId":${$(this).find(".documenttype").find("input[type='hidden']").val()}, 
                            "DocumentNumber":"${$(this).find(".documentnumber").text()}", 
                            "DocumentDate":"${docdate}", 
                            "DocumentDue":"${docdue}", 
                            "Value":${$(this).find(".documentvalue").text().replace("R$", "").replace(",", ".")}, 
                            "Interest":${$(this).find(".interest").text().replace("R$", "").replace(",", ".")}, 
                            "Penalty":${$(this).find(".penalty").text().replace("R$", "").replace(",", ".")}, 
                            "Discount": ${$(this).find(".discount").text().replace("R$", "").replace(",", ".")}, 
                            "Rate":${$(this).find(".rate").text().replace("R$", "").replace(",", ".")}, 
                            "Registry":${$(this).find(".register").text().replace("R$", "").replace(",", ".")},
                            "DocumentPath":"${$(this).find(".newRowHistoric").find(".documentfile").val()}", 
                            "Complement":"${$(this).find(".newRowHistoric").find(".historiccomplement").val()}",
                            "TotalValue":${$(this).find(".newRowValueExpense").text().replace("R$", "").replace(",", ".")}},`;

            expensers = expensers + row;
        });

        expensers = expensers.substring(0, expensers.lastIndexOf(','));
        expensers = expensers + "]";

        $('#tblSource tbody tr').each(function () {

            var colNumber = 0;
            var tds = $(this).find('td').each(function () {

                if (colNumber === 0) {
                    idSource = $(this).text();
                } else if (colNumber === 2) {
                    value = $(this).text().replace("R$", "").replace(",", ".");
                } else if (colNumber === 3) {
                    establishment = $(this).text();
                }

                colNumber++;
            });

            paymentSource = paymentSource + "{\"AccountPlanId\":" + idSource + ", \"Value\":" + value + ", \"EstablishmentId\": 0 },";
        });

        paymentSource = paymentSource.substring(0, paymentSource.lastIndexOf(','));
        paymentSource = paymentSource + "]";

        var protocolModel = "{";

        protocolModel = protocolModel + "\"CompanyId\":" + $('#CompanyId').val();
        protocolModel = protocolModel + ", \"LoteId\":" + $('#txtNumeroLote').val();
        protocolModel = protocolModel + ", \"ProtocolId\":" + $('#ProtocolId').val();
        protocolModel = protocolModel + ",\"PayingSource\":" + paymentSource;
        protocolModel = protocolModel + ",\"Expense\":" + expensers;

        protocolModel = protocolModel + "}";

        try {

            $('.loader').css('display', 'block');

            $.ajax({
                url: '/Launch/InsertProtocol/',
                dataType: 'json',
                type: 'post',
                contentType: 'application/json',
                data: protocolModel,
                processData: false,
                success: function (data, textStatus, jQxhr) {

                    $('.loader').css('display', 'none');
                    LoadLots();

                    ClearAllFields();

                    ShowNotify(`Protocolo ${$('#txtNumeroProtocol').val()} incluído com SUCESSO!`, 'Sucesso', 'success');
                    $('#txtNumeroProtocol').val(data.message);
                    ShowNotify('Novo protocolo disponível: ' + data.message, 'Novo Protocolo', 'alert');

                    dtLaunch = "";
                },
                error: function (jqXhr, textStatus, errorThrown) {
                    $('.loader').css('display', 'none');
                    ShowNotify('Error: ' + errorThrown, 'Erro', 'danger');
                }
            });

        } catch (e) {
            ShowNotify('Error: ' + e, 'Erro', 'danger');
        }

    });

    $('#btnAdd').click(function () {


        if ($("#txtValue").val() === "") {
            ShowNotify('Informe o valor da despesa', 'Valor Despesa', 'danger');
            $("#txtValue").focus();
            $("#txtValue").closest('td').css('border', '1px solid red');
            return;
        }

        if ($("#PayingSource option:selected").val() === "0") {
            ShowNotify('Selecione uma fonte pagadora', 'Fonte Pagadora', 'danger');
            $("#PayingSource").focus();
            $("#PayingSource").select2('open');
            $("#PayingSource").closest('td').css('border', '1px solid red');
            return;
        }

        $("#txtValue").closest('td').css('border', 'none');
        $("#PayingSource").closest('td').css('border', 'none');
        $("#vlrTotal").closest('td').css('border', 'none');

        var newRow = "<tr>";
        newRow = newRow + "<td>" + $("#PayingSource option:selected").val() + "</td>";
        newRow = newRow + "<td>" + $("#PayingSource option:selected").text() + "</td>";
        newRow = newRow + "<td class='newRowValue'>" + FormatMoney($("#txtValue").val().replace(',', '.')) + "</td>";
        newRow = newRow + "<td style='text-align:center;' class='newRow'><input type='hidden' value='0' class='idpayingtemp'/><i class='fa fa-trash'/></td>";
        newRow = newRow + "</tr>";

        $('#tblSource tbody').append(newRow);

        var today = ConvertDateToJson(null);

        var jsonObject = {

            "Id": 0,
            "Created": today,
            "Updated": today,
            "CompanyId": parseInt($('#CompanyId').val()),
            "AccountCode": parseInt($("#PayingSource option:selected").val()),
            "AccountDescription": $("#PayingSource option:selected").text(),
            "Value": parseFloat($("#txtValue").val().replace(',', '.'))
        };

        var vlTtl = 0;

        $('.newRowValue').each(function () {

            var ttl = $(this).text().replace(",", ".").replace("R$", "");
            vlTtl = parseFloat(vlTtl) + parseFloat(ttl);

            $("#txtValue").val("");
            $("#txtValue").focus();

        });

        $('#vlrTotal').text(FormatMoney(vlTtl));

        $('.newRow').click(function () {

            vlTtl = 0;

            $(this).closest('tr').remove();

            $('#tblSource .newRowValue').each(function () {
                var ttl = $(this).text().replace(",", ".").replace("R$", "");
                vlTtl = parseFloat(vlTtl) + parseFloat(ttl);
            });

            $('#vlrTotal').text(FormatMoney(vlTtl));

            ShowNotify('Fonte Pagadora removida da lista', 'Fonte Pagadora', 'success');

            $.get("/Launch/DeletePayingSourceTemporary/" + $(this).find('.idpayingtemp').val(), function (data) { });

        });

        ValidateProtocol();

        $("#PayingSource").val("0").change();

        $("#txtValue").focus();

        ShowNotify('Fonte Pagadora incluída na lista', 'Fonte Pagadora', 'success');

        $.ajax({

            url: "/Launch/SavePayingSourceTemporary/",
            dataType: "json",
            type: "post",
            contentType: "application/json",
            processData: false,
            async: true,
            data: JSON.stringify(jsonObject),
            success: function (dt) {
                //console.log(dt.message);
            },
            error: function (jqXhr, textStatus, errorThrown) {
                ShowNotify('Error: ' + errorThrown, 'Erro', 'danger');
            }
        });
    });

    $('#txtPartialValue').on('input', function () {

        if ($('#txtPartialValue').val() === "") {
            return;
        }

        subTotal = parseFloat($('#txtPartialValue').val().replace(",", "."));

        var sumTtl = Interest + Penalty + Rate - Discount + Registry + subTotal;
        ValidateResult();
    });

    $('#txtValueJuros').on('input', function () {

        if ($('#txtValueJuros').val() === "") {
            return;
        }

        if ($('#txtPartialValue').val() === "") {
            $('#txtPartialValue').focus();
            ShowNotify("Informe o subtotal do documento", "Subtotal", "danger");
            return;
        }

        Interest = parseFloat($('#txtValueJuros').val().replace(",", "."));

        ValidateResult();
    });

    $('#txtValueJuros').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtValueMulta').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtValueMulta').on('input', function () {
        if ($('#txtValueMulta').val() === "") {
            return;
        }

        if ($('#txtPartialValue').val() === "") {
            $('#txtPartialValue').focus();
            ShowNotify("Informe o subtotal do documento", "Subtotal", "danger");
            return;
        }

        Penalty = parseFloat($('#txtValueMulta').val().replace(",", "."));

        ValidateResult();
    });

    $('#txtValueMulta').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtValueDescontos').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtValueDescontos').on('input', function () {
        if ($('#txtValueDescontos').val() === "") {
            return;
        }

        if ($('#txtPartialValue').val() === "") {
            $('#txtPartialValue').focus();
            ShowNotify("Informe o subtotal do documento", "Subtotal", "danger");
            return;
        }

        Discount = parseFloat($('#txtValueDescontos').val().replace(",", "."));

        ValidateResult();
    });

    $('#txtValueDescontos').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtValueTaxas').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtValueTaxas').on('input', function () {

        if ($('#txtValueTaxas').val() === "") {
            return;
        }

        if ($('#txtPartialValue').val() === "") {
            $('#txtPartialValue').focus();
            ShowNotify("Informe o subtotal do documento", "Subtotal", "danger");
            return;
        }

        Rate = parseFloat($('#txtValueTaxas').val().replace(",", "."));
        ValidateResult();
    });

    $('#txtValueTaxas').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtValueCartorio').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtValueCartorio').on('input', function () {
        if ($('#txtValueCartorio').val() === "") {
            return;
        }

        if ($('#txtPartialValue').val() === "") {
            $('#txtPartialValue').focus();
            ShowNotify("Informe o subtotal do documento", "Subtotal", "danger");
            return;
        }

        Registry = parseFloat($('#txtValueCartorio').val().replace(",", "."));

        ValidateResult();
    });

    $('#txtValueCartorio').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#btnAddExpense').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#Companies').keypress(function (e) {

        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#Companies').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtValue').keypress(function (e) {

        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#PayingSource').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtNumDocument').keypress(function (e) {

        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtPartialValue').focus();
        }
        //e.preventDefault(e);
        //return false;
    });

    $('#txtPartialValue').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtValueJuros').focus();
        }
        e.preventDefault(e);
        return false;
    });

    $('#txtComplementDescription').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#txtDtDocument').focus();
        }
    });

    $('#txtValueExpense').keypress(function (e) {
        var _key = (e.keyCode ? e.keyCode : e.which);
        if (_key === 13) {
            $('#DocumentType').focus();
        }
    });

    $('#txtValueExpense').on('input', function () {
        if ($('#txtValueCartorio').val() === "" || $('#txtPartialValue').val() === "") {
            return;
        }

        ValidateResult();
    });

    function ValidateResult() {

        var sumTtl = Interest + Penalty + Rate - Discount + Registry + subTotal;

        $('#txtTotalValue').val(sumTtl.toFixed(2).replace('.', ','));

        if (parseFloat(sumTtl.toFixed(2)) !== parseFloat($('#txtValueExpense').val().replace("R$", "").replace(",", "."))) {
            $('#txtTotalValue').closest('td').css('border', '1px solid red');
            $('#btnAddExpense').attr('disabled', 'disabled');
        }
        else {
            $('#txtTotalValue').closest('td').css('border', '1px solid green');
            $('#btnAddExpense').removeAttr('disabled', 'disabled');
            return true;
        }
    }

    function ValidateProtocol() {

        var valuePaying = parseFloat($('#vlrTotal').text().replace("R$", "").replace(",", ".")).toFixed(2);
        var valueTotal = parseFloat($('#spanTotalExpense').text().replace("R$", "").replace(",", ".")).toFixed(2);

        if (valueTotal === 0 | valuePaying === 0) {
            prorocolIsValid = false;
            $('#btnSaveProtocol').removeAttr('disabled', 'disabled');
            $('#spanMessageProtocol').css('display', 'none');
        }

        if (valuePaying === valueTotal) {
            $('#btnSaveProtocol').removeAttr('disabled', 'disabled');
            $('#spanMessageProtocol').css('display', 'none');
            ShowNotify("Protolo liberado para envio", "Protocolo Liberado", "success");
            prorocolIsValid = true;
        }
        else {
            prorocolIsValid = false;
            $('#btnSaveProtocol').attr('disabled', 'disabled');
            $('#spanMessageProtocol').css('display', 'block');
        }
    }

    function FormatMoney(value) {

        if (isNaN(value)) {
            return value;
        }
        var newValue = parseFloat(value).toFixed(2);
        return "R$ " + newValue.replace('.', ',');

    }

    function LoadDataTemp() {

        $.ajax({
            url: "/Launch/GetExpenseTemporary/" + $('#CompanyId').val(),
            type: "get",
            async: true,
            contentType: "application/json",
            success: function (data) {

                for (var i = 0; i < data.length; i++) {

                    var newRow = "<tr>";
                    newRow = newRow + "<td class='company'><input type='hidden' value='" + data[i].establishmentId + "' />" + data[i].establishmentDescription + "</td>";
                    newRow = newRow + "<td class='expense'><input type='hidden' value='" + data[i].accountPlanId + "' />" + data[i].accountDescription + "</td>";
                    newRow = newRow + "<td class='documenttype'><input type='hidden' value='" + data[i].documentTypeId + "' />" + data[i].documentTypeDescription + "</td>";
                    newRow = newRow + "<td class='documentdate'>" + data[i].documentDate + "</td>";
                    newRow = newRow + "<td class='documentdue'>" + data[i].documentDue + "</td>";
                    newRow = newRow + "<td class='documentnumber'>" + data[i].documentNumber + "</td>";
                    newRow = newRow + "<td class='documentvalue'>R$ " + data[i].value.toFixed(2).toString().replace('.', ',') + "</td>";
                    newRow = newRow + "<td class='interest'>R$ " + data[i].interest.toFixed(2).toString().replace('.', ',') + "</td>";
                    newRow = newRow + "<td class='penalty'>R$ " + data[i].penalty.toFixed(2).toString().replace('.', ',') + "</td>";
                    newRow = newRow + "<td class='discount'>R$ " + data[i].discount.toFixed(2).toString().replace('.', ',') + "</td>";
                    newRow = newRow + "<td class='rate'>R$ " + data[i].rate.toFixed(2).toString().replace('.', ',') + "</td>";
                    newRow = newRow + "<td class='register'>R$ " + data[i].registry.toFixed(2).toString().replace('.', ',') + "</td>";

                    newRow = newRow + "<td class='newRowValueExpense'>" + data[i].totalValue.toFixed(2).toString().replace('.', ',') + "</td>";
                    newRow = newRow + "<td style='text-align:center;' class='newRow'><input type='hidden' value='" + data[i].id + "' class='idexpensetemp'/><i class='fa fa-trash' data-toggle='tooltip' data-placement='top' title='Excluir Despesa'/></td>";
                    newRow = newRow + "<td style='text-align:center;width:0px;' class='newRowHistoric'><input type='hidden' class='documentfile' value='" + data[i].documentPath + "'/><input type='hidden' class='historicid' value='" + data[i].historicId + "' /><input type='hidden' class='historiccomplementvalue' value='" + data[i].complement + "'/><input type='hidden' class='historiccomplement' value='" + data[i].historicDescription + " - " + data[i].complement + "'/><i class='fa fa-search' data-toggle='tooltip' data-placement='top' title='Visualizar histórico da despesa'/></td>";
                    newRow = newRow + "<td style='text-align:center;' class='editRow'><i class='fa fa-edit' data-toggle='tooltip' data-placement='top' title='Editar Despesa'/></td>";
                    newRow = newRow + "</tr>";

                    var vlTtl = 0;
                    dtLaunch = data[i].documentDate;

                    $('#tblAllExpensers tbody').append(newRow);
                    $('[data-toggle="tooltip"]').tooltip();

                    $('.newRowValueExpense').each(function () {
                        var ttl = $(this).text().replace(",", ".").replace("R$", "");
                        vlTtl = parseFloat(vlTtl) + parseFloat(ttl);
                    });

                    $('#spanTotalExpense').text(vlTtl.toFixed(2).replace('.', ','));

                    $('.newRowHistoric').click(function () {
                        ShowMessage($(this).find(".historiccomplement").val());
                    });

                    $('.newRow').click(function () {

                        vlTtl = 0;

                        $(this).closest('tr').remove();

                        $('#tblAllExpensers .newRowValueExpense').each(function () {
                            var ttl = $(this).text().replace(",", ".").replace("R$", "");
                            vlTtl = parseFloat(vlTtl) + parseFloat(ttl);
                        });

                        if ($('#tblAllExpensers .newRowValueExpense').length <= 0) {
                            dtLaunch = "";
                        }

                        $('#spanTotalExpense').text("R$ " + vlTtl);

                        ShowNotify("Despesa removida da lista", "Despesa Removida", "success");

                        ValidateProtocol();

                        $.get("/Launch/DeleteExpenseTemporary/" + $(this).find('.idexpensetemp').val(), function (data) { });

                    });

                    $('.editRow').click(function () {

                        var rowEdit = $(this).closest('tr');

                        var company = rowEdit.find('.company').text();
                        $('#Companies').val(company).change();

                        var expense = rowEdit.find('.expense').find("input[type='hidden']").val();
                        $('#Expense').val(expense).change();

                        var documenttype = rowEdit.find('.documenttype').find("input[type='hidden']").val();
                        $('#DocumentType').val(documenttype).change();

                        var historic = rowEdit.find('.newRowHistoric').find(".historicid").val();
                        $('#Historic').val(historic).change();

                        var historicComplement = rowEdit.find('.newRowHistoric').find(".historiccomplementvalue").val();
                        $('#txtComplementDescription').val(historicComplement);

                        var documentdate = rowEdit.find('.documentdate').text();
                        $('#txtDtDocument').text(documentdate);

                        var documentdue = rowEdit.find('.documentdue').text();
                        $('#txtDtDue').val(documentdue);

                        var documentnumber = rowEdit.find('.documentnumber').text();
                        $('#txtNumDocument').val(documentnumber);

                        var documentvalue = rowEdit.find('.documentvalue').text();
                        subTotal = parseFloat(documentvalue.replace(',', '.').replace('R$', ''));

                        $('#txtPartialValue').val(documentvalue);

                        var interest = rowEdit.find('.interest').text();
                        Interest = parseFloat(interest.replace(',', '.').replace('R$', ''));
                        $('#txtValueJuros').val(interest);

                        var penalty = rowEdit.find('.penalty').text();
                        Penalty = parseFloat(penalty.replace(',', '.').replace('R$', ''));
                        $('#txtValueMulta').val(penalty);

                        var discount = rowEdit.find('.discount').text();
                        Discount = parseFloat(discount.replace(',', '.').replace('R$', ''));
                        $('#txtValueDescontos').val(discount);

                        var rate = rowEdit.find('.rate').text();
                        Rate = parseFloat(rate.replace(',', '.').replace('R$', ''));
                        $('#txtValueTaxas').val(rate);

                        var register = rowEdit.find('.register').text();
                        Registry = parseFloat(register.replace(',', '.').replace('R$', ''));
                        $('#txtValueCartorio').val(register);

                        var newRowValueExpense = rowEdit.find('.newRowValueExpense').text().replace(',', '.');
                        $('#txtTotalValue').val(newRowValueExpense);
                        $('#txtValueExpense').val(newRowValueExpense);

                        if (ValidateResult()) {
                            $('#btnAddExpense').focus();
                            $(this).closest('tr').remove();

                            var vlTtlN = 0;

                            $('#tblAllExpensers .newRowValueExpense').each(function () {
                                var ttl = $(this).text().replace(",", ".").replace("R$", "");
                                vlTtlN = parseFloat(vlTtlN) + parseFloat(ttl);
                            });

                            $('#spanTotalExpense').text(vlTtlN.toFixed(2).replace('.', ','));

                            $.get("/Launch/DeleteExpenseTemporary/" + rowEdit.find('.idexpensetemp').val(), function (data) { });

                            if ($('#tblAllExpensers .newRowValueExpense').length <= 0) {
                                dtLaunch = "";
                            }
                        }

                    });

                    ClearLaunch();

                    $('#btnAddExpense').attr('disabled', 'disabled');

                    ValidateProtocol();

                    if (prorocolIsValid) {
                        $('#btnSaveProtocol').focus();
                    }
                    else {
                        $("#Companies").focus();
                        $("#Companies").select2('open');
                    }
                }

            },
            error: function (jqXhr, textStatus, errorThrown) {

            }
        });

        $.ajax({
            url: "/Launch/GetPayingSourceTemporary/" + $('#CompanyId').val(),
            type: "get",
            async: true,
            contentType: "application/json",
            success: function (data) {

                for (var i = 0; i < data.length; i++) {

                    var newRow = "<tr>";
                    newRow = newRow + "<td>" + data[i].accountCode + "</td>";
                    newRow = newRow + "<td>" + data[i].accountDescription + "</td>";
                    newRow = newRow + "<td class='newRowValue'>" + FormatMoney(data[i].value) + "</td>";
                    newRow = newRow + "<td style='text-align:center;' class='newRow'><input type='hidden' value='" + data[i].id + "' class='idpayingtemp'/><i class='fa fa-trash'/></td>";
                    newRow = newRow + "</tr>";

                    $('#tblSource tbody').append(newRow);

                }

                var vlTtl = 0;

                $('.newRowValue').each(function () {

                    var ttl = $(this).text().replace(",", ".").replace("R$", "");
                    vlTtl = parseFloat(vlTtl) + parseFloat(ttl);

                    $("#txtValue").val("");
                    $("#txtValue").focus();

                });

                $('#vlrTotal').text(FormatMoney(vlTtl));

                $('.newRow').click(function () {

                    vlTtl = 0;

                    $(this).closest('tr').remove();

                    $('#tblSource .newRowValue').each(function () {
                        var ttl = $(this).text().replace(",", ".").replace("R$", "");
                        vlTtl = parseFloat(vlTtl) + parseFloat(ttl);
                    });

                    $('#vlrTotal').text(FormatMoney(vlTtl));

                    ShowNotify('Fonte Pagadora removida da lista', 'Fonte Pagadora', 'success');

                    $.get("/Launch/DeletePayingSourceTemporary/" + $(this).find('.idpayingtemp').val(), function (data) { });

                });

            },
            error: function (jqXhr, textStatus, errorThrown) {
                console.log(errorThrown);
            }
        });

    }

    $('.datepicker').inputmask("99/99/9999", { "placeholder": "dd/mm/aaaa" });

    $('.datepicker').focusout(function () {
        if (!$(this).inputmask("isComplete")) {
            ShowNotify("Data inválida [" + $(this).val() + "]", "Formato Data", "danger");
            $(this).css('border', '1px solid red').focus();
        } else {
            $(this).css('border', '1px solid #afafaf');
        }
    });

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

    var loadTemp = setTimeout(function () { LoadDataTemp(); }, 500);

});