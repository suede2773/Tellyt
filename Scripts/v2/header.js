function onHandleNavBarHighlight(name) {
    $(".tlt_header_nav_item").each(function (i, obj) {
        var _name = $(this).attr("data-tlt-name");
        if (name == _name) {
            $(this).addClass("active");
        } else {
            $(this).removeClass("active");
        }
    });
}