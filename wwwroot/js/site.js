$(function () {
    'use strict'

    $('input.secure-input').each(function () {
        $(this).attr({
            'aria-autocomplete': 'none',
            'autofill': 'off',
            'autocomplete': 'off',
            'autocorrect': 'off',
            'autocapitalize': 'off',
            'spellcheck': 'false'
        });
    });

});
