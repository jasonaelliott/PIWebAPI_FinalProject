using System;
using System.Net.Http;
using System.Configuration;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PIWebAPIFinalProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string strVerb = string.Empty;
            string strWebID = string.Empty;
            string strPITagName = string.Empty;
            string strTimestamp = string.Empty;
            string strValue = string.Empty;
            string strPIWebAPI_URL = string.Empty;
            string strJSON = string.Empty;
            string strGood = "true";                 //Hard-coded value
            string strQuestionable = "false";        //Hard-coded value

            try
            {
                //---------------- FOR FINAL PROJECT REQUIREMENTS------------------------
                //Use 'GET' VERB, Use batch controller request that utilizes ParentIDs 
                //-----------------------------------------------------------------------
                strPITagName = ConfigurationManager.AppSettings["pitagname"].ToString();
                string strWebAPI_URL = ConfigurationManager.AppSettings["piwebapiurl"].ToString();
                string strBatchWebAPI_URL = strWebAPI_URL + "/batch";
                string strBatchJSON = "{     \"1\": { \"Method\": \"GET\", \"Resource\": \"" + strWebAPI_URL + "/search/query?q=name:" + strPITagName + "\" },  \"2\": { \"Method\": \"GET\", \"Resource\": \"" + strWebAPI_URL + "/streams/{0}/value\", \"Parameters\": [ \"$.1.Content.Items[*].WebId\" ], \"ParentIds\": [ \"1\" ] }    } ";

                //************************************   BATCH  *********************************************
                Task<string> BatchTask = Task.Run(() => PostToPIWebAPI(strBatchWebAPI_URL, strBatchJSON));
                BatchTask.Wait();
                string strBatchResult = BatchTask.Result;
                //*******************************************************************************************


                                                                                                 
                strTimestamp = DateTime.Now.ToShortDateString() + " 12:00:01 AM";
                strValue = "789.45";  //Hard-coded value
                strJSON = "{  \"Timestamp\": \"" + strTimestamp + "\", \"Good\": " + strGood + ", \"Questionable\": " + strQuestionable + ", \"Value\": " + strValue + " }";

                //---------------- FOR FINAL PROJECT REQUIREMENTS------------------------
                //Use 'POST' VERB
                //-----------------------------------------------------------------------
                //************************************   POST  ************************************                
                strVerb = "POST";
                strWebID = GetPIWebAPI_WebID(strPITagName);
                strPIWebAPI_URL = GetPiWebAPI_URL(strWebID, strVerb);

                Task<string> PostTask = Task.Run(() => PostToPIWebAPI(strPIWebAPI_URL, strJSON));
                PostTask.Wait();
                string strPOSTResult = PostTask.Result;                
                //*********************************************************************************

                //---------------- FOR FINAL PROJECT REQUIREMENTS------------------------------------------
                //Use 'DELETE' VERB ('POST' action but using ?updateOption=Remove in URL to DELETE point)
                //************************************   DELETE  ******************************************
                strVerb = "DELETE";
                strWebID = GetPIWebAPI_WebID(strPITagName);
                strPIWebAPI_URL = GetPiWebAPI_URL(strWebID, strVerb);

                Task<string> DeleteTask = Task.Run(() => PostToPIWebAPI(strPIWebAPI_URL, strJSON));
                DeleteTask.Wait();
                string strDELETEResult = DeleteTask.Result;                
                //***********************************************************************************                

            }
            catch (Exception ex)
            {
                throw ex;
            }

            Environment.Exit(0);
        }

        //---------------- FOR FINAL PROJECT REQUIREMENTS------------------------
        //Generation of WebIDs
        //-----------------------------------------------------------------------
        static private string GetPIWebAPI_WebID(string strPITagName)
        {
            try
            {                
                //Declare local variables         
                string strWebID = string.Empty;                
                string strType = ConfigurationManager.AppSettings["type"].ToString();
                string strVersionNumber = ConfigurationManager.AppSettings["versionnumber"].ToString();
                string strMarker = ConfigurationManager.AppSettings["marker"].ToString();
                string strPIDataArchive = ConfigurationManager.AppSettings["pidataarchive"].ToString();

                //Encode the path as base 64
                string strPath = strPIDataArchive + "\\" + strPITagName;
                byte[] byteEncodedPath = Encoding.UTF8.GetBytes(strPath.ToUpperInvariant());
                string strEncodedPath = Convert.ToBase64String(byteEncodedPath);

                //Remove special characters from Encoded Path string
                strEncodedPath = strEncodedPath.TrimEnd(new char[] { '=' }).Replace('+', '-').Replace('/', '_');

                //Build the WebID and URL to search for a Pi Point
                strWebID = string.Format("{0}{1}{2}{3}", strType, strVersionNumber, strMarker, strEncodedPath);

                return strWebID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        static private string GetPiWebAPI_URL(string strWebID, string strVerb)
        {
            try
            {
                string strPIWebAPI_URL = string.Empty;
                string strWebAPI_URL = ConfigurationManager.AppSettings["piwebapiurl"].ToString();

                if (strWebID != string.Empty)
                {
                    if (strVerb == "POST")
                    {
                        strPIWebAPI_URL = string.Format(strWebAPI_URL + "/streams/{0}/value", strWebID);
                    }
                    else if (strVerb == "DELETE")
                    {
                        strPIWebAPI_URL = string.Format(strWebAPI_URL + "/streams/{0}/value?updateOption=Remove", strWebID);
                    }                    
                }                

                return strPIWebAPI_URL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }        

        static public async Task<string> PostToPIWebAPI(string strURL, string strJSON)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });

            StringContent content = new StringContent(strJSON, Encoding.UTF8, "application/json");
            Uri uri = new Uri(strURL);

            try
            {
                HttpResponseMessage response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }
        }        
    }
}
