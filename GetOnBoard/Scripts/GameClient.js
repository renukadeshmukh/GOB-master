var GameClient = function () {
    this.GameHub = $.connection.gameHub;
    this.IsConnected = false;
    this.Connect = function() {
        $.connection.hub.qs = { "userName": Session.UserName };
        gameClient.GameHub.client.RenderMessage = function (from, message) {
            actManager.RenderMessages(from, [message]);
        };
        gameClient.GameHub.client.RenderMessageStatus = function (from, ids) {
            actManager.UpdateChatMessages(from, ids);
        };
        gameClient.GameHub.client.UpdateUserStatus = function (userName, isOnline) {
            actManager.UpdateUserStatus(userName, isOnline);
        };
        gameClient.GameHub.client.SetPlayerStatus = function (playerStatus) {
            if (playerStatus) {
                for (var i = 0; i < playerStatus.length; i++) {
                    actManager.UpdateUserStatus(playerStatus[i].UserName, playerStatus[i].IsOnline);
                }
            }
        };
        gameClient.GameHub.client.ItsMyTurn = function (from, gameId) {
            gameManager.MM.PollGameMoves();
        };
        $.connection.hub.start().done(function () {
            gameClient.IsConnected = true;
            actManager.GetAndRenderPlayerStatus();
        });
    };
    
    this.SendMessage = function () {
        var message = $("#txtMsg").val();
        var gameStatus = gameManager.GameStatus[gameManager.SelGameId];
        if (message && gameStatus) {
            gameClient.GameHub.server.sendMessage(gameStatus.Player2.UserName, message);
            $("#txtMsg").val('');
            $("#txtMsg").focus();
        }
    };

    this.SendMessageStatus = function(to, ids) {
        if (to && ids && ids.length > 0) {
            gameClient.GameHub.server.sendMessageStatus(to, ids);
        }
    };

    this.GetPlayerStatus = function (players) {
        if (players && players.length > 0) {
            gameClient.GameHub.server.getPlayerStatus(players);
        }
    };

    this.NotifyTurn = function(to, gameId) {
        if (to && gameId) {
            gameClient.GameHub.server.notifyTurn(to, gameId);
        }
    };
};