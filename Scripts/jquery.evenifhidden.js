/*!
 * jQuery evenIfHidden
 * https://github.com/piotrooo/jquery-evenifhidden
 *
 * Copyright 2013 Piotr Olaszewski
 * Released under the MIT license
 *
 * !!!IMPORTANT!!!
 * Not editing styles in callback function (will disappear)
 */
(function ($) {
    $.fn.evenIfHidden = function (callback) {
        return this.each(function () {
            var _this = $(this);
            var currentStyles = [];

            var hiddenElements = _this.parents().andSelf().filter(':hidden');

            if (!hiddenElements.length) {
                callback(_this);
                return;
            }

            hiddenElements.each(function () {
                var style = $(this).attr('style');
                style = typeof style == 'undefined' ? '' : style;
                currentStyles.push(style);
                $(this).attr('style', style + ' display: block !important;');
            });

            hiddenElements.eq(0).css('left', -10000);

            callback(_this);

            hiddenElements.each(function () {
                $(this).attr('style', currentStyles.shift());
            });
        });
    };
}(jQuery));
