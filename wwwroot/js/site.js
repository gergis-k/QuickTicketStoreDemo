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


    $('#session-deletion-form').on('submit', function (event) {
        var confirmDelete = confirm("Are you sure you want to destroy this session?");
        if (!confirmDelete) {
            event.preventDefault(); // Prevent form submission if user cancels
        }
    });

});
