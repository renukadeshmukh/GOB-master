$(document).ready(function () {
    $("body").keypress(function (event) {
        if (event.which == 13) {
            event.preventDefault();
            $("#btnLogin").trigger("click");
        }
    });
    $("#btnLogin").click(function () {
        var account = {
            UserName: $("#txtUserName").val(),
            Password: $("#txtPassword").val(),
        };
        EnableLoading = true;
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(account),
            url: 'get/AccountService.svc/Login',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    Session.Id = data.Account.Id;
                    Session.UserName = data.Account.UserName;
                    Session.SessionId = data.SessionId;
                    Session.FirstName = data.Account.FirstName;
                    Session.LastName = data.Account.LastName;
                    Session.Email = data.Account.Email;
                    Session.Level = data.Account.Level;
                    Session.TotalPoints = data.Account.Points;
                    Session.MaxGameLimit = data.Account.MaxGameLimit;
                    SaveSession();
                    window.location.href = "/Home.aspx";
                } else {
                    showMessage(data && data.ErrorMessage ? data.ErrorMessage : "Login failed! Unknown error." );
                }
            },
            error: function () {
                showMessage('Unknown error please report.');
            }
        });
    });
});