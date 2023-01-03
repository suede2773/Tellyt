$(document).ready(function () {
    onHandleKendoInit();
    setTimeout(onHandleKendoInit, 500);
});

function onHandleKendoInit() {
    handleKendoButtons();
    handleKendoPanelBars();
}

function handleKendoButtons() {
    $(".tlt-kendo-button").each(function (index) {
        if (!$(this).hasClass("tlt-kendo-initialized")) {
            if ($(this).hasClass("tlt-kendo-primary-button")) {
                $(this).kendoButton({
                    themeColor: "primary"
                });
            } else {
                if ($(this).hasClass("tlt-kendo-disabled-primary-button")) {
                    $(this).kendoButton({
                        themeColor: "primary",
                        enable: false
                    });
                } else {
                    if ($(this).hasClass("tlt-kendo-disabled-button")) {
                        $(this).kendoButton({
                            enable: false
                        });
                    } else {
                        $(this).kendoButton();
                    }
                }
            }
            $(this).addClass("tlt-kendo-initialized");
        }
    });
}

function handleKendoPanelBars() {
    $(".tlt-kendo-panel-bar").each(function (index) {
        if (!$(this).hasClass("tlt-kendo-initialized")) {
            $(this).kendoPanelBar();
            $(this).addClass("tlt-kendo-initialized");
        }
    });
}