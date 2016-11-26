$(function () {
    var $elem = $('#scrollUpDown');

    $(window).bind('scrollstart', function () {
        $('#buttonScrollUp,#buttonScrollDown').stop().animate({ 'opacity': '0.2' });
    });
    $(window).bind('scrollstop', function () {
        $('#buttonScrollUp,#buttonScrollDown').stop().animate({ 'opacity': '1' });
    });

    $('#buttonScrollDown').click(
    function (e) {
        $('html, body').animate({ scrollTop: $('body').height() }, 800);
    }
    );
    $('#buttonScrollUp').click(
    function (e) {
        $('html, body').animate({ scrollTop: '0px' }, 800);
    }
    );
});
