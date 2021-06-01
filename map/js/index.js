var controller = new MapController();
var timer;
var updateTime = 10000;
// random add position https://fleetdev.allinks.com.au/api/OnSendVehicleTrackingRandomly.aspx?key=xVgsbCuuxkCGFDwLk1EpEQ&package=1_D000-0001




$(document).ready(
    function () {
        controller.INIT();
        $("#close_btn").bind("click", hide_table);
        $("#show_table").bind("click", show_table);
        $(".mapboxgl-ctrl-bottom-left").attr('style', 'display:none');
    }
);


function getUserSelectedDevice() {
    resetTableData();
    var deviceId = document.getElementById("deviceId");
    var packageNumber;
    document.getElementById("tracking").innerHTML = "Now Tracking: " + deviceId.value;
    console.log("package geted");
    var package = controller.OnRequestVehicleTracking(deviceId.value);

    controller.removeLayers();

    if (package.trackingRecords != null) {
        if (package.trackingRecords.length > 0) {
            packageNumber = package.trackingRecords.length;
            getTableValue(package.trackingRecords);
            controller.readData(package);
        }
        else {
            packageNumber = 0;
        }
    }
    else {
        packageNumber = 0;
    }
    document.getElementById("packageNumber").innerHTML = "Node Number: " + packageNumber;
}


function clearVehicleRecord() {
    var deviceId = document.getElementById("deviceId");
    var api_response = controller.OnClearVehicleTracking(deviceId.value);
    resetTableData();
    if (api_response == 'true') {
        console.log("Clear Data Success");
        getUserSelectedDevice();
    } else {
        console.log(api_response);
    }
}

function getTableValue(package) {
    var index = package.length - 1;

    var rpm = package[index].vehicleRPM;
    var speed = package[index].vehicleSpeed;
    var c_temp = package[index].vehicleCoolentTemp;
    var e_temp = package[index].vehicleEngineTemp;
    var consumption = package[index].vehicleFuelConsumption;
    var voltage = package[index].vehicleBatteryVoltage;
    var mile = package[index].vehicleClearMile;
    var code = package[index].vehicleEngineCode;

    var vehicle_name = ['rpm', 'speed', 'c_temp', 'e_temp', 'consumption', 'voltage', 'mile', 'code'];
    var vehicle_info = [rpm, speed, c_temp, e_temp, consumption, voltage, mile, code];

    var index = 0;
    vehicle_name.forEach(element => {
        document.getElementById(element).innerHTML = checkData(vehicle_info[index], code);
        index++;
    });
}


function checkData(_input, _code) {
    console.log(_input);
    if(_input > -100) {
        return _input;
    } else if (_input == _code) {
        return _code;
    } else {
        return "NO DATA";
    }
}


function resetTableData() {
    var vehicle_info = ['rpm', 'speed', 'c_temp', 'e_temp', 'consumption', 'voltage', 'mile', 'code'];
    vehicle_info.forEach(element => {
        $('#'+element).html("NO DATA");
    });
}


function getUpdatedData() {
    //var time = getUpdateTime();
    if (document.getElementById("updateChecked").checked) {
        updateTime = getUpdateTime();
        timer = setInterval(function () { getUserSelectedDevice() }, updateTime);
    } else {
        clearInterval(timer);
    }
}


function getUpdateTime() {
    var selectedTime = document.getElementById("updateTime").value;
    switch (selectedTime) {
        case "3s":
            return 3000;
        case "5s":
            return 5000;
        case "10s":
            return 10000;
        case "30s":
            return 30000;
        case "60s":
            return 60000;
        default:
            return 10000;
    }
}

//hide or show data table 
function hide_table() {
    $("#data_table").attr('style', 'display:none');
    $("#show_table").attr('style', 'display:block');
}

function show_table() {
    $("#data_table").attr('style', 'display:block');
    $("#show_table").attr('style', 'display:none');
}