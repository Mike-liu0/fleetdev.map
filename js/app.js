// Mark sure to change to dev or production
//var API_ROOT = "https://fleet.allinks.com.au/api/";
//var APP_ROOT = "Build/fleet.allinks.com.au.json";

var API_ROOT = "https://fleetdev.allinks.com.au/api/";
var APP_ROOT = "Build/fleetdev.allinks.com.au.json";

var API_KEY = "jQTHqBgLA0aNSNoVUbU0NQ";

var gameInstance = UnityLoader.instantiate("gameContainer", APP_ROOT, { onProgress: UnityProgressAlter });

function UnityProgress(gameInstance, progress) {
    if (!gameInstance.Module) {
        return;
    }
	const loader = document.querySelector("#loader");



    if (!gameInstance.progress) {
        const progress = document.querySelector("#loader .progress");
        progress.style.display = "block";
        gameInstance.progress = progress.querySelector(".full");
        loader.querySelector(".spinner").style.display = "none";
	}

	gameInstance.progress.style.transform = `scaleX(${progress})`;


    if (progress === 1 && !gameInstance.removeTimeout) {
        gameInstance.removeTimeout = setTimeout(function () {
            loader.style.display = "none";
        }, 200);
    }
}


function UnityProgressAlter(gameInstance, progress) {

	if (!gameInstance.Module) {
		return;
	}

	const loader = document.querySelector("#particles-icon");
	const loader_Background = document.querySelector("#particles-background");
	const loader_Foreground = document.querySelector("#particles-foreground");
	const loader_information = document.querySelector("#loader-information");

	var pv = "Loading..." + Math.round(progress * 100) + "%";
	document.getElementById("loader-information").innerHTML = pv;

	if (progress === 1 && !gameInstance.removeTimeout) {
		gameInstance.removeTimeout = setTimeout(function () {
			loader.style.display = "none";
			loader_Background.style.display = "none";
			loader_Foreground.style.display = "none";
			loader_information.style.display = "none";
		}, 200);
	}
	
}


function OnClick_ReloadFrame() {
    document.getElementById("bim-frame").src += "";
}

function OnClick_CloseFrame() {
    document.getElementById("frameContainer").style.display = "none";
}

function OnClick_LoadFrame(url) {
    document.getElementById("bim-frame").src = url;
    document.getElementById("frameContainer").style.display = "block";
}


var attachmentItem;
var attachmentUser;

var uploadedFile = "";
var isUploading = false;

var uploadPost = "";
var uploadUser = "";
var uploadAction = "";

function OnClick_ConfirmUploadImage() {
	console.log("OnClick_ConfirmUploadImage:");

	if (isUploading === false) {

		if (uploadedFile === "") {
			document.getElementById("messages").innerHTML = "No File Selected";
		} else
		{
			document.getElementById("messages").innerHTML = "Uploading file...";

			var reader = new FileReader();
			reader.readAsDataURL(uploadedFile);

			reader.onload = function () {
				var payload = reader.result;
				console.log(payload);
				UploadImage(payload,uploadedFile.name, uploadedFile.type, uploadedFile.size);
			};

		}
	}
}

function UploadImage(payload, name, type, size) {

	console.log("UploadImage: " + name);
	isUploading = true;

	if (uploadAction === "post") {
		$.ajax({
			type: 'POST',
			url: API_ROOT + "OnUploadImage.aspx",

			data: {
				key: API_KEY,
				payload: payload,
				itemGuid: uploadPost,
				action: uploadAction,
				attachmentName: name,
				attachmentExtension: type,
				attachmentSize: size,
			},

			success: function (response) {
				console.log("UploadImage Response: \n" + response);
				isUploading = false;
				var result = JSON.parse(response);

				if (result.result !== "false") {
					OnUploadPostImage_Callback(result.package);
				} else {
					OnUploadPostImage_Callback("false");
				}
			},
			error: function (response) {
				isUploading = false;
				console.log("UploadImage Error: \n" + response);
				OnUploadPostImage_Callback("false");
			}
		});

	} else if (uploadAction === "postedit") {
		$.ajax({
			type: 'POST',
			url: API_ROOT + "OnUploadImage.aspx",

			data: {
				key: API_KEY,
				payload: payload,
				itemGuid: uploadUser,
				action: uploadAction,
				attachmentName: name,
				attachmentExtension: type,
				attachmentSize: size,
			},

			success: function (response) {
				console.log("UploadImage Response: \n" + response);
				isUploading = false;
				var result = JSON.parse(response);

				if (result.result !== "false") {
					OnUploadPostEditorImage_Callback(result.package);
				} else {
					OnUploadPostEditorImage_Callback("false");
				}

			},
			error: function (response) {
				isUploading = false;
				console.log("UploadImage Error: \n" + response);
				OnUploadPostEditorImage_Callback("false");
			}
		});

	} else if (uploadAction === "user") {

		$.ajax({
			type: 'POST',
			url: API_ROOT + "OnUploadImage.aspx",

			data: {
				key: API_KEY,
				payload: payload,
				itemGuid: uploadUser,
				action: uploadAction,
				attachmentName: name,
				attachmentExtension: type,
				attachmentSize: size,
			},

			success: function (response) {
				console.log("UploadImage Response: \n" + response);
				isUploading = false;
				var result = JSON.parse(response);

				if (result.result !== "false") {
					OnUploadUserImage_Callback(result.package);
				} else {
					OnUploadUserImage_Callback("false");
				}

			},
			error: function (response) {
				isUploading = false;
				console.log("UploadImage Error: \n" + response);
				OnUploadUserImage_Callback("false");
			}
		});

	}

}


function OnUploadPostImage_Callback(success) {
	console.log("OnUploadPostImage_Callback: " + success);
	document.getElementById("overlay-frame").style.display = "none";
	gameInstance.SendMessage('JavescriptCallBack', 'OnUploadPostImageCompleteCallback', success);
	uploadedFile = "";
	uploadPost = "";
	uploadUser = "";
	uploadAction = "";
}

function OnUploadUserImage_Callback(success) {
	console.log("OnUploadUserImage_Callback: " + success);
	document.getElementById("overlay-frame").style.display = "none";
	gameInstance.SendMessage('JavescriptCallBack', 'OnUploadUserImageCompleteCallback', success);
	uploadedFile = "";
	uploadPost = "";
	uploadUser = "";
	uploadAction = "";
}

function OnUploadPostEditorImage_Callback(success) {
	console.log("OnUploadUserImage_Callback: " + success);
	document.getElementById("overlay-frame").style.display = "none";
	gameInstance.SendMessage('JavescriptCallBack', 'OnUploadPostImageEditorCompleteCallback', success);
	uploadedFile = "";
	uploadPost = "";
	uploadUser = "";
	uploadAction = "";
}


function OnClick_CancelUploadImage() {
	uploadedFile = "";
	document.getElementById("overlay-frame").style.display = "none";
	if (uploadAction === "post") {
		gameInstance.SendMessage('JavescriptCallBack', 'OnUploadPostImageCompleteCallback', "false");
	} else if (uploadAction === "user") {
		gameInstance.SendMessage('JavescriptCallBack', 'OnUploadUserImageCompleteCallback', "false");
	}
}










// Schiavello Reserved
// Attachment
function OnClick_ConfirmUploadPodAttachment() {
	console.log("OnClick_ConfirmUploadPodAttachment:");

	if (isUploading === false) {

		if (uploadedFile === "") {
			document.getElementById("messages").innerHTML = "No File Selected";
		} else {
			document.getElementById("messages").innerHTML = "Uploading file...";
			var reader = new FileReader();
			reader.readAsDataURL(uploadedFile);

			reader.onload = function () {
				var payload = reader.result;
				console.log(payload);
				UploadAttachment(payload, attachmentItem, attachmentUser, uploadedFile.name, uploadedFile.type, uploadedFile.size);
			};

		}
	}	
}

function OnClick_CancelUploadPodAttachment() {
	uploadedFile = "";
    document.getElementById("overlay-frame").style.display = "none";

}

function UploadAttachment(payload, itemGuid, userGuid, attachmentName, attachmentExtension, attachmentSize) {

	console.log("UploadAttachment: " + itemGuid);
	isUploading = true;

    $.ajax({
        type: 'POST',
        url: API_ROOT + "AddNewPodAttachment.aspx",

        data: {
			key: API_KEY,
			payload: payload,
			itemGuid: itemGuid,
			userGuid: userGuid,
			attachmentName: attachmentName,
			attachmentExtension: attachmentExtension,
			attachmentSize: attachmentSize,
        },

        success: function (response) {
			console.log("UploadAttachment Response: \n" + response);
			isUploading = false;
            var result = JSON.parse(response).success;
			OnUploadAttachment_Callback(result);

        },
		error: function (response) {
			isUploading = false;
			console.log("UploadAttachment Error: \n" + response);
        }
    });
}


function OnUploadAttachment_Callback(success) {
	console.log("OnUploadAttachment_Callback: " + success);
	uploadedFile = "";
	document.getElementById("overlay-frame").style.display = "none";
	gameInstance.SendMessage('JavescriptCallBack', 'OnAttachmentCompleteCallback', success);
}


var attachedProject;
var projectName;
var attachedUser;
var podTypeName;
var attachedTaskGroup;

function OpenUploaderBimFrame_Override() {

	// Enter default value:
	attachedUser = "bbf174dd-e476-4c93-836f-25a6ef38da6b";
	attachedProject = "7dab9ac3-0b23-4615-b597-0617c6582da9";
	taskGroupName = "MUCCP-1A";
	attachedTaskGroup = "6ecb5924-02e6-4578-b83c-461ee0e3ccc3";
	projectName = "XXX";

	// fix the url
	var newUrl = BIM_INDEX_ROOT +
		"attachedUser=" + attachedUser + "&" +
		"attachedProject=" + attachedProject + "&" +
		"attachedTaskGroup=" + attachedTaskGroup + "&" +
		"projectName=" + projectName + "&" +
		"taskGroupName=" + taskGroupName;
	console.log(newUrl);
	// reload url
	document.getElementById("bim-uploader").src = newUrl;

	document.getElementById('overlay-frame-bim-uploader').style.display = "block";
	document.getElementById("bim-uploader").src += ''; // not very good way to reload iframe
}


function OnClick_CloseUploaderBimFrame() {
	CloseUploaderBimFrame();
} 


function CloseUploaderBimFrame(){
	document.getElementById('overlay-frame-bim-uploader').style.display = "none";
	//gameInstance.SendMessage('MessageCallBack', 'OnBimCompleteCallback', success);
} 


// BIM uploading
function UploaderBim() {
	document.getElementById('overlay-frame-bim-uploader').style.display = "block";
	document.getElementById("bim-uploader").src = url;
}

function OnConfirmBimUploaderFrame() {

}

function OnConfirmBimUploaderFrame_Callback(success) {

}


/*
filedrag.js - HTML5 File Drag & Drop demonstration
Featured on SitePoint.com
Developed by Craig Buckler (@craigbuckler) of OptimalWorks.net
*/
(function () {

	// getElementById
	function $id(id) {
		return document.getElementById(id);
	}


	// output information
	function Output(msg) {
		var m = $id("messages");
		m.innerHTML = msg;
	}


	// file drag hover
	function FileDragHover(e) {
		e.stopPropagation();
		e.preventDefault();
		e.target.className = (e.type == "dragover" ? "hover" : "");
	}




	// file selection
	function FileSelectHandler(e) {

		// cancel event and hover styling
		FileDragHover(e);

		// fetch FileList object
		var files = e.target.files || e.dataTransfer.files;
		uploadedFile = files[0];
		ParseFile(uploadedFile);

	}


	// output file information
	function ParseFile(file) {
		Output(
			"<p>File information: <strong>" + file.name +
			"</strong> type: <strong>" + file.type +
			"</strong> size: <strong>" + file.size +
			"</strong> bytes</p>"
		);

	}


	// initialize
	function Init() {

		var fileselect = $id("fileselect"),
			filedrag = $id("filedrag"),
			submitbutton = $id("submitbutton");

		// file 
		if (fileselect !== null) {
			fileselect.addEventListener("change", FileSelectHandler, false);
		}

		// is XHR2 available?
		var xhr = new XMLHttpRequest();
		if (xhr.upload) {

			if (filedrag !== null) {
				// file drop
				filedrag.addEventListener("dragover", FileDragHover, false);
				filedrag.addEventListener("dragleave", FileDragHover, false);
				filedrag.addEventListener("drop", FileSelectHandler, false);
				filedrag.style.display = "block";
			}

			if (submitbutton !== null) {
				// remove submit button
				submitbutton.style.display = "none";
            }

		}

	}

	// call initialization file
	if (window.File && window.FileList && window.FileReader) {
		Init();
	}


})();



// ============== Registed Key ========================





// ============== utility ========================



function OnRequestLocation() {

	if (navigator.geolocation) {
		navigator.geolocation.getCurrentPosition(

			function (position) {
				var lat = position.coords.latitude;
				var lng = position.coords.longitude;
				var _package = lat + ',' + lng;
				console.log('Package = ' + _package);
				gameInstance.SendMessage('JavescriptCallBack', 'ReceiveLocation', _package);
			},
			function (error) {
				console.log('OnRequestLocation: Error:' + error.errorMessage);
			},
			{ timeout: 10000 }

		);

	} else {
		alert("Geolocation is not supported by this browser!");
	}
}


