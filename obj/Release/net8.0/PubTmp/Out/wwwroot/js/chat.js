$(document).ready(function () {
    
    var connection = new signalR.HubConnectionBuilder().withUrl("/MessageHub").build();

    var viewModel = new AppViewModel();
    ko.applyBindings(viewModel);
    connection.start().then(function () {
        console.log('SignalR Started...')
        viewModel.loadFriends();
        viewModel.isLoading(false);

    }).catch(function (err) {
        return console.error(err);
    });


    connection.on("joinMessage", function (user) {

        // Update friends list
        ko.utils.arrayForEach(viewModel.friends(), function (friend) {
            if (friend.id() === user.id) {
                // Update the friend's isOnline
                friend.isOnline(true);

                // If needed, you can also use friend.notifySubscribers to force update bindings
                //viewModel.friends.valueHasMutated();
            }
        });
    });
    connection.on("leaveMessage", function (user) {

        // Update friends list
        ko.utils.arrayForEach(viewModel.friends(), function (friend) {
            if (friend.id() === user.id) {
                // Update the friend's isOnline
                friend.isOnline(false);
            }
        });
    });
    connection.on("newMessage", function (message) {
        var isMine = message.senderId == userId ? true : false;
        console.log(message.sender.name)

        if (isMine == true) {
            // send by self
            viewModel.chatMessages.push(new ChatMessage(message.id, message.content, message.timestamp, message.sender.name, message.receiver.name, message.senderId, isMine));
            viewModel.message("");
            $(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 300);
        }
        else {
            console.log(viewModel.selectedFriend())
            // receicer is not select friend
            if (viewModel.selectedFriend() === undefined || viewModel.selectedFriend().id() != message.senderId) {
                console.log("unread")
                ko.utils.arrayForEach(viewModel.friends(), function (friend) {
                    if (friend.id() === message.senderId) {
                        //update this friend send new message unread
                        friend.hasUnreadMessage(true);
                    }
                });
            }
            // receicer is selecting friend who send message
            else if (viewModel.selectedFriend().id() == message.senderId) {
                viewModel.chatMessages.push(new ChatMessage(message.id, message.content, message.timestamp, message.sender.name, message.receiver.name, message.senderId, isMine));
                viewModel.message("");
                $(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 300);

            }
        }
        
    });

    connection.on("deleteMessage", function (messageId) {
        var temp;
        ko.utils.arrayForEach(viewModel.chatMessages(), function (message) {
            if (message.id() == messageId)
                temp = message;
        });
        viewModel.chatMessages.remove(temp)
    });
    connection.on("deleteFriend", function (senderId) {
        console.log(senderId)
        var temp;
        ko.utils.arrayForEach(viewModel.friends(), function (friend) {
            if (friend.id() == senderId)
                temp = friend;
        });
        viewModel.friends.remove(temp)
        
        if (viewModel.selectedFriend().id() == senderId) {
            console.log(viewModel.selectedFriend().id() == senderId)
            viewModel.chatMessages([])
            viewModel.selectedFriend(null)
            viewModel.userInfo(null)
        }
    });

    connection.on("onError", function (message) {
        viewModel.serverInfoMessage(message);
        $("#errorAlert").removeClass("d-none").show().delay(5000).fadeOut(500);
    });

    function AppViewModel() {
        var self = this;

        self.message = ko.observable("");
        self.friends = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]);
        self.serverInfoMessage = ko.observable("");
        self.myProfile = ko.observable();
        self.isLoading = ko.observable(true);
        self.selectedFriend = ko.observable();
        self.userInfo = ko.observable();


        //self.showAvatar = ko.computed(function () {
        //    return self.isLoading() == false && self.myProfile().avatar() != null;
        //});


        self.loadFriends = function () {
            fetch('/api/Message/GetFriends?id=' + userId)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok ' + response.statusText);
                    }
                    return response.json();
                })
                .then(data => {
                    self.friends([]);
                    connection.invoke("GetUsers").then(result => {
                        for (var i = 0; i < data.length; i++) {
                            var isOnline = false
                            result.forEach(u => {
                                if (u.id == data[i].id) isOnline = true
                            })
                            self.friends.push(new User(data[i].id, data[i].name, data[i].email, data[i].description, isOnline, false));
                        }
                    })
                    console.log(self.friends())
                    //self.selectedFriend(data[0])
                })
                .catch(error => {
                    console.error('There has been a problem with your fetch operation:', error);
                });
        }
        self.getMessage = function () {
            fetch('/api/Message/GetMessages?senderId=' + userId + '&receiverId=' + self.selectedFriend().id())
                .then(response => {
                    if (!response.ok) {
                        self.chatMessages([]);
                        throw new Error('Network response was not ok ' + response.statusText);
                    }
                    return response.json();
                })
                .then(data => {
                    //console.log(data)
                    self.chatMessages([]);
                    for (var i = 0; i < data.length; i++) {
                        var isMine = data[i].senderId == userId ? true : false
                        self.chatMessages.push(new ChatMessage(data[i].id, data[i].content, data[i].timestamp, data[i].senderName, data[i].receiverName, data[i].senderId, isMine));
                    }
                    $(".messages-container").animate({ scrollTop: $(".messages-container")[0].scrollHeight }, 300);
                    //console.log(self.chatMessages()[0].content());
                })
                .catch(error => {
                    console.error('There has been a problem with your fetch operation:', error);
                });
        }

        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {
                self.sendNewMessage();
            }
            return true;
        }
        self.filter = ko.observable("");
        self.filteredFriend = ko.computed(function () {
            if (!self.filter()) {
                return self.friends().sort((a, b) => {
                    return a.hasUnreadMessage() ? -1 : 1; // Sort Unread message
                });
            } else {
                return ko.utils.arrayFilter(self.friends(), function (friend) {
                    var name = friend.name().toLowerCase();
                    return name.includes(self.filter().toLowerCase());
                }).sort((a, b) => {
                    return a.hasUnreadMessage() ? -1 : 1; // Sort Unread message
                });
            }
        });
        self.selectFriend = function (user) {
            user.hasUnreadMessage(false)
            self.selectedFriend(user);
            self.userInfo('');
            self.getMessage();
        }
        self.showUserInfo = function (friend) {
            console.log(self.userInfo())
            if (self.userInfo() == undefined || self.userInfo() == '')
                self.userInfo(friend);
            else self.userInfo(null);
        };
        self.sendNewMessage = function () {
            if (self.message().length > 0) {
                connection.invoke("SendMessage", userId, userName, ko.unwrap(self.selectedFriend().id), self.message())
            }
        }


        
        self.deleteMessage = function () {
            var messageId = $("#itemToDelete").val();

            fetch('/api/Message/DeleteMessage?id=' + messageId, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: messageId })
            }).then(response => response.json())
                .then(data => {
                    if (data == 1) {
                        //invoke to tell other message was delete
                        connection.invoke("DeleteMessage", messageId, self.selectedFriend().id())
                        // delete by self
                        var temp;
                        ko.utils.arrayForEach(self.chatMessages(), function (message) {
                            if (message.id() == messageId)
                                temp = message;
                        });
                        self.chatMessages.remove(temp)
                    }
                });
        }

        

        self.messageDeleted = function (id) {
            var temp;
            ko.utils.arrayForEach(self.chatMessages(), function (message) {
                if (message.id() == id)
                    temp = message;
            });
            self.chatMessages.remove(temp);
        }
        self.deleteFriend = function () {
            var friendId = $("#friendToDelete").val();

            fetch('/api/Message/DeleteFriend?userId=' + userId + '&friendId=' + friendId, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId: userId, friendId: friendId })
            }).then(response => response.json())
                .then(data => {
                    if (data == 1) {
                        console.log(self.selectedFriend().id())
                        //invoke to tell other friend was delete
                        connection.invoke("DeleteFriend", userId, self.selectedFriend().id())
                        // delete by self
                        var temp;
                        ko.utils.arrayForEach(self.friends(), function (friend) {
                            if (friend.id() == friendId)
                                temp = friend;
                        });
                        self.selectedFriend(null)
                        self.userInfo(null)
                        self.chatMessages([])
                        self.friends.remove(temp)
                        
                    }
                });
        }
        self.uploadFiles = function () {
            var form = document.getElementById("uploadForm");
            $.ajax({
                type: "POST",
                url: '/api/Message/Upload',
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function (message) {
                    $("#UploadedFile").val("");
                    var receiverId = form.querySelector('input[name="receiverId"]').value;
                    message.sender.image = null;
                    message.receiver.image = null

                    connection.invoke("Upload", message, receiverId)
                },
                error: function (error) {
                    alert('Error: ' + error.responseText);
                }
            });
        }
    }
    
    function User(id, name, email, description,isOnline, hasUnreadMessage) {
        var self = this;
        self.id = ko.observable(id);
        self.name = ko.observable(name);
        self.email = ko.observable(email);
        self.description = ko.observable(description);
        self.isOnline = ko.observable(isOnline);
        self.hasUnreadMessage = ko.observable(hasUnreadMessage);

    }

    function ChatMessage(id, content, timestamp, fromUser, toUser, senderId, isMine) {
        var self = this;
        self.id = ko.observable(id);
        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.timestampRelative = ko.pureComputed(function () {
            // Get diff
            var date = new Date(self.timestamp());
            var now = new Date();
            var diff = Math.round((date.getTime() - now.getTime()) / (1000 * 3600 * 24));

            // Format date
            var { dateOnly, timeOnly } = formatDate(date);

            // Generate relative datetime
            if (diff == 0)
                return `Today, ${timeOnly}`;
            if (diff == -1)
                return `Yestrday, ${timeOnly}`;

            return `${dateOnly}`;
        });
        self.timestampFull = ko.pureComputed(function () {
            var date = new Date(self.timestamp());
            var { fullDateTime } = formatDate(date);
            return fullDateTime;
        });
        self.fromUser = ko.observable(fromUser);
        self.toUser = ko.observable(toUser);
        self.senderId = ko.observable(senderId);
        self.isMine = ko.observable(isMine);
    }


    function formatDate(date) {
        // Get fields
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        var day = date.getDate();
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var d = hours >= 12 ? "PM" : "AM";

        // Correction
        if (hours > 12)
            hours = hours % 12;

        if (minutes < 10)
            minutes = "0" + minutes;

        // Result
        var dateOnly = `${day}/${month}/${year}`;
        var timeOnly = `${hours}:${minutes} ${d}`;
        var fullDateTime = `${dateOnly} ${timeOnly}`;

        return { dateOnly, timeOnly, fullDateTime };
    }

    
});
