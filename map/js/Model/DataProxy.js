function DataProxy() {
    var _ = this;



    // MARK 
    _.OnRequestVehicleTracking = function (deviceID) {
        var package;

        $.ajax({
            type: 'POST',
            async: false,
            url: API_Root + API_OnRequestVechicleTracking,
            data: {
                key: API_Key,
                deviceID: deviceID,
            },
            success: function (response) {
                console.log("API COMPELTE Response :" + response);
                package = JSON.parse(response).package;
                // Callback
                //controller.OnRequestVehicleTracking_Callback(package);
            },
            error: function (response) {
                console.log("API error Response :" + response);
            }
        })
        return package;
    }

    _.OnRequestDirections = function (startEnd) {
        var routes;
        $.ajax({
            type: 'GET',
            async: false,
            url: API_directions + startEnd + '.json?',
            data: {
                access_token: map_accessToken,
                geometries: 'geojson',
                overview: 'full',
            },
            success: function (response) {
                routes = response.routes;
            },
            error: function (response) {
                console.log("API error Response :" + response);
            }

        })
        return routes;
    }

    _.OnClearVehicleTracking = function (deviceID) {
        var success;
        $.ajax({
            type: 'GET',
            async: false,
            url: API_Root + API_OnClearVechicleTracking,
            data: {
                key: API_Key,
                deviceID: deviceID,
            },
            success: function (response) {
                success = JSON.parse(response).success;
            },
            error: function (response) {
                console.log(response);
            }
        })
        return success;
    }

    _.OnSendVehicleTracking = function (package) {
        var MSG;
        $.ajax({
            type: 'POST',
            async: false,
            url: API_Root + API_OnSendVechicleTracking,
            data: {
                key: API_Key,
                package: package,
            },
            success: function (response) {
                MSG = response;
            },
            error: function (response) {
                console.log(response);
            }
        })
        return MSG;
    }
    // login functions
    _.OnRequestAuthCodeByMobile = function (mobile) {
        var accesstoken;
        $.ajax({
            type: 'POST',
            async: false,
            url: API_Root + get_SMS_api,
            data: {
                key: API_Key,
                mobile: mobile,
            },
            success: function (response) {
                console.log(response);
                var package_info = JSON.parse(response).package;
                accesstoken = package_info.accesstoken;
            },
            error: function (response) {
                login = false;
                console.log("API error Response :" + response);
            }
        });

        return accesstoken;
    }


    _.OnRequestLoginByMobile = function (mobile, code, accesstoken) {
        var return_Token;
        var flag = false;
        var info;
        var guid;
        var user;
        $.ajax({
            type: 'POST',
            async: false,
            url: API_Root + LoginByMobile_api,
            data: {
                key: API_Key,
                mobile: mobile,
                code: code,
                token: accesstoken,
            },
            success: function (response) {
                console.log(response);
                var package_info = JSON.parse(response).success;
                flag = package_info;
                user = JSON.parse(response).package;
                info = JSON.parse(response).message;
                guid = JSON.parse(response).package.guid;
                return_Token = JSON.parse(response).package.accesstoken;

            },
            error: function (response) {
                login = false;
                console.log("API error Response :" + response);
            }
        });
        return {
            guid: guid,
            isLogin: flag,
            info: info,
            savedToken: return_Token,
            user: user,
        };

    }

    _.OnRequestLoginByToken = function (accesstoken) {
        var flag;
        var return_Token;
        var guid;
        var user;
        $.ajax({
            type: 'POST',
            async: false,
            url: API_Root + LoginByToken_api,
            data: {
                key: API_Key,
                token: accesstoken,
            },

            success: function (response) {
                flag = JSON.parse(response).success;
                user = JSON.parse(response).package;
            },
            error: function (response) {
                login = false;
                return_Token = null;
                console.log("API error Response :" + response);

            }
        });
        return {
            isLogin: flag,
            user: user,
        };

    }


}