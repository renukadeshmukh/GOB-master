var Session = {
    Id: '',
    SessionId: '',
    UserName: '',
    FirstName: '',
    LastName: '',
    Email: '',
    Level: 0,
    TotalPoints: 0,
    MaxGameLimit: 1
};

var EnableLoading = false;

$(document).ready(function() {
    LoadSession();
    $("#logOut").click(function() {
        Logout();
    });
    if (window.location.href.indexOf("HowToPlay.aspx") == -1) {
        if (Session.SessionId && Session.UserName) {
            ValidateSession();
        } else {
            if (window.location.href.indexOf("Login.aspx") == -1 && window.location.href.indexOf("Register.aspx") == -1) {
                window.location.href = "/Login.aspx";
                return;
            }
        }
    }

    $('#divLoadingMain')
    .hide() // hide it initially
    .ajaxStart(function () {
        if (EnableLoading) {
            $(this).show();
            $(this).dialog({
                resizable: false,
                minHeight: 50,
                closeOnEscape: false,
                modal: true
            });
            $(".ui-dialog-titlebar").hide();
            $(".ui-widget-content").css("border", 0);
        }
    })
    .ajaxStop(function () {
        $(this).dialog('close');
        EnableLoading = false;
    });
});

function showMessage(message, type) {
    if (type == undefined)
        type = 'error_message';
    $("#divMessage").addClass(type);
    $("#divMessage").html(message);
    $("#divMessage").show();
    setTimeout(function () { $("#divMessage").hide(); $("#divMessage").removeClass(type); }, 5000);
    $('html, body').animate({ scrollTop: 0 }, 'slow');
}

function ValidateSession() {
    $.ajax({
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({SessionId: Session.SessionId}),
        url: 'get/AccountService.svc/ValidateSession',
        dataType: "json",
        cache: false,
        success: function (data) {
            if (data == null || !data.IsSucess) {
                Session = {};
                SaveSession();
                window.location.href = "/Login.aspx";
                showMessage(data && data.ErrorMessage ? "ValidateSession: " + data.ErrorMessage : "Session validation failed! Unknown error.");
            }
        },
        error: function () {
            showMessage('Unknown error please report.');
            window.location.href = "/Login.aspx";
        }
    });
}

function Logout() {
    $.ajax({
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ SessionId: Session.SessionId }),
        url: 'get/AccountService.svc/Logout',
        dataType: "json",
        cache: false,
        success: function (data) {
            if (data != null && data.IsSucess) {
                Session = {};
                SaveSession();
                window.location.href = "/Login.aspx";
            }
        },
        error: function () {
        }
    });
}

function SaveSession() {
    document.cookie = "Session=" + JSON.stringify(Session);
}

function LoadSession() {
    var cookieStr = GetCookie("Session");

    if (cookieStr != undefined && cookieStr != '') {
        var object = JSON.parse(cookieStr);

        if (object != null)
            Session = object;
    }
}

function GetCookie(c_name) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == c_name) {
            return unescape(y);
        }
    }
    return null;
}

function findIndex(arr, key, value) {
    if (!arr || !key)
        return -1;
    for (var i = 0; i < arr.length; i++) {
        if (arr[i][key] === value)
            return i;
    }
    return -1;
}

var RandArr = function (array, noOfTiles) {
    var retArr = [];
    for (var i = 0; i < 6; i++) {
        var randNum = Math.floor((Math.random() * array.length) + 1) - 1;
        retArr.push(array[randNum]);
    }
    return retArr;
};

