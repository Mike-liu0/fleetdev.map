/* ============================================================================
 * File: notification.html
 * Author: Deyou Zou
 * Date: 5 Jan 2021
 * Project: Sycamore - ALLINK
 ==============================================================================
 */



var key = "jQTHqBgLA0aNSNoVUbU0NQ";
var domain = "https://dev.allinks.com.au/api/";
var API_OnRequestSendFCMPushNotification = "OnRequestSendFCMPushNotification.aspx";
var API_GetAllUsers = "GetAllUsers.aspx";
var testGuidList = ["50135ec2-8275-492c-a3d1-0e1449f1ca0f", "729c57e7-f9fa-4e4c-bf8d-25cf1d58a033", "cbf2eaad-4b99-438c-aa1e-68d54bd24591", "75a27c43-f73c-4525-aea8-e6c95b8364b0", "13a3780-216d-46bb-b10f-fd9141ab3586"];
var isTestMode = true;
var stop = false;
var status = "idle";

document.getElementById("sendingbutton").addEventListener("click", send);
document.getElementById("locked").addEventListener("click", function () {
    isTestMode = false;
    document.getElementById("locked").style.display = "none";
    document.getElementById("unlocked").style.display = "block";
    console.log("Test mode off.")
})
document.getElementById("unlocked").addEventListener("click", function () {
    isTestMode = true;
    document.getElementById("locked").style.display = "block";
    document.getElementById("unlocked").style.display = "none";
    console.log("Test mode on.")
})


function send() {
    stop = false;
    status = "sending"
    document.getElementById("errormessage").style.display = "none";
    document.getElementById("buttontext").innerHTML = "Stop";
    document.getElementById("sendingbutton").removeEventListener("click", send);
    document.getElementById("sendingbutton").addEventListener("click", function () { stop = true;});
    document.getElementById("status").innerHTML = "Prepairing...";
    document.getElementById("status").style.display = "block";

    var message = document.getElementById("inputbox").value;
    var checkResult = checkString(message);


    if (checkResult == "Valid") {

        // Get user list
        var package = new FormData();
        package.append("key", key);
        package.append("filterName", "status");
        package.append("filterValue", "dev");


        var request = new XMLHttpRequest();
        request.open("POST", domain + API_GetAllUsers, true);
        request.send(package);

        request.onreadystatechange = function () {
            if (request.readyState == 4 & request.status == 200) {
                var response = JSON.parse(this.responseText);
                var userList = response.package;

                // Test mode
                if (isTestMode) {
                    userList = toTestModeUserList(userList, testGuidList)
                }

                // Maximum 4 seconds for sending a message to 1 user
                var waitingTime = userList.length * 4000;

                sendMessageToUser(userList, message);

                // If not finished in a reasonable time
                setTimeout(function () {
                    if (status == "sending") {
                        status = "idle";
                        document.getElementById("status").style.display = "none";
                        document.getElementById("errormessage").innerHTML = "Something went wrong. Please try again.";
                        document.getElementById("errormessage").style.display = "block";
                        document.getElementById("buttontext").innerHTML = "Send";
                        document.getElementById("sendingbutton").removeEventListener("click", function () { stop = true; });
                        document.getElementById("sendingbutton").addEventListener("click", send);
                    }
                },waitingTime);
            }
        }
    } else {
        status = "idle"
        document.getElementById("errormessage").innerHTML = checkResult;
        document.getElementById("errormessage").style.display = "block";
        document.getElementById("buttontext").innerHTML = "Send";
        document.getElementById("sendingbutton").removeEventListener("click", function () { stop = true; });
        document.getElementById("sendingbutton").addEventListener("click", send);
    }
}


function toTestModeUserList(userList, testGuidList) {
    var finalUserList = [];
    for (const user of userList) {
        if (testGuidList.includes(user.guid)) {
            finalUserList.push(user);
        }
    }
    return finalUserList
}


function sendMessageToUser(userList, message) {
    if (!stop) {
        if (userList.length != 0) {
            var user = userList.pop();
            var usertoken = user.fcmToken;
        }
        else {
            status = "idle";
            document.getElementById("inputarea").style.display = "none";
            document.getElementById("succeeded").style.display = "block";
            document.getElementById("backbutton").addEventListener("click", back);
            return;
        }

        document.getElementById("status").innerHTML = "Sending to " + user.guid;
        let package = new FormData();
        package.append("key", key);
        package.append("message", message);
        package.append("target", usertoken);

        let request = new XMLHttpRequest();
        request.open("POST", domain + API_OnRequestSendFCMPushNotification, true);
        request.send(package);

        request.onreadystatechange = setTimeout(function () {
            if (request.readyState == 4 && request.status == 200) {
                sendMessageToUser(userList, message)
            }
        }, 2000)
    } else {
        status = "idle";
        document.getElementById("status").style.display = "none";
        document.getElementById("errormessage").innerHTML = "Stopped";
        document.getElementById("errormessage").style.display = "block";
        document.getElementById("buttontext").innerHTML = "Send";
        document.getElementById("sendingbutton").removeEventListener("click", function () { stop = true; });
        document.getElementById("sendingbutton").addEventListener("click", send);
        return;
    }
}


function defaultmessage(num) {
    document.getElementById("inputbox").value = document.getElementById("message"+num).innerHTML;
}


function back() {
    document.getElementById("backbutton").removeEventListener("click", back);
    document.getElementById("succeeded").style.display = "none";
    document.getElementById("inputbox").value = "";
    document.getElementById("buttontext").innerHTML = "Send";
    document.getElementById("inputarea").style.display = "block";
    document.getElementById("sendingbutton").removeEventListener("click", function () { stop = true; });
    document.getElementById("sendingbutton").addEventListener("click", send);
    document.getElementById("status").innerHTML = "";
    document.getElementById("status").style.display = "none";
}



function checkString(str) {
    if (str == "") {
        return "Empty Message";
    }
    if (str.length > 100) {
        return "Total length should be less than 100 characters";
    }
    if (str.includes(",") || str.includes("，")) {
        return "The message should not contain ','";
    }
    return "Valid";
}