function Login() {
    var _ = this;


    _.INIT = function () {
        _.OnRequestByLocalToken();
        $("#SMS_btn").bind("click", _.OnRequestAuthCodeByMobile);
        $("#login_btn").bind("click", _.OnRequestLoginByMobile);
        $("#logout").bind("click", _.CheckLogout);
        $("#closeLogout").bind("click", _.closeLogout);
        $("#confirmLogout_btn").bind("click", _.Logout);
        console.log("start login");

    }


    _.OnRequestByLocalToken = function () {
        var localToken = localStorage.getItem("savedToken");
        console.log("localtoken :" + localToken);
        if (localToken != "null") {
            var LoginByToken = _.parent.proxy.OnRequestLoginByToken(localToken);
            _.isLogin = JSON.parse(LoginByToken.isLogin);
            _.user = LoginByToken.user;
            console.log(_.user);
            _.savedToken = _.user.accesstoken;
            _.userName = _.user.fullName;
            _.isLogin = _.CheckLogin();
            if (_.isLogin) {
                $("#form_page").attr('style', 'display:none');
            }
        }
    }
    //send a request to get the auth message
    _.OnRequestAuthCodeByMobile = function () {
        clearNoteOnLogin();
        mobile = document.getElementById("Phone_Number").value;
        mobile = UconvertPhoneNumber(mobile);

        if (mobile != "wrongNum") {
            addGreenNoteOnLogin("The code message is sent to this mobile");
            clearNoteOnLoginIn10S();
            _.CountDown();
            _.accesstoken = _.parent.proxy.OnRequestAuthCodeByMobile(mobile);
        }
    }
    //send a request to login with the mobile number and auth code 
    //return a token for next access
    _.OnRequestLoginByMobile = function () {
        clearNoteOnLogin();
        mobile = document.getElementById("Phone_Number").value;
        mobile = UconvertPhoneNumber(mobile);
        code = document.getElementById("SMS_Code").value;
        var LoginByMobile = _.parent.proxy.OnRequestLoginByMobile(mobile, code, _.accesstoken);
        _.isLogin = JSON.parse(LoginByMobile.isLogin);
        // _.user = LoginByMobile.user;
        // _.userName = _.user.fullName;
        // console.log(_.user);
        if (!(_.isLogin)) {
            addRedNoteOnLogin("Mobile and Security Code do not match!Please insert right number!");
            redBorderOnMobileInput();
            redBorderOnCodeInput();
            clearNoteOnLoginIn10S();
        } else {
            _.guid = LoginByMobile.guid;
            _.savedToken = LoginByMobile.savedToken;
            _.isLogin = _.CheckLogin();
        }
    }
    //a function to help check if the user login
    //if the user login, the login page hides  
    _.CheckLogin = function () {
        console.log("login flag:" + _.isLogin);
        _.isLogin = _.isLogin == true;
        if (_.isLogin) {
            _.hideLoginPage();
            console.log("save to local" + _.savedToken);
            localStorage.setItem('savedToken', _.savedToken);
            if (_.userName == "Default User") {
                $("#form_page").attr('style', 'display:block');
            } else {
                $("#userName").append(`<h5>Hi, ${_.userName}</h5>`);
            }
        } else {
            _.showLoginPage();
            localStorage.setItem("savedToken", null);
        }
        return _.isLogin;
    }

    _.hideLoginPage = function () {
        $("#login_page").attr('style', 'display:none');
    }

    _.showLoginPage = function () {
        $("#login_page").attr('style', 'display:block');
    }
    //function to logout and clear access token
    _.Logout = function () {
        _.isLogin = false;
        $("#logout-confirm").attr('style', 'display:none');
        document.getElementById("SMS_Code").value = '';
        localStorage.setItem("savedToken", null);
        _.isLogin = _.CheckLogin();
    }

    //double check logout
    _.CheckLogout = function () {
        $("#logout-confirm").attr('style', 'display:block');
    }

    _.closeLogout = function () {
        $("#logout-confirm").attr('style', 'display:none');

    }
    //avoid users to send request too many times
    //users can press get auth code 30s a time
    _.CountDown = function () {
        var timeCount = Time_Count;
        $("#SMS_btn").attr('disabled', 'disabled');
        $("#SMS_btn").attr('cursor', 'default');
        $("#SMS_btn").attr('background-color', 'gray');
        var timeStop = setInterval(function () {
            timeCount--;
            if (timeCount > 0) {
                $("#SMS_btn").text('Please resend in ' + timeCount + ' s');
            } else {
                timeCount = 0;
                $("#SMS_btn").text('Get SMS Code');
                clearInterval(timeStop);
                $("#SMS_btn").removeAttr('disabled');
            }
            localStorage.setItem("timestop", timeCount);
        }, 1000);
    }

    _.onChcekTimeStop = function () {
        var timeCount = localStorage.getItem("timestop");
        if (timeCount > 0) {
            $("#SMS_btn").attr('cursor', 'default');
            $("#SMS_btn").attr('background-color', 'gray');
            var timeStop = setInterval(function () {
                timeCount--;
                if (timeCount > 0) {
                    $("#SMS_btn").text('Please resend in ' + timeCount + ' s');
                    $("#SMS_btn").attr('disabled', 'disabled');
                    $("#SMS_btn").css('cursor', 'default');
                } else {
                    timeCount = 0;
                    $("#SMS_btn").text('Get SMS Code');
                    clearInterval(timeStop);
                    $("#SMS_btn").removeAttr('disabled');
                }
                localStorage.setItem("timestop", timeCount);
            }, 1000);
        }
    }


}