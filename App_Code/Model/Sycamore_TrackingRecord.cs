using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Fleet_TrackingRecord
/// </summary>
[Serializable]
public class Sycamore_TrackingRecord
{
    public string guid;
    public string created = DateTime.Now.Ticks.ToString();
    public string updated = DateTime.Now.Ticks.ToString();
    public string status = Config.DevelopmentStage;       // dev/production/pending/testing/other
    public string tableName = "Sycamore_TrackingRecord";

    public string attachedDevice = "none";
    public string attachedVehicle = "none";
    public string attachedOperation = "none";
    public string attachedDriver = "none";

    public int packageID;
    public string SupportedPid;

    /* Processed OBD Data */
    public int vehicleRPM;
    public int vehicleSpeed;
    public int vehicleCoolentTemp;
    public int vehicleEngineOilTemp;
    public float vehicleFuelConsumption;
    public float vehicleBatteryVoltage;
    public int vehicleClearMile;  // mile after clear
    public String vehicleEngineCode;

    /* GPS*/
    public double gnssLatitude = 0;
    public double gnssLongitude = 0;
    public double gnssAltitude =0;
    public float gnssSpeed = 0;
    public string gnssCourse = "0";
    public string rawData;

    public Sycamore_TrackingRecord()
    {
        guid = Guid.NewGuid().ToString();
    }

    public static string ToJson(Sycamore_TrackingRecord _item)
    {
        return JsonConvert.SerializeObject(_item);
    }

    public static Sycamore_TrackingRecord FromJson(string _json)
    {
        return JsonConvert.DeserializeObject<Sycamore_TrackingRecord>(_json);
    }
}

[Serializable]
public class DeviceRecord
{
    /* OBD Data */
    public string id = "0000";

    public string pid = "00";
    public string model = "none";

    public string raw_rpm = "0";
    public string raw_speed = "0";
    public string raw_oiltempeture = "0";
    public string raw_enginetempeture = "0";
    public string raw_battery = "0";
    public string raw_fuelconsumption = "0";
    public string raw_mile = "0";
    public string raw_enginecode = "0";
    /*
     * 1_D000-0001_A_35_2000_D_E_F_G_H_I_J_K_GPS
    int  PackageID = 0;
    String data_pid = "N";
    String data_model = "N";

    String data_rpm = "N";
    String data_speed = "N";
    String data_oilTemp = "N";
    String data_engineTemp = "N";
    String data_battery = "N";
    String data_fuel = "N";
    String data_mile = "N";
    String data_engine_code = "N";

    String data_gps = "N";

    14_
    D000-0005_
    OK_
    N_
    41,0C,0C,E0,41,0C,0C,DC,_
    41,0D,00,41,0D,00,_
    NO,DATA_
    41,05,72,41,05,72,_
    14.5V_
    NO,DATA_
    41,31,85,4C,41,31,85,48,_
    N_
    1,1,20210508054449.000,-37.794180,144.937802,26.500,0.00,91.5,1,,0.8,1.1,0.7,,18,8,2,,,34,,

    /*

   /* GNSS Data Structure ======  1,0,0,41.40338, 2.17403,,0,0,0,0,0,0,0,0,0,0,0,0,0
   <GNSS run status>,
   <Fix status>,
   <UTC date & Time>,
   <Latitude>,
   <Longitude>,
   <MSL Altitude>,
   <Speed Over Ground>,
   <Course Over Ground>,
   <Fix Mode>,
   <Reserved1>,
   <HDOP>,
   <PDOP>,
   <VDOP>,
   <Reserved2>,
   <GNSS Satellites in View>,
   <Reserved3>,
   <HPA>,
   <VPA>


   */
    public string raw_gps = "0";  // 18 block

    public static string ToJson(DeviceRecord _item)
    {
        return JsonConvert.SerializeObject(_item);
    }

    public static DeviceRecord FromJson(string _json)
    {
        return JsonConvert.DeserializeObject<DeviceRecord>(_json);
    }
}

