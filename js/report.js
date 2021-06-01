/* ============================================================================
 * File: post.html
 * Author: Deyou Zou
 * Date: 7 Jan 2021
 * Project: Sycamore - ALLINK
 ==============================================================================
 */

var key = "jQTHqBgLA0aNSNoVUbU0NQ";
var domain = "https://dev.allinks.com.au/api/";
var API_GetAllEarthworks = "GetAllEarthworks.aspx";
var API_GetUserByGuid = "GetUserByGuid.aspx";
var API_GetAllReports = "GetAllReports.aspx";
var API_UpdateReport = "UpdateReport.aspx";
var API_UpdateEarthwork = "UpdateEarthwork.aspx";
var EarthworkImageDir = "resource/post/";
var UserIconDir = "resource/user/";
var epochMicrotimeDiff = Math.abs(new Date(0, 0, 1).setFullYear(1));

onload("Unprocessed");
document.getElementById("unprocessed").addEventListener("click", function () {onload("Unprocessed")});
document.getElementById("ignored").addEventListener("click", function () {onload("Ignored") });
document.getElementById("postDeleted").addEventListener("click", function () { onload("Post Deleted") });

function onload(processResult) {
    // Clear the list
    document.getElementById("reportList").innerHTML = ""

    // Change the title
    document.getElementById("status").innerHTML = processResult + " Reports";

    var package = new FormData();
    package.append("key", key);
    package.append("filterName", "processResult");
    package.append("filterValue", processResult);

    var request = new XMLHttpRequest();
    request.open("POST", domain + API_GetAllReports, true);
    request.send(package);

    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = JSON.parse(this.responseText).package;
            response.sort(function (a, b) { return b.updated - a.updated });

            // Generate report previews
            var i = 0;
            while (i < response.length) {

                // Build up the structure
                let newDiv = document.createElement("div");
                newDiv.id = "report" + i;
                newDiv.classList.add("reportItem");

                let newReportTime = document.createElement("div");
                newReportTime.id = "preReportTime" + i;
                newReportTime.classList.add("reportPreDetails");
                newReportTime.classList.add("reportTime");
                newDiv.appendChild(newReportTime);

                let newUserName = document.createElement("div");
                newUserName.id = "preUserName" + i;
                newUserName.classList.add("reportPreDetails");
                newDiv.appendChild(newUserName);

                let newPostGuid = document.createElement("div");
                newPostGuid.id = "prePostGuid" + i;
                newPostGuid.classList.add("reportPreDetails");
                newDiv.appendChild(newPostGuid);


                // Update post guid
                let thisReport = response[i];
                newPostGuid.innerHTML = "Post guid: " + thisReport.attachedPost;

                // Convert and update time
                let convertedTime = new Date((thisReport.created / 10000) - epochMicrotimeDiff);
                newReportTime.innerHTML = reorderDateTime(convertedTime.toString());

                // Request for user data
                let userRequest = new XMLHttpRequest();
                let package = new FormData();
                package.append("key", key);
                package.append("guid", thisReport.attachedUser);
                userRequest.open("POST", domain + API_GetUserByGuid, true);
                userRequest.send(package);

                userRequest.onreadystatechange = function () {
                    if (userRequest.readyState == 4 & userRequest.status == 200) {
                        let userResponse = JSON.parse(userRequest.responseText);
                        let userInfo = userResponse.package;

                        newUserName.innerHTML = "Report user: " + userInfo.fullName;
                    }
                }
                // Update onclick
                newDiv.onclick = function () {
                    updateReportViewer(newDiv.id, thisReport.guid);
                };

                // Add to HTML
                document.getElementById("reportList").appendChild(newDiv);
                i++;
            }
        }
    }
}


function adjustHeight() {
    var popUpWindowH = document.getElementById("reportViewer").offsetHeight;
    var buttonGroupH = document.getElementById("buttonGroup").offsetHeight;
    var userInfoH = document.getElementById("userInfo").offsetHeight;
    document.getElementById("details").style.height = (popUpWindowH - buttonGroupH - userInfoH - 20) + "px";
}


function updateReportViewer(viewingId, reportGuid) {

    // Update buttons
    document.getElementById("viewPostButton").onclick = function () {
        window.open("admin_post_viewer.html?guid="+document.getElementById("postGuid").innerHTML, "_self");
    }
    document.getElementById("ignoreReportButton").onclick = function () {
        deleteReport(viewingId, reportGuid);
    }

    var package = new FormData();
    package.append("key", key);
    package.append("filterName", "guid");
    package.append("filterValue", reportGuid);

    var request = new XMLHttpRequest();
    request.open("POST", domain + API_GetAllReports, true);
    request.send(package);

    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = JSON.parse(this.responseText);
            var message = response.package;
            var thisReport = message[0];

            // Update report details
            document.getElementById("postGuid").innerHTML = thisReport.attachedPost;
            document.getElementById("reportType").innerHTML = thisReport.reportType;
            document.getElementById("reportPlatform").innerHTML = thisReport.reportPlatform;
            document.getElementById("reportContent").innerHTML = thisReport.reportContent;

            // Convert time
            let convertedTime = new Date((thisReport.created / 10000) - epochMicrotimeDiff);
            document.getElementById("reportTime").innerHTML = reorderDateTime(convertedTime.toString());

            // Request for user data
            let userRequest = new XMLHttpRequest();
            let package = new FormData();
            package.append("key", key);
            package.append("guid", thisReport.attachedUser);
            userRequest.open("POST", domain + API_GetUserByGuid, true);
            userRequest.send(package);

            userRequest.onreadystatechange = function () {
                if (userRequest.readyState == 4 & userRequest.status == 200) {
                    let userResponse = JSON.parse(userRequest.responseText);
                    let userInfo = userResponse.package;

                    document.getElementById("userName").innerHTML = userInfo.fullName;

                    // Check if there is an icon
                    if (userInfo.iconUrl == "default") {
                        document.getElementById("userIcon").src = UserIconDir + "noavatar.png";
                    } else {
                        document.getElementById("userIcon").src = UserIconDir + userInfo.guid + "/" + userInfo.iconUrl;
                    }


                }
            }
        }
    }

    // Show detail window
    document.getElementById("content").style.display = "block";
    adjustHeight();
}


function viewPost() {
    document.getElementById("postViewer").style.display = "block";
}


function hideWindow(){
    document.getElementById("content").style.display = "none";
}

function hidePost() {
    document.getElementById("postViewer").style.display = "none";
}


function deleteReport(viewingId, reportGuid) {

    var package1 = new FormData();
    package1.append("key", key);
    package1.append("filterName", "guid");
    package1.append("filterValue", reportGuid);

    var getReportRequest = new XMLHttpRequest();
    getReportRequest.open("POST", domain + API_GetAllReports, true);
    getReportRequest.send(package1);

    getReportRequest.onreadystatechange = function () {
        if (getReportRequest.readyState == 4 && getReportRequest.status == 200) {
            var response = JSON.parse(this.responseText);
            var thisReport = response.package[0];
            thisReport["processResult"] = "Ignored";

            var package2 = new FormData();
            package2.append("key", key);
            package2.append("package", JSON.stringify(thisReport));

            var updateReportRequest = new XMLHttpRequest();
            updateReportRequest.open("POST", domain + API_UpdateReport, true);
            updateReportRequest.send(package2);

            updateReportRequest.onreadystatechange = function () {
                if (updateReportRequest.readyState == 4 && updateReportRequest.status == 200) {
                    document.getElementById(viewingId).remove();
                }
            }
        }
    }

    hideWindow();
}

// Helper functions
function reorderDate(dateString) {
    var splited = dateString.split(" ");
    var newString = splited[2] + " " + splited[1] + " " + splited[3];
    return newString;
}

function reorderDateTime(dateTimeString) {
    var splited = dateTimeString.split(" ");
    var newString = splited[2] + " " + splited[1] + " " + splited[3] + " " + splited[4];
    return newString;
}