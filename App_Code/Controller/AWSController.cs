using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Darkspede
{
    /// <summary>
    /// Summary description for AWSController
    /// </summary>
    public class AWSController
    {

        #region AWS Instance
        public string IdentityPoolId = Config.AWS_IdentityPoolId;
        public string CognitoIdentityRegion = RegionEndpoint.APSoutheast2.SystemName;
        public string Region = RegionEndpoint.APSoutheast2.SystemName;


        private AmazonDynamoDBConfig _configuration;

        private AmazonDynamoDBConfig Configuration
        {
            get {
                if (_configuration == null)
                    _configuration = new AmazonDynamoDBConfig {
                        ServiceURL = Config.DynamoDB_Endpoint,               
                    } ;
                return _configuration;
            }
        }




        private RegionEndpoint _CognitoIdentityRegion
        {
            get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
        }
        private RegionEndpoint _LambdaRegion
        {
            get { return RegionEndpoint.GetBySystemName(Region); }
        }

        private AWSCredentials _credentials;
        private AWSCredentials Credentials
        {
            get
            {
                if (_credentials == null)
                    _credentials = new CognitoAWSCredentials(IdentityPoolId, RegionEndpoint.APSoutheast2);
                return _credentials;
            }
        }


        public AWSController()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /* Client Intance  */
        private IAmazonLambda _lambdaClient;
        private IAmazonDynamoDB _ddbClient;
        private IAmazonS3 _s3Client;
        private IAmazonLambda Lambda_Client
        {
            get
            {
                if (_lambdaClient == null)
                {
                    _lambdaClient = new AmazonLambdaClient(Credentials, RegionEndpoint.APSoutheast2);
                }
                return _lambdaClient;
            }
        }

        private IAmazonDynamoDB Dynamo_Client
        {
            get
            {
                if (_ddbClient == null)
                {
                   _ddbClient = new AmazonDynamoDBClient(Credentials, RegionEndpoint.APSoutheast2);
                   // _ddbClient = new AmazonDynamoDBClient("","", Configuration);
                }
                return _ddbClient;
            }
        }
        private IAmazonS3 S3_Client
        {
            get
            {
                if (_s3Client == null)
                {
                    _s3Client = new AmazonS3Client(Credentials, RegionEndpoint.APSoutheast2);
                }
                //test comment
                return _s3Client;
            }
        }

        public object ScanFilter { get; private set; }


        #endregion


        // Table Operations
        #region System

        public string OnLambda_RequestSendSMS(string _message, string _target)
        {
            FCM_SMS sms = new FCM_SMS();
            sms.FCM_To = _target;
            sms.FCM_Message = _message;

            string jsonPackage = FCM_SMS.ToJson(sms);

            try
            {
                InvokeRequest Request = new InvokeRequest
                {
                    FunctionName = "OnRequestSendSMS",
                    InvocationType = InvocationType.Event,
                    Payload = jsonPackage,
                };

                InvokeResponse response = Lambda_Client.Invoke(Request);
            }
            catch (Exception e)
            {
                return "";
            }


            return jsonPackage;
        }


        public string OnDynamoDB_GetSystemConfig()
        {
            return "";
        }


        public string OnDynamoDB_UserLoginByUsername(string _username, string _password)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("username", ScanOperator.Equal, _username);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            do
            {
                documentList = search.GetNextSet();

            } while (!search.IsDone);


            if (documentList.Count != 1)
            {
                return "none";
            }

            Sycamore_User user = Sycamore_User.FromJson(documentList[0].ToJson());
            string _comparePassword;

            _comparePassword = _password;

            if (user.password == _comparePassword)
            {
                // do not return password to user
                user.password = "none";
                return Sycamore_User.ToJson(user);

            }

            return "none";

        }
        public string GetTableList()
        {
            string tableListString = "";
            foreach (string table in Config.TableList.Keys)
            {
                if(tableListString != "")
                {
                    tableListString += ",";
                }

                tableListString += Config.TableList[table];
            }
            return tableListString;
        }

        #endregion


        #region Sycamore User

        //Add new user
        public string OnDynamoDB_AddNewUser(Sycamore_User _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            Document item = Document.FromJson(Sycamore_User.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);    //if put in item success

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed: " + config.ReturnValues.ToString();
            }

        }


        //Modify user
        public string OnDynamoDB_ModifyUser(Sycamore_User _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            Document item = Document.FromJson(Sycamore_User.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }


        //Delete user by guid
        public string OnDynamoDB_DeleteUserByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            Sycamore_User item = new Sycamore_User();
            item.guid = _guid;
            Document document = Document.FromJson(Sycamore_User.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }


        //Get user by guid
        public string OnDynamoDB_GetUserByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }


        //Get all users
        public string OnDynamoDB_GetAllUsers(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }


        public string OnDynamoDB_GetAllUserFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }


            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {

                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }

            return jsonList + "]";
        }


        public List<Document> OnDynamoDB_GetAllUserDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_User"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }


        #endregion


        #region Sycamore Insight

        //Add new insight
        public string OnDynamoDB_AddNewInsight(Sycamore_Insight _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            Document item = Document.FromJson(Sycamore_Insight.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);    //if put in item success

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed: " + config.ReturnValues.ToString();
            }

        }


        //Modify insight
        public string OnDynamoDB_ModifyInsight(Sycamore_Insight _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            Document item = Document.FromJson(Sycamore_Insight.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }


        //Delete insight by guid
        public string OnDynamoDB_DeleteInsightByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            Sycamore_Insight item = new Sycamore_Insight();
            item.guid = _guid;
            Document document = Document.FromJson(Sycamore_Insight.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }


        //Get insight by guid
        public string OnDynamoDB_GetInsightByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }


        //Get all insights
        public string OnDynamoDB_GetAllInsights(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }


        public string OnDynamoDB_GetAllInsightFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }


            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {

                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }

            return jsonList + "]";
        }


        public List<Document> OnDynamoDB_GetAllInsightDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Insight"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }


        #endregion


        #region Sycamore Notification

        //Add new Notification
        public string OnDynamoDB_AddNewNotification(Sycamore_Notification _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            Document item = Document.FromJson(Sycamore_Notification.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);    //if put in item success

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed: " + config.ReturnValues.ToString();
            }

        }


        //Modify Notification
        public string OnDynamoDB_ModifyNotification(Sycamore_Notification _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            Document item = Document.FromJson(Sycamore_Notification.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }


        //Delete Notification by guid
        public string OnDynamoDB_DeleteNotificationByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            Sycamore_Notification item = new Sycamore_Notification();
            item.guid = _guid;
            Document document = Document.FromJson(Sycamore_Notification.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }


        //Get Notification by guid
        public string OnDynamoDB_GetNotificationByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }


        //Get all Notifications
        public string OnDynamoDB_GetAllNotifications(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }


        public string OnDynamoDB_GetAllNotificationFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }


            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {

                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }

            return jsonList + "]";
        }


        public List<Document> OnDynamoDB_GetAllNotificationDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Notification"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }


        #endregion


        #region Sycamore Report

        //Add new Report
        public string OnDynamoDB_AddNewReport(Sycamore_Report _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            Document item = Document.FromJson(Sycamore_Report.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);    //if put in item success

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed: " + config.ReturnValues.ToString();
            }

        }


        //Modify Report
        public string OnDynamoDB_ModifyReport(Sycamore_Report _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            Document item = Document.FromJson(Sycamore_Report.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }


        //Delete Report by guid
        public string OnDynamoDB_DeleteReportByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            Sycamore_Report item = new Sycamore_Report();
            item.guid = _guid;
            Document document = Document.FromJson(Sycamore_Report.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }


        //Get Report by guid
        public string OnDynamoDB_GetReportByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }


        //Get all Reports
        public string OnDynamoDB_GetAllReports(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }


        public string OnDynamoDB_GetAllReportFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }


            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {

                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }

            return jsonList + "]";
        }


        public List<Document> OnDynamoDB_GetAllReportDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Report"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }


        #endregion
        /*
        #region Fleet_Report
        public string OnDynamoDB_AddNewReport(Sycamore_Report _item)
        {

            Table table = Table.LoadTable(Dyanmo_Client, Config.Site + Config.TableList["Fleet_Report"]);
            Document item = Document.FromJson(Sycamore_Report.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool reuslt = table.TryPutItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_UpdateReport(Sycamore_Report _item)
        {
            Table table = Table.LoadTable(Dyanmo_Client, Config.Site + Config.TableList["Fleet_Report"]);
            Document item = Document.FromJson(Sycamore_Report.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_GetReportByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dyanmo_Client, Config.Site + Config.TableList["Fleet_Report"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }


        public string OnDynamoDB_GetAllReports(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dyanmo_Client, Config.Site + Config.TableList["Fleet_Report"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {

                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }

            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllReportsDocument(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dyanmo_Client, Config.Site + Config.TableList["Fleet_Report"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList.AddRange(search.GetNextSet());
            }

            return documentList;
        }

        public string OnDynamoDB_RemoveReportByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dyanmo_Client, Config.Site + Config.TableList["Fleet_Report"]);
            Sycamore_Report Report = new Sycamore_Report();
            Report.guid = _guid;
            Document item = Document.FromJson(Sycamore_Report.ToJson(Report));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(item, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }


        }

        #endregion
        */


        #region Sycamore Operation

        public string OnDynamoDB_AddNewOperation(Sycamore_Operation _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            Document item = Document.FromJson(Sycamore_Operation.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteOperationByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            Sycamore_Operation item = new Sycamore_Operation
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Operation.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetOperationByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            Document document = table.GetItem(_guid);

            if(document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllOperations(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllOperationFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }



            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllOperationDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyOperation(Sycamore_Operation _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation"]);
            Document item = Document.FromJson(Sycamore_Operation.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion

        #region Sycamore Operation Request

        public string OnDynamoDB_AddNewOperationRequest(Sycamore_Operation_Request _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            Document item = Document.FromJson(Sycamore_Operation_Request.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteOperationRequestByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            Sycamore_Operation_Request item = new Sycamore_Operation_Request
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Operation_Request.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetOperationRequestByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllOperationRequests(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllOperationRequestFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }



            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllOperationRequestDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyOperationRequest(Sycamore_Operation_Request _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Operation_Request"]);
            Document item = Document.FromJson(Sycamore_Operation_Request.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion

        #region Sycamore Driver

        public string OnDynamoDB_AddNewDriver(Sycamore_Driver _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            Document item = Document.FromJson(Sycamore_Driver.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteDriverByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            Sycamore_Driver item = new Sycamore_Driver
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Driver.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetDriverByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllDrivers(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllDriverFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllDriverDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyDriver(Sycamore_Driver _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Driver"]);
            Document item = Document.FromJson(Sycamore_Driver.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore DriverRate

        public string OnDynamoDB_AddNewDriverRate(Sycamore_DriverRate _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            Document item = Document.FromJson(Sycamore_DriverRate.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteDriverRateByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            Sycamore_DriverRate item = new Sycamore_DriverRate
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_DriverRate.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetDriverRateByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllDriverRates(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllDriverRateFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllDriverRateDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyDriverRate(Sycamore_DriverRate _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_DriverRate"]);
            Document item = Document.FromJson(Sycamore_DriverRate.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore Group

        public string OnDynamoDB_AddNewGroup(Sycamore_Group _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            Document item = Document.FromJson(Sycamore_Group.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteGroupByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            Sycamore_Group item = new Sycamore_Group
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Group.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetGroupByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllGroups(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllGroupFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllGroupDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyGroup(Sycamore_Group _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Group"]);
            Document item = Document.FromJson(Sycamore_Group.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore Fleet

        public string OnDynamoDB_AddNewFleet(Sycamore_Fleet _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            Document item = Document.FromJson(Sycamore_Fleet.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteFleetByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            Sycamore_Fleet item = new Sycamore_Fleet
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Fleet.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetFleetByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllFleets(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllFleetFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllFleetDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyFleet(Sycamore_Fleet _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Fleet"]);
            Document item = Document.FromJson(Sycamore_Fleet.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore Vehicle

        public string OnDynamoDB_AddNewVehicle(Sycamore_Vehicle _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            Document item = Document.FromJson(Sycamore_Vehicle.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteVehicleByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            Sycamore_Vehicle item = new Sycamore_Vehicle
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Vehicle.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetVehicleByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllVehicles(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllVehicleFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllVehicleDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyVehicle(Sycamore_Vehicle _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Vehicle"]);
            Document item = Document.FromJson(Sycamore_Vehicle.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore Document

        public string OnDynamoDB_AddNewDocument(Sycamore_Document _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            Document item = Document.FromJson(Sycamore_Document.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteDocumentByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            Sycamore_Document item = new Sycamore_Document
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Document.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_ModifyDocument(Sycamore_Document _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            Document item = Document.FromJson(Sycamore_Document.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_GetDocumentByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllDocuments(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllDocumentFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllDocumentDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Document"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        #endregion


        #region Sycamore Service

        public string OnDynamoDB_AddNewService(Sycamore_Service _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            Document item = Document.FromJson(Sycamore_Service.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteServiceByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            Sycamore_Service item = new Sycamore_Service
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_Service.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_ModifyService(Sycamore_Service _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            Document item = Document.FromJson(Sycamore_Service.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_GetServiceByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllServices(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllServiceFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllServiceDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_Service"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        #endregion


        #region Sycamore ServiceProvider

        public string OnDynamoDB_AddNewServiceProvider(Sycamore_ServiceProvider _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            Document item = Document.FromJson(Sycamore_ServiceProvider.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteServiceProviderByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            Sycamore_ServiceProvider item = new Sycamore_ServiceProvider
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_ServiceProvider.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetServiceProviderByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllServiceProviders(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllServiceProviderFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllServiceProviderDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyServiceProvider(Sycamore_ServiceProvider _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_ServiceProvider"]);
            Document item = Document.FromJson(Sycamore_ServiceProvider.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore TrackingRecord

        public string OnDynamoDB_AddNewTrackingRecord(Sycamore_TrackingRecord _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            Document item = Document.FromJson(Sycamore_TrackingRecord.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteTrackingRecordByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            Sycamore_TrackingRecord item = new Sycamore_TrackingRecord
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_TrackingRecord.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetTrackingRecordByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllTrackingRecords(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllTrackingRecordFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllTrackingRecordDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyTrackingRecord(Sycamore_TrackingRecord _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingRecord"]);
            Document item = Document.FromJson(Sycamore_TrackingRecord.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore TrackingPackage

        public string OnDynamoDB_AddNewTrackingPackage(Sycamore_TrackingPackage _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            Document item = Document.FromJson(Sycamore_TrackingPackage.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteTrackingPackageByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            Sycamore_TrackingPackage item = new Sycamore_TrackingPackage
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_TrackingPackage.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetTrackingPackageByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllTrackingPackages(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllTrackingPackageFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllTrackingPackageDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyTrackingPackage(Sycamore_TrackingPackage _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_TrackingPackage"]);
            Document item = Document.FromJson(Sycamore_TrackingPackage.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Sycamore OperationQR

        public string OnDynamoDB_AddNewOperationQR(Sycamore_OperationQR _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            Document item = Document.FromJson(Sycamore_OperationQR.ToJson(_item));
            PutItemOperationConfig config = new PutItemOperationConfig();
            bool result = table.TryPutItem(item, config);

            if (result)
            {
                return _item.guid;
            }
            else
            {
                return "Failed" + config.ReturnValues.ToString();
            }
        }

        public string OnDynamoDB_DeleteOperationQRByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            Sycamore_OperationQR item = new Sycamore_OperationQR
            {
                guid = _guid
            };
            Document document = Document.FromJson(Sycamore_OperationQR.ToJson(item));
            DeleteItemOperationConfig config = new DeleteItemOperationConfig();
            bool result = table.TryDeleteItem(document, config);

            if (result)
            {
                return "success";
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetOperationQRByGuid(string _guid)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            Document document = table.GetItem(_guid);

            if (document != null)
            {
                return document.ToJson();
            }
            else
            {
                return "none";
            }
        }

        public string OnDynamoDB_GetAllOperationQRs(string _tableName, string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("tableName", ScanOperator.Equal, _tableName);
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> docuementList = new List<Document>();

            string jsonList = "[";  //Empty json list
            //string jsonList = "none";  //Empty json list
            int index = 0;

            while (!search.IsDone)    //Still searching
            {
                docuementList = search.GetNextSet();

                foreach (var doc in docuementList)
                {
                    if (index != 0)    //first item in documentlist
                    {
                        jsonList += "," + doc.ToJson();
                    }
                    else
                    {
                        jsonList += doc.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
            //return jsonList;
        }

        public string OnDynamoDB_GetAllOperationQRFilters(Dictionary<string, string> _filterPair)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            ScanFilter scanFilter = new ScanFilter();

            foreach (KeyValuePair<string, string> pair in _filterPair)
            {
                if (pair.Value != "N")
                {
                    scanFilter.AddCondition(pair.Key, ScanOperator.Equal, pair.Value);
                }
            }

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            string jsonList = "[";
            int index = 0;

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();

                foreach (var document in documentList)
                {
                    if (index != 0)
                    {
                        jsonList = jsonList + "," + document.ToJson();
                    }
                    else
                    {
                        jsonList = jsonList + document.ToJson();
                    }
                    index++;
                }
            }
            return jsonList + "]";
        }

        public List<Document> OnDynamoDB_GetAllOperationQRDocuments(string _condition, string _value)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition(_condition, ScanOperator.Equal, _value);

            Search search = table.Scan(scanFilter);

            List<Document> documentList = new List<Document>();

            while (!search.IsDone)
            {
                documentList = search.GetNextSet();
            }

            return documentList;
        }

        public string OnDynamoDB_ModifyOperationQR(Sycamore_OperationQR _item)
        {
            Table table = Table.LoadTable(Dynamo_Client, Config.Site + Config.TableList["Sycamore_OperationQR"]);
            Document item = Document.FromJson(Sycamore_OperationQR.ToJson(_item));
            UpdateItemOperationConfig config = new UpdateItemOperationConfig();
            bool reuslt = table.TryUpdateItem(item, config);

            if (reuslt)
            {
                return _item.guid;
            }
            else
            {
                return config.ReturnValues.ToString();
            }
        }

        #endregion


        #region Serializable
        [Serializable]
        public class LambdaResult
        {
            public string result = "fail";
            public string message = "";
            public int count;
        }

        [Serializable]
        public class SingleGuid
        {
            public string guid = "";
        }

        [Serializable]
        public class SingleToken
        {
            public string guid = "";
            public string token = "";
        }
        #endregion
    }
}