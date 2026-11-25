/* Validação de CPF/CNPJ no navegador (par do atributo [Documento] do servidor) */
(function ($) {
    if (!$ || !$.validator || !$.validator.unobtrusive) return;

    function soDigitos(v) { return (v || '').replace(/\D/g, ''); }

    function repetidos(d) { return /^(\d)\1+$/.test(d); }

    function cpfValido(d) {
        if (d.length !== 11 || repetidos(d)) return false;
        var soma = 0, i, resto;
        for (i = 0; i < 9; i++) soma += parseInt(d.charAt(i), 10) * (10 - i);
        resto = (soma * 10) % 11; if (resto === 10) resto = 0;
        if (resto !== parseInt(d.charAt(9), 10)) return false;
        soma = 0;
        for (i = 0; i < 10; i++) soma += parseInt(d.charAt(i), 10) * (11 - i);
        resto = (soma * 10) % 11; if (resto === 10) resto = 0;
        return resto === parseInt(d.charAt(10), 10);
    }

    function cnpjValido(d) {
        if (d.length !== 14 || repetidos(d)) return false;
        var p1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var p2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var soma = 0, i, resto, dv1, dv2;
        for (i = 0; i < 12; i++) soma += parseInt(d.charAt(i), 10) * p1[i];
        resto = soma % 11; dv1 = resto < 2 ? 0 : 11 - resto;
        if (dv1 !== parseInt(d.charAt(12), 10)) return false;
        soma = 0;
        for (i = 0; i < 13; i++) soma += parseInt(d.charAt(i), 10) * p2[i];
        resto = soma % 11; dv2 = resto < 2 ? 0 : 11 - resto;
        return dv2 === parseInt(d.charAt(13), 10);
    }

    $.validator.addMethod('documento', function (value, element, tipo) {
        if (!value) return true;
        var d = soDigitos(value);
        if (tipo === 'cpf') return cpfValido(d);
        if (tipo === 'cnpj') return cnpjValido(d);
        if (d.length === 11) return cpfValido(d);
        if (d.length === 14) return cnpjValido(d);
        return false;
    });

    $.validator.unobtrusive.adapters.add('documento', ['tipo'], function (options) {
        options.rules['documento'] = options.params.tipo;
        options.messages['documento'] = options.message;
    });
})(window.jQuery);
