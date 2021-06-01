var controller = new testController();
$(document).ready(
    function () {
        controller.INIT();

    }
);

function testController() {
    var _ = this;
    var proxy = new DataProxy();
    var packageID;

    _.INIT = function () {
        _.proxy = proxy;
        _.proxy.parent = _;
        //bind function with bottons
        $("#deviceId").bind("click", _.enableSubmit);
        $("#clear_btn").bind("click", _.clearRecord);
        $("#submit_btn").bind("click", _.submitRecord);
        $("#random_btn").bind("click", _.randomLocation);
    }

    _.enableSubmit = function () {
        //make sure the submit works when the user select one device
        if (document.getElementById("deviceId").value != "Select Device Id") {
            $("#submit_btn").attr("disabled", false);
        }
    }

    _.submitRecord = function () {
        console.log("send a request");
        //read value from table
        var deviceId = document.getElementById("deviceId").value;
        var rpm = document.getElementById("rpm").value;
        var speed = document.getElementById("speed").value;
        var c_temp = document.getElementById("c_temp").value;
        var e_temp = document.getElementById("e_temp").value;
        var consumption = document.getElementById("consumption").value;
        var voltage = document.getElementById("voltage").value;
        var mile = document.getElementById("mile").value;
        var code = document.getElementById("code").value;
        var lng = document.getElementById("lng").value;
        var lat = document.getElementById("lat").value;


        //update packageID after send one request

        if (packageID == undefined) {
            packageID = 1;
        }
        packageID = packageID + 1;
        document.getElementById("packageNumber").innerHTML = "packageID: " + packageID;

        console.log(deviceId);

        //send infos on api
        //default value , some values needs to update
        var gps = "1,1,20210506021643.000," + lat + "," + lng + ",26.500,0.00,91.5,1,,0.8,1.1,0.7,,18,8,2,,,34,,"
        var package = packageID + "_" + deviceId + "_OK_N_" + rpm + "_" + speed + "_" +
            c_temp + "_" + e_temp + "_" + voltage + "_" + consumption + "_" + mile + "_" + code + "_" + gps;
        console.log(package);
        //send the infos on API
        var message = _.proxy.OnSendVehicleTracking(package);
        console.log(message);
        alert(message);
    }



    _.clearRecord = function () {
        //refresh the page
        location.reload();
    }

    _.randomLocation = function () {
        //randomly generate a gps position, and the range is (-0.005,0.005)
        var lng = (parseFloat(document.getElementById("lng").value) + ((Math.random() - 0.3) * 0.01)).toFixed(6);
        var lat = (parseFloat(document.getElementById("lat").value) + ((Math.random() - 0.3) * 0.01)).toFixed(6);
        document.getElementById("lng").value = lng;
        document.getElementById("lat").value = lat;
    }
}
