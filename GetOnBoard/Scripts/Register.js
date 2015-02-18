
$(document).ready(function () {
    $("#btnRegister").click(function () {
        var account = {
            UserName: $("#txtUserName").val(),
            Password: $("#txtPassword").val(),
            Email: $("#txtEmail").val(),
            FirstName: $("#txtFname").val(),
            LastName: $("#txtLname").val()
        };
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(account),
            url: 'get/AccountService.svc/RegisterUser',
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
                    SaveSession();
                    window.location.href = "/Login.aspx";
                } else {
                    showMessage("Registration failed!");
                }
            },
            error: function () {
                showMessage('Unknown error please report.');
            }
        });
    });
});

