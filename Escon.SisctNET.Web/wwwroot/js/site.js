// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ConvertToCurrencyReal(data) {
    return `R$ ${data.toFixed(2).toString().replace(',', '*').replace('.', ',').replace('*', '.')}`;
}