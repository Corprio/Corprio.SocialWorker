$(function () {
    //redirect to Edge if still using IE
    if (/MSIE \d|Trident.*rv:/.test(navigator.userAgent)) {
        window.location = 'microsoft-edge:' + window.location;
        setTimeout(function () {
            window.location = 'https://go.microsoft.com/fwlink/?linkid=2135547';
        }, 1);
    }
    // Sidebar toggle behavior
    $('#sidebar-toggle').on('click', function (e) {
        e.preventDefault();

        $(this).children("i:first-child").toggleClass("fa-chevron-right");
        $('#sidebar, #main').toggleClass('is-collapsed');
    });
});

