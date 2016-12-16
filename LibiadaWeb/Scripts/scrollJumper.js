$(function () {
    var scrolledUp = false;
    var scrolledDown = false;
    var savedPosition = 0;

    $('#buttonScrollDown').click(
    function (e) {
        if (scrolledDown) {
            $('html, body').animate({ scrollTop: savedPosition }, 800);
            scrolledDown = false;
            scrolledUp = false;
        } else {
            savedPosition = window.pageYOffset || document.documentElement.scrollTop;
            $('html, body').animate({ scrollTop: $('body').height() }, 800);
            scrolledDown = true;
        }
        
    }
    );
    $('#buttonScrollUp').click(
    function (e) {
        if (scrolledUp) {
            $('html, body').animate({ scrollTop: savedPosition }, 800);
            scrolledDown = false;
            scrolledUp = false;
        } else {
            savedPosition = window.pageYOffset || document.documentElement.scrollTop;
            $('html, body').animate({ scrollTop: '0px' }, 800);
            scrolledUp = true;
        }
        
    }
    );
});
