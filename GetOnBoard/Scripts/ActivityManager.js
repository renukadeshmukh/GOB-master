var ActivityManager = function () {
    this.Players = [];
    this.UnreadMessages = [];
    this.RenderMessages = function (from, messages) {
        if (messages && messages.length > 0) {
            var myChats = [];
            gameManager.Chats[from] = gameManager.Chats[from] ? gameManager.Chats[from] : [];
            for (var i = 0; i < messages.length; i++) {
                var index = findIndex(gameManager.Chats[from], "Id", messages[i].Id);
                if (index === -1) {
                    myChats.push(messages[i]);
                    if (messages[i].From !== Session.UserName) {
                        setUnreadMessage(from, 1, true);
                    }
                } else {
                    gameManager.Chats[from][index] = messages[i];
                }
            }
            for (var j = 0; j < gameManager.Chats[from].length; j++) {
                myChats.push(gameManager.Chats[from][j]);
            }
            gameManager.Chats[from] = myChats;
        }
        if ($("#boardContainer #msgBox").length > 0) {
            actManager.SendMessageStatus(from);
            $("#msgBox").setTemplate($("#msgTemplate").val());
            $("#msgBox").processTemplate({ Chats: gameManager.Chats[from], Me: Session.UserName });
        }
        actManager.ShowUnreadMessages(from);
    };

    this.UpdateChatMessages = function (from, messageIds) {
        for (var i = 0; i < messageIds.length; i++) {
            var index = findIndex(gameManager.Chats[from], "Id", messageIds[i]);
            if (index !== -1) {
                gameManager.Chats[from][index].IsSeen = true;
            }
        }
        actManager.RenderMessages(from);
    };

    this.SendMessageStatus = function(from) {
        if (gameManager.Chats[from]) {
            var ids = [];
            var chats = gameManager.Chats[from];
            for (var i = 0; i < chats.length; i++) {
                if (chats[i].From === from && chats[i].IsSeen === false) {
                    ids.push(chats[i].Id);
                    gameManager.Chats[from][i].IsSeen = true;
                }
            }
            setUnreadMessage(from, 0, false);
            gameClient.SendMessageStatus(from, ids);
        }
    };
    
    function setUnreadMessage(from, count, incr)
    {
        var index = findIndex(actManager.UnreadMessages, "UserName", from);
        if (index !== -1) {
            if (incr) {
                actManager.UnreadMessages[index].Count = actManager.UnreadMessages[index].Count + count;
            } else {
                actManager.UnreadMessages[index].Count = count;
            }
        } else {
            actManager.UnreadMessages.push({ UserName: from, Count: count });
        }
    }
    
    this.ShowUnreadMessages = function(from) {
        var unread = 0;
        if (gameManager.SelGameId) {
            var gameStatus = gameManager.GameStatus[gameManager.SelGameId];
            if (!gameStatus)
                return;
            var index = findIndex(actManager.UnreadMessages, "UserName", gameStatus.Player2.UserName);
            if (index !== -1) {
                unread = actManager.UnreadMessages[index].Count;
            } else {
                actManager.UnreadMessages.push({ UserName: gameStatus.Player2.UserName, Count: 0 });
            }
        }
        if (unread > 0) {
            $("#tabLnkChats").addClass("unread");
            $("#tabLnkChats").html("Chats(" + unread + ")");
        } else {
            $("#tabLnkChats").removeClass("unread");
            $("#tabLnkChats").html("Chats(" + unread + ")");
        }
    };

    this.UpdateUserStatus = function (userName, isOnline) {
        var player = { UserName: userName, IsOnline: isOnline };
        var index = findIndex(actManager.Players, "UserName", userName);
        if (index === -1) {
            actManager.Players.push(player);
        } else {
            actManager.Players[index] = player;
        }
        $("[id*=ply_]").each(function () {
            var parts = this.id.split('_');
            if (parts.length == 3 && parts[2] === userName) {
                $("#" + this.id).removeAttr("class");
                if (isOnline) {
                    $("#" + this.id).addClass("online");
                } else {
                    $("#" + this.id).addClass("offline");
                }
            }
        });
    };

    this.GetAndRenderPlayerStatus = function () {
        var players = [];
        if (gameManager.MyGames) {
            for (var i = 0; i < gameManager.MyGames.length; i++) {
                var opponant = gameManager.MyGames[i].Opponant;
                if (opponant != '') {
                    var index = findIndex(actManager.Players, "UserName", opponant);
                    if (index === -1) {
                        if (players.indexOf(opponant) === -1) {
                            players.push(opponant);
                        }
                    } else {
                        var id = "ply_" + gameManager.MyGames[i].Id +"_"+ opponant;
                        $("#" + id).removeAttr("class");
                        if (actManager.Players[index].IsOnline) {
                            $("#" + id).addClass("online");
                        } else {
                            $("#" + id).addClass("offline");
                        }
                    }
                }
            }
            gameClient.GetPlayerStatus(players);
        }
    };
    
    this.GetUnreadMessages = function () {
        EnableLoading = true;
        $.ajax({
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ SessionId: Session.SessionId }),
            url: 'get/AccountService.svc/GetUnReadMessages',
            dataType: "json",
            cache: false,
            success: function (data) {
                if (data != null && data.IsSucess) {
                    actManager.UnreadMessages = data.UnReadMessages;
                    actManager.ShowUnreadMessages();
                } else {
                    showMessage(data && data.ErrorMessage ? "GetUnreadMessages: " + data.ErrorMessage : "Faild to get your Unread Messages! Unknown error.");
                }
            },
            error: function () {
                showMessage('Unknown error please report.');
            }
        });
    };
};