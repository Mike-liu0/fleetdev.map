function UconvertPhoneNumber(mobile) {
    // /^(\+?61|0)4\d{8}$/,
    var convertedMobile;
    var reg1 = /^04\d{8}$/;
    var reg2 = /^\+614\d{8}$/;
    var reg3 = /^614\d{8}$/;
    var reg4 = /^4\d{8}$/;
    if (reg1.test(mobile)) {
        // debug("f1");
        convertedMobile = mobile.replace("0", "61");
    }
    else if (reg2.test(mobile)) {
        // debug("f2");
        convertedMobile = mobile.substring(1);
    }
    else if (reg3.test(mobile)) {
        // debug("f3");
        convertedMobile = mobile;
    }
    else if (reg4.test(mobile)) {
        // debug("f4");
        convertedMobile = "61" + mobile;
    }
    else {
        addRedNoteOnLogin("Please check your mobile number!");
        clearNoteOnLoginIn10S();
        redBorderOnMobileInput();
        convertedMobile = "wrongNum";
    }
    return convertedMobile;
}


function addRedNoteOnLogin(info) {
    document.getElementById("alert-red-note").innerHTML = info;
}

function redBorderOnMobileInput() {
    document.getElementById("mobile_label").style.color = "red";
}


function redBorderOnCodeInput() {
    document.getElementById("code_label").style.color = "red";
}
function addGreenNoteOnLogin(info) {
    document.getElementById("alert-green-note").innerHTML = info;
}

function clearNoteOnLoginIn10S() {
    setTimeout(function () {
        document.getElementById("alert-red-note").innerHTML = "";
        document.getElementById("alert-green-note").innerHTML = "";
        document.getElementById("mobile_label").style.color = "#878787";
        document.getElementById("code_label").style.color = "#878787";
    }, 10000);
}

function clearNoteOnLogin() {
    document.getElementById("alert-red-note").innerHTML = "";
    document.getElementById("mobile_label").style.color = "#878787";
    document.getElementById("alert-green-note").innerHTML = "";
    document.getElementById("code_label").style.color = "#878787";
}

function JsonStringToObject(json) {
    return JSON.parse(json);
}


function ObjectToJsonString(object) {
    return JSON.stringify(object);
}