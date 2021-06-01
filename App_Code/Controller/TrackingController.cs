using Amazon.DynamoDBv2.Model;
using Darkspede;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for TrackingController
/// </summary>
public class TrackingController
{
    public static TrackingController Instance = new TrackingController();

    public  Dictionary<string, Sycamore_Operation> Operation = new Dictionary<string, Sycamore_Operation>();

    //private int QueueSize = 30;

    Sycamore_Operation op;

    public TrackingController()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }


    public  Sycamore_Operation GetOperation( string _id )
    {
        if (Operation.ContainsKey(_id))
        {
            return Operation[_id];
        }
        return null;
    }


    public  int GetOperationCmd(string _id)
    {
        if (Operation.ContainsKey(_id))
        {

        }

        return 200;
    }

    public  void AddRecords(string _id, Sycamore_TrackingRecord _record)
    {
        if (Operation.ContainsKey(_id))
        {
            if(Operation[_id].trackingRecords.Count > (Config.RecordQueueMaxCount - 1))
            {
                Operation[_id].trackingRecords.Dequeue();
            }
            Operation[_id].trackingRecords.Enqueue(_record);
            AddRecordToDB(Operation[_id], _record);
        }
        else
        {
            op = new Sycamore_Operation();
            op.trackingRecords.Enqueue(_record);
            Operation.Add(_id, op);
            AddRecordToDB(op, _record);
        }
    }

    public void AddRecordToDB(Sycamore_Operation _operation, Sycamore_TrackingRecord _record)
    {
        AWSController controller = new AWSController();
        int onIndex = -1;

        if (_record.SupportedPid.Contains("Down") && _operation.trackingRecords.Count > 1)
        {
            var recordList = _operation.trackingRecords.ToList();

            foreach(Sycamore_TrackingRecord record in recordList)
            {
                if(record.SupportedPid.Contains("On"))
                {
                    onIndex = recordList.IndexOf(record);
                }
            }

            if (onIndex < _operation.trackingRecords.Count && onIndex != -1)
            {
                int index = 0;
                while (index != onIndex)
                {
                    _operation.trackingRecords.Dequeue();
                    index++;
                }
                controller.OnDynamoDB_AddNewOperation(_operation);
                _operation.trackingRecords.Clear();
            }
        }
    }


    /*
    record.id = dataArray[1];

    record.pid = dataArray[2];
    record.model = dataArray[3];

    record.raw_rpm = dataArray[4];
    record.raw_speed = dataArray[5];
    record.raw_oiltempeture = dataArray[6];
    record.raw_enginetempeture = dataArray[7];
    record.raw_battery = dataArray[8];
    record.raw_fuelconsumption = dataArray[9];
    record.raw_mile = dataArray[10];

    record.raw_enginecode = dataArray[11];

    record.raw_gps = dataArray[12];
    */
    public  Sycamore_TrackingRecord ProcessRAW(Sycamore_TrackingRecord _record)
    {
        string raw = _record.rawData;

        string[] element = raw.Split('_');
        int position = 0;
        foreach(string item in element)
        {
            switch (position)
            {
                case 0: // package id
                    _record.packageID = int.Parse(element[position]);
                    break;
                case 1: // deivce id
                    //_record.deviceID = element[position];
                    break;
                case 2:  // pid
                    _record.SupportedPid = element[position];
                    break;
                case 3:  //modele
                    //_record.modle
                    break;
                case 4:  // rpm
                    _record.vehicleRPM = CovertOBD_RPM(element[position]);
                    break;
                case 5:  // speed
                    _record.vehicleSpeed = ConvertOBD_Speed(element[position]);
                    break;
                case 6:  // oil Temp
                    _record.vehicleCoolentTemp = ConvertOBD_CoolentTemp(element[position]);
                    break;
                case 7:  // engineTemp
                    _record.vehicleEngineOilTemp = ConvertOBD_EngineOilTemp(element[position]);
                    break;
                case 8:  // battery
                    _record.vehicleBatteryVoltage = ConvertOBD_BatteryVoltage(element[position]); 
                    break;
                case 9:  // fuel
                    _record.vehicleFuelConsumption = ConvertOBD_FuelConsumption(element[position]); 
                    break;
                case 10: // mile
                    _record.vehicleClearMile = ConvertOBD_ClearMile(element[position]);
                    break;
                case 11: //engine code
                    _record.vehicleEngineCode = ConvertOBD_EngineCode(element[position]);
                    break;
                case 12:
                    //_record.vehicleRPM = 100;
                    CovertGPS(_record, element[position]);
                    break;
                default:
                    break;
            }
            position++;
        }
        return _record;
    }



    /*  GPS CONVERTION  */
    public void CovertGPS(Sycamore_TrackingRecord _record, string _gps)
    {
        string[] elements = _gps.Split(',');
        double lat;
        double lng;
        float alt;
        float spd;
        if (elements.Length > 10) {
            _record.gnssLatitude = double.TryParse(elements[3], out lat)?lat:0;
            _record.gnssLongitude = double.TryParse(elements[4], out lng) ? lng : 0;
            _record.gnssAltitude = float.TryParse(elements[5], out alt) ? alt : 0;
            _record.gnssSpeed = float.TryParse(elements[6], out spd) ? spd : 0;
            _record.gnssCourse = elements[7];
        }
    }


    /*  OBD CONVERTION  */

    // _41,0C,0C,E0,41,0C,0C,DC,

    // 010c RPM
    // Formula
    // (256 * A + B) / 4
    public int CovertOBD_RPM(string _hex)
    {
        string[] element = _hex.Split(',');
        int RPM = -100;
        if (element.Length >= 4)
        {
            int a = int.Parse(element[2], System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(element[3], System.Globalization.NumberStyles.HexNumber);
            float result = (256 * a + b) / 4;
            RPM = (int)result;
        }
        return RPM;
    }


    // 010d SPEED
    public int ConvertOBD_Speed(string _hex)
    {
        string[] element = _hex.Split(',');
        int SPEED = -100;
        if (element.Length >= 3)
        {
            SPEED = int.Parse(element[2], System.Globalization.NumberStyles.HexNumber);
        }
        return SPEED;
    }

    // 015c
    public int ConvertOBD_EngineOilTemp(string _hex)
    {
        string[] element = _hex.Split(',');
        int TEMP = -100;
        if (element.Length >= 3)
        {
            TEMP = int.Parse(element[2], System.Globalization.NumberStyles.HexNumber) - 40;
        }
        return TEMP;
    }

    // 0105
    public int ConvertOBD_CoolentTemp(string _hex)
    {
        string[] element = _hex.Split(',');
        int TEMP = -100;
        if (element.Length >= 3)
        {
            TEMP = int.Parse(element[2], System.Globalization.NumberStyles.HexNumber) - 40;
        }
        return TEMP;
    }

    // 012f
    // (100/255) * A
    public float ConvertOBD_FuelConsumption(string _hex)
    {
        string[] element = _hex.Split(',');
        float TEMP = -100;
        if (element.Length >= 3)
        {
            TEMP = (100/255) * int.Parse(element[2], System.Globalization.NumberStyles.HexNumber);
        }
        return TEMP;
    }


    //  atrv
    public float ConvertOBD_BatteryVoltage(string _hex)
    {

        float TEMP = -100;

        if (_hex != "N")
        {
            string v = _hex.ToLower().Replace("v", "");
            //TEMP = float.TryParse(v);
            float.TryParse(v, out TEMP);
        }
        return TEMP;
    }

    // 0131 
    // Mile Formula
    // 256A + B
    public int ConvertOBD_ClearMile(string _hex)
    {
        string[] element = _hex.Split(',');
        int TEMP = -100;
        if (element.Length >= 3)
        {
            int a = int.Parse(element[2], System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(element[3], System.Globalization.NumberStyles.HexNumber);

            TEMP = 256 * a + b;
        }
        return TEMP;
    }

    // 
    public string ConvertOBD_EngineCode(string _hex)
    {
        if(_hex == "N")
        {
            return "NO CODE";
        }
        else
        {
            return _hex;
        }

    }

}
