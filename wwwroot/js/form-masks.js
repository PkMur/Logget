// Apply input masks and normalize values before submit
(function () {
    function applyMasks() {
        // CPF mask: 000.000.000-00 but we will strip non-digits before submit
        try {
            $("input[name='CPF']").inputmask({ mask: "999.999.999-99", placeholder: "_", showMaskOnHover: false });
        } catch (e) { /* fail silently if library missing */ }

        // CNH: 11 digits (no formatting, but we allow grouped input)
        try {
            $("input[name='NumeroCnh']").inputmask({ mask: "99999999999", placeholder: "_", showMaskOnHover: false });
        } catch (e) { }
        
        // Destinatário Documento (CPF or CNPJ) - accept either mask
        try {
            $("input[name='DestinatarioDocumento']").inputmask({
                mask: ["999.999.999-99", "99.999.999/9999-99"],
                keepStatic: true,
                placeholder: "_",
                showMaskOnHover: false
            });
        } catch (e) { }

        // Endereço número and QuantidadeVolumes should accept digits only
        try {
            $("input[name='EnderecoNumero'], input[name='QuantidadeVolumes']").inputmask({ alias: "integer", rightAlign: false });
        } catch (e) { }

        // Peso: decimal with 2 decimals
        try {
            $("input[name='Peso']").inputmask('decimal', { digits: 2, groupSeparator: ',', radixPoint: '.', autoGroup: false, rightAlign: false });
        } catch (e) { }
    }

    function normalizeOnSubmit() {
        $(document).on('submit', 'form', function () {
            var $form = $(this);
            $form.find("input[name='CPF']").each(function () {
                var v = $(this).val() || "";
                $(this).val(v.replace(/\D/g, ''));
            });
            $form.find("input[name='NumeroCnh']").each(function () {
                var v = $(this).val() || "";
                $(this).val(v.replace(/\D/g, ''));
            });
            $form.find("input[name='DestinatarioDocumento']").each(function () {
                var v = $(this).val() || "";
                $(this).val(v.replace(/\D/g, ''));
            });
            $form.find("input[name='EnderecoNumero']").each(function () {
                var v = $(this).val() || "";
                $(this).val(v.replace(/\D/g, ''));
            });
            $form.find("input[name='QuantidadeVolumes']").each(function () {
                var v = $(this).val() || "0";
                $(this).val(v.replace(/\D/g, ''));
            });
            $form.find("input[name='Peso']").each(function () {
                var v = $(this).val() || "";
                // normalize decimal separator to dot and remove non-numeric except dot
                v = v.replace(',', '.');
                v = v.replace(/[^0-9\.]/g, '');
                $(this).val(v);
            });
            return true;
        });
    }

    function validationFeedback() {
        // On validation error show Bootstrap invalid styles (works with unobtrusive validation)
        $(document).on('blur change input', 'input,select,textarea', function () {
            var $el = $(this);
            // defer to jQuery Validate if available
            var validator = $el.closest('form').data('validator');
            if (validator) {
                validator.element($el);
            }
            // toggle is-invalid/is-valid classes
            if ($el.hasClass('input-validation-error') || $el.next('.field-validation-error').length) {
                $el.addClass('is-invalid').removeClass('is-valid');
            } else if ($el.val()) {
                $el.addClass('is-valid').removeClass('is-invalid');
            } else {
                $el.removeClass('is-valid is-invalid');
            }
        });
    }

    function cpfValidateRaw(digits) {
        if (!digits) return false;
        if (digits.length !== 11) return false;
        if ((new Set(digits.split(''))).size === 1) return false;
        try {
            var nums = digits.split('').map(function (c) { return parseInt(c, 10); });
            var sum = 0;
            for (var i = 0; i < 9; i++) sum += nums[i] * (10 - i);
            var rem = sum % 11;
            var dv1 = rem < 2 ? 0 : 11 - rem;
            if (nums[9] !== dv1) return false;
            sum = 0;
            for (var j = 0; j < 10; j++) sum += nums[j] * (11 - j);
            rem = sum % 11;
            var dv2 = rem < 2 ? 0 : 11 - rem;
            if (nums[10] !== dv2) return false;
            return true;
        } catch (e) { return false; }
    }

    function registerCpfClientValidator() {
        if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
            // method
            jQuery.validator.addMethod('cpf', function (value, element) {
                if (!value) return true; // required handles emptiness
                var digits = value.replace(/\D/g, '');
                return cpfValidateRaw(digits);
            }, 'CPF inválido.');

            // adapter to map data-val-cpf attribute to rule
            jQuery.validator.unobtrusive.adapters.addBool('cpf');
        }
    }

    $(function () {
        applyMasks();
        normalizeOnSubmit();
        validationFeedback();
        registerCpfClientValidator();
    });
})();
