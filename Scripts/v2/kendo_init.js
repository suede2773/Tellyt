$(document).ready(function () {
    onHandleKendoInit();
    setTimeout(onHandleKendoInit, 500);
});

function onHandleKendoInit() {
    handleKendoButtons();
}

function handleKendoButtons() {
    $(".tlyt_kendo_button").each(function (index) {
        if (!$(this).hasClass("tlyt_kendo_initialized")) {
            $(this).kendoButton();
            $(this).addClass("tlyt_kendo_initialized");
        }
    });
}

function handleDropdownListInputs() {
    $(".tlyt_dropdownlist").each(function (index) {
        if (!$(this).hasClass("tlyt_kendo_initialized")) {
            //$(this).kendoDropDownList();
            $(this).addClass("tlyt_kendo_initialized");
        }
    });
}