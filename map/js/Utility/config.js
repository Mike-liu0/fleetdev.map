// API Configuration
var API_Key = "xVgsbCuuxkCGFDwLk1EpEQ";
//var API_Root = "../../api/";
var API_Root = "https://fleetdev.allinks.com.au/api/";

var API_OnRequestVechicleTracking = "OnRequestVehicleTracking.aspx";    // Request a list of tracking records via device ID
var API_OnSetVechicleTracking = "OnSetVehicleTracking.aspx";            // Send over a package of tracking record to the attached device
var API_OnClearVechicleTracking = "OnClearVehicleTracking.aspx";        // Clear all tracking records by a specified device ID
var API_OnSendVechicleTracking = "OnSendVehicleTracking.aspx";        // Clear all tracking records by a specified device ID

var API_directions = 'https://api.mapbox.com/directions/v5/mapbox/driving/';
// Thirdparty Configuration
var map_accessToken = 'pk.eyJ1IjoieXVoZTA5MjUiLCJhIjoiY2tkbnJobWM2MHY2ZTJycWltNG1lbm5xNyJ9.OPHhWDcFai-mdAqNLzhATA';
var map_style_url = 'mapbox://styles/yuhe0925/ckmyi9chk1i4t17mn3iiepo70';


// Login parameter
var Time_Count = 30;
var get_SMS_api = "OnRequestAuthCodeByMobile.aspx";
var LoginByMobile_api = "OnRequestLoginByMobile.aspx";
var LoginByToken_api = "OnRequestLoginByToken.aspx";