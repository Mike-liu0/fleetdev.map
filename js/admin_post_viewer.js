/* ============================================================================
 * File: post.html
 * Author: Deyou Zou
 * Date: 22 Dec 2020
 * Project: Sycamore - ALLINK
 ==============================================================================
 */

var key = "jQTHqBgLA0aNSNoVUbU0NQ";
var domain = "https://dev.allinks.com.au/api/";
var API_GetAllEarthworks = "GetAllEarthworks.aspx";
var API_GetUserByGuid = "GetUserByGuid.aspx";
var API_GetAllMessages = "GetAllMessages.aspx";
var API_GetAllReports = "GetAllReports.aspx";
var API_UpdateReport = "UpdateReport.aspx";
var API_UpdateEarthwork = "UpdateEarthwork.aspx";
var EarthworkImageDir = "resource/post/";
var UserIconDir = "resource/user/";
var epochMicrotimeDiff = Math.abs(new Date(0, 0, 1).setFullYear(1));



// Get the query string
const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
const guid = urlParams.get('guid');

update(guid);

function update(guid) {
    var earthworkRequest = new XMLHttpRequest();
    var package = new FormData();
    package.append("key", key);
    package.append("filterName", "guid");
    package.append("filterValue", guid);
    earthworkRequest.open("POST", domain + API_GetAllEarthworks, true);
    earthworkRequest.send(package);

    earthworkRequest.onreadystatechange = function () {
        if (earthworkRequest.readyState == 4 & earthworkRequest.status == 200) {
            var response = JSON.parse(earthworkRequest.responseText);
            var earthworkinfo = response.package[0];

            // If deleted, change the button
            if (earthworkinfo.status == "archived") {
                document.getElementById("dltbuttontext").innerHTML = "Deleted";
                document.getElementById("dltbuttontext").onclick = "";
                document.getElementById("dltbuttontext").style.color = "gray";
            }

            // Show earthwork image
            if (earthworkinfo == undefined || earthworkinfo.attachedImages=="default") {
                document.getElementById("EarthWork").src = EarthworkImageDir + "/" + "noimage.png";
                document.getElementById("Icon").src = UserIconDir + "noavatar.png";
            } else {
                var imagedir = EarthworkImageDir + earthworkinfo.guid + "/" + earthworkinfo.attachedImages;
                document.getElementById("EarthWork").src = imagedir;
            }
            
            // Value validity check
            var attributeList = ["siteAddress", "equipmentType", "workingHourStart", "workingHourEnd",
                "soilType", "availableTime", "accessiability", "soilVolume", "price", "deliveryMethod","workingDays"];
            for (attr of attributeList) {
                if (earthworkinfo[attr] == "N" || earthworkinfo[attr] == "undefined") {
                    earthworkinfo[attr] = "Not Provided";
                }
            }
            
            // Decide theme colour based on demand or supply
            if (earthworkinfo.postType == "supply") {
                document.getElementById("postType").style.backgroundColor = "#00ABFFFF";
                document.getElementById("siteAddressMark").style.color = "#00ABFFFF";
                document.getElementById("soilTypeMark").style.color = "#00ABFFFF";
                document.getElementById("equipmentTypeMark").style.color = "#00ABFFFF";
                document.getElementById("soilVolumeMark").style.color = "#00ABFFFF";
                document.getElementById("priceMark").style.color = "#00ABFFFF";
                document.getElementById("availableTimeMark").style.color = "#00ABFFFF";
                document.getElementById("deliveryMethodMark").style.color = "#00ABFFFF";
                document.getElementById("deliveryCostMark").style.color = "#00ABFFFF";
                document.getElementById("workingDaysMark").style.color = "#00ABFFFF";
                document.getElementById("workingHourMark").style.color = "#00ABFFFF";
                document.getElementById("accessiabilityMark").style.color = "#00ABFFFF";
                document.getElementById("commentsMark").style.color = "#00ABFFFF";
                document.getElementById("dltbutton").style.backgroundColor = "#00ABFFFF";
                document.getElementById("delete").style.backgroundColor = "#00ABFFFF";
            } else if (earthworkinfo.postType == "demand") {
                document.getElementById("postType").style.backgroundColor = "#FF860AFF";
                document.getElementById("siteAddressMark").style.color = "#FF860AFF";
                document.getElementById("soilTypeMark").style.color = "#FF860AFF";
                document.getElementById("equipmentTypeMark").style.color = "#FF860AFF";
                document.getElementById("soilVolumeMark").style.color = "#FF860AFF";
                document.getElementById("priceMark").style.color = "#FF860AFF";
                document.getElementById("availableTimeMark").style.color = "#FF860AFF";
                document.getElementById("deliveryMethodMark").style.color = "#FF860AFF";
                document.getElementById("deliveryCostMark").style.color = "#FF860AFF";
                document.getElementById("workingDaysMark").style.color = "#FF860AFF";
                document.getElementById("workingHourMark").style.color = "#FF860AFF";
                document.getElementById("accessiabilityMark").style.color = "#FF860AFF";
                document.getElementById("commentsMark").style.color = "#FF860AFF";
                document.getElementById("dltbutton").style.backgroundColor = "#FF860AFF";
                document.getElementById("delete").style.backgroundColor = "#FF860AFF";
            }

            // Show description
            document.getElementById("description").innerHTML = earthworkinfo.detailContent;

            // General info
            document.getElementById("postStatus").innerHTML = earthworkinfo.postStatus;
            document.getElementById("postStatus").style.display = "block";
            document.getElementById("siteAddressContent").innerHTML = earthworkinfo.siteAddress;

            // If working days provided, show it
            if (earthworkinfo.workingDays != undefined) {
                document.getElementById("workingDaysContent").innerHTML = convertWorkDayList(earthworkinfo.workingDays);
                if (document.getElementById("workingDaysContent").innerHTML != "Working Days" && document.getElementById("workingDaysContent").innerHTML != "") {
                    document.getElementById("workingDays").style.display = "block";
                }
            }


            // Equipment Post
            if (earthworkinfo.workType == "Equipment") {
                document.getElementById("postTypeContent").innerHTML = earthworkinfo.postType.capitalize() + " Equipment";
                document.getElementById("equipmentTypeContent").innerHTML = earthworkinfo.equipmentType;
                document.getElementById("equipmentType").style.display = "block";
                document.getElementById("workingHourContent").innerHTML = earthworkinfo.workingHourStart + " - " + earthworkinfo.workingHourEnd;
                document.getElementById("workingHour").style.display = "block";
                document.getElementById("availableTimeContent").innerHTML = earthworkinfo.availableTime;
                document.getElementById("availableTime").style.display = "block";

            }

            if (earthworkinfo.workType == "Earthwork") {
                document.getElementById("postTypeContent").innerHTML = earthworkinfo.postType.capitalize() + " Material";
                document.getElementById("soilTypeContent").innerHTML = earthworkinfo.soilType;
                document.getElementById("soilType").style.display = "block";
                document.getElementById("accessiabilityContent").innerHTML = earthworkinfo.accessiability;
                document.getElementById("accessiability").style.display = "block";
            }

            // Show details
            document.getElementById("soilVolumeContent").innerHTML = earthworkinfo.soilVolume;
            document.getElementById("priceContent").innerHTML = earthworkinfo.price;
            document.getElementById("deliveryMethodContent").innerHTML = earthworkinfo.deliveryMethod;
            document.getElementById("deliveryMethodContent").innerHTML = earthworkinfo.deliveryMethod;

            
            // Get user information
            var userRequest = new XMLHttpRequest();
            var package = new FormData;
            package.append("key", key);
            package.append("guid", earthworkinfo.attachedUser);
            userRequest.open("POST", domain + API_GetUserByGuid, true);
            userRequest.send(package);

            userRequest.onreadystatechange = function () {
                if (userRequest.readyState == 4 & userRequest.status == 200) {
                    var response = JSON.parse(userRequest.responseText);
                    var userinfo = response.package;
                    if (userinfo.iconUrl != "default") {
                        // Show user icon
                        document.getElementById("Icon").src = UserIconDir + userinfo.guid + "/" + userinfo.iconUrl;
                    } else {
                        // Show default icon
                        document.getElementById("Icon").src = UserIconDir + "noavatar.png";
                    }
                    document.getElementById("Username").innerHTML = userinfo.fullName;

                    // Show post time
                    var convertedTime = new Date((earthworkinfo.updated / 10000) - epochMicrotimeDiff);
                    document.getElementById("PostTime").innerHTML = "Posted: " + reorderDate(convertedTime.toDateString());


                }
            }

            // Update comments
            var commentsRequest = new XMLHttpRequest();
            var commentsPackage = new FormData();
            commentsPackage.append("key", key);
            commentsPackage.append("filterName", "attachedPost");
            commentsPackage.append("filterValue", guid);

            commentsRequest.open("POST", domain + API_GetAllMessages, true);
            commentsRequest.send(commentsPackage);

            commentsRequest.onreadystatechange = function () {
                if (commentsRequest.readyState == 4 & commentsRequest.status == 200) {
                    var response = JSON.parse(commentsRequest.responseText);
                    var messages = response.package;

                    document.getElementById("commentsTitle").innerHTML = "Comments (" + messages.length + ")";

                    var i = 0;
                    while (i < messages.length) {

                        // Build up the structure
                        let newDiv = document.createElement("div");
                        newDiv.id = "message" + i;
                        newDiv.classList.add("messageDivs");

                        let newImg = document.createElement("img");
                        newImg.id = "msgUserIcon" + i;
                        newImg.classList.add("msgUserIcons");
                        newDiv.appendChild(newImg);

                        let newUserName = document.createElement("p");
                        newUserName.id = "msgUserName" + i;
                        newUserName.classList.add("msgUserNames");
                        newDiv.appendChild(newUserName);

                        let newPosted = document.createElement("p");
                        newPosted.id = "msgPosted" + i;
                        newPosted.classList.add("msgPosteds");
                        newDiv.appendChild(newPosted);

                        let newHr = document.createElement("hr");
                        newDiv.appendChild(newHr);

                        let newContent = document.createElement("p");
                        newContent.id = "msgContent" + i;
                        newContent.classList.add("msgContents");
                        newDiv.appendChild(newContent);


                        // Update data
                        let thisMessage = messages[i];
                        let messageContent = thisMessage.messageContent;
                        newContent.innerHTML = messageContent;

                        // Convert time
                        let convertedTime = new Date((thisMessage.updated / 10000) - epochMicrotimeDiff);
                        newPosted.innerHTML = "Posted: " + reorderDateTime(convertedTime.toString());

                        // Request for user data
                        let userRequest = new XMLHttpRequest();
                        let package = new FormData();
                        package.append("key", key);
                        package.append("guid", thisMessage.attachedUser);
                        userRequest.open("POST", domain + API_GetUserByGuid, true);
                        userRequest.send(package);

                        userRequest.onreadystatechange = function () {
                            if (userRequest.readyState == 4 & userRequest.status == 200) {
                                let userResponse = JSON.parse(userRequest.responseText);
                                let userInfo = userResponse.package;

                                newUserName.innerHTML = userInfo.fullName;

                                // Check if there is an icon
                                if (userInfo.iconUrl == "default") {
                                    newImg.src = UserIconDir + "noavatar.png";
                                } else {
                                    newImg.src = UserIconDir + userInfo.guid + "/" + userInfo.iconUrl;
                                }

                                
                            }
                        }
                        document.getElementById("details").appendChild(newDiv);
                        i++;
                    }
                    
                }
            }
        }
    }
}

function deletePost() {
    var curPostGuid = guid;
    var package1 = new FormData();
    package1.append("key", key);
    package1.append("filterName", "guid");
    package1.append("filterValue", curPostGuid);

    var getPostRequest = new XMLHttpRequest();
    getPostRequest.open("POST", domain + API_GetAllEarthworks, true);
    getPostRequest.send(package1);

    getPostRequest.onreadystatechange = function () {
        if (getPostRequest.readyState == 4 && getPostRequest.status == 200) {
            var response = JSON.parse(this.responseText);
            var thisPost = response.package[0];
            thisPost["status"] = "archived";

            var package2 = new FormData();
            package2.append("key", key);
            package2.append("package", JSON.stringify(thisPost));

            var updatePostRequest = new XMLHttpRequest();
            updatePostRequest.open("POST", domain + API_UpdateEarthwork, true);
            updatePostRequest.send(package2);

            updatePostRequest.onreadystatechange = function () {
                if (updatePostRequest.readyState == 4 && updatePostRequest.status == 200) {
                    archiveReports();
                }
            }
        }
    }
}

function archiveReports() {
    var package = new FormData();
    package.append("key", key);
    package.append("filterName", "processResult");
    package.append("filterValue", "Unprocessed");

    var request = new XMLHttpRequest();
    request.open("POST", domain + API_GetAllReports, true);
    request.send(package);

    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var response = JSON.parse(this.responseText).package;
            var toArchive = [];

            for (var report of response) {
                if (report.attachedPost == guid) {
                    toArchive.push(report)
                }
            }
            deleteReport(toArchive);
        }
    }
}

function deleteReport(toArchive) {
    if (toArchive.length == 0) {
        window.open("report.html", "_self");
        return;
    }
    var thisReport = toArchive.pop(0);
    thisReport["processResult"] = "Post Deleted";

    var package = new FormData();
    package.append("key", key);
    package.append("package", JSON.stringify(thisReport));

    var updateReportRequest = new XMLHttpRequest();
    updateReportRequest.open("POST", domain + API_UpdateReport, true);
    updateReportRequest.send(package);

    updateReportRequest.onreadystatechange = function () {
        if (updateReportRequest.readyState == 4 && updateReportRequest.status == 200) {
            deleteReport(toArchive);
        }
    }
}

String.prototype.capitalize = function () {
    return this.charAt(0).toUpperCase() + this.slice(1);
}

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

function convertWorkDayList(wdlist) {
    var result = "";
    for (wd of wdlist) {
        if (wd["isWorking"] == true) {
            if (result == "") {
                result += wd["day"]
            } else {
                result += ", "
                result += wd["day"]
            }
        }
    }
    return result;
}