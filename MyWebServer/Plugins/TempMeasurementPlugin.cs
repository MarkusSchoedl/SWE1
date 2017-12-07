using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Xml.Linq;
using System.Xml;


namespace MyWebServer
{
    /// <summary>
    /// A plugin that stores a Temperature and Date+Time in a DB.
    /// Lets you read all of the Temperature Data in a specified timespan as XML format.<para/>
    /// Also reads a Temperature sensor in the background and stores the Data in the DB.
    /// </summary>
    [AttributePlugins]
    class TempMeasurementPlugin : IPlugin
    {
        #region Fields
        /// <summary>The string how the URL has to start with we use in the Browser.</summary>
        public const string _Url = "/gettemperature/"; //Web
        /// <summary>The string how the REST URL has to start with.</summary>
        public const string _RestUrl = "/temperature/"; //Rest

        private string _ConnectionString = "Data Source=(local);Initial Catalog=MyWebServer; Integrated Security=SSPI;";
        #endregion Fields

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="TempMeasurementPlugin"/> class.
        /// Starts a thread in to read the Temperature Sensor. 
        /// </summary>
        public TempMeasurementPlugin()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ReadSensor();
            }).Start();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Returns how much the plugin wants to handle the request.
        /// If Request-URL starts with <seealso cref="_Url"/> or <seealso cref="_RestUrl"/>, the Plugin wants to handle the request. 
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>A floating point number greater than 0 and smaller or equal to 1.</returns>
        public float CanHandle(IRequest req)
        {
            if (req.Url.RawUrl.StartsWith(_Url) || req.Url.RawUrl.StartsWith(_RestUrl))
            {
                return 1.0f;
            }
            return 0.1f;
        }

        /// <summary>
        /// Depending on the Request-URL the plugin decides if the client gets REST Data or HTML Data containing the Temperature Data.
        /// </summary>
        /// <param name="req">The request the Browser/Client sent to us.</param>
        /// <returns>
        /// A response which just needs to be sent. The content of the response is either XML or HTML containing the Temperature Data.
        /// </returns>
        public IResponse Handle(IRequest req)
        {
            return req.Url.RawUrl.StartsWith(_RestUrl) ? HandleRest(req) : HandleWeb(req);
        }

        private IResponse HandleWeb(IRequest req)
        {
            Response rsp = new Response(req);

            string[] segments = req.Url.Segments;

            string[] dates = new string[2];

            //Convert date in Url to database format
            try
            {
                try
                {
                    for (int i = 0; i < 2; i++)
                    {
                        string[] nums = segments[i + 1].Split('-');
                        dates[i] = nums[2] + "-" + nums[1] + "-" + nums[0];
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    rsp.SetContent("<Info>The given Request was not valid</Info>");
                    return rsp;
                }
            }
            catch (FormatException)
            {
                rsp.SetContent("<Info>The given Request was not valid</Info>");
                return rsp;
            }

            rsp.ContentType = "text/html";
            rsp.SetContent(SqlGetRestData(dates[0], dates[1]));

            return rsp;
        }

        private IResponse HandleRest(IRequest req)
        {
            Response rsp = new Response(req);

            string[] segments = req.Url.Segments;

            string[] dates = new string[2];

            //Convert date in Url to database format
            try
            {
                try
                {
                    for (int i = 0; i < 2; i++)
                    {
                        string[] nums = segments[i + 1].Split('-');
                        dates[i] = nums[2] + "-" + nums[1] + "-" + nums[0];
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    rsp.SetContent("<Info>The given Request was not valid</Info>");
                    return rsp;
                }
            }
            catch (FormatException)
            {
                rsp.SetContent("<Info>The given Request was not valid</Info>");
                return rsp;
            }

            rsp.ContentType = "text/xml";
            rsp.SetContent(SqlGetRestData(dates[0], dates[1]));

            return rsp;
        }

        private void ReadSensor()
        {
            float oldTemp = 20f;
            while (true)
            {
                Random ran = new Random();
                double x = oldTemp + ran.NextDouble() * 2;

                Console.WriteLine("Adding Temperature " + x + "°C");
                SqlInsertTemperature(x);

                Thread.Sleep(1 * 60 * 1000); // 1 minute
            }
        }
        #endregion Methods

        #region Database
        private string SqlGetRestData(string from, string until)
        {
            string result = string.Empty;
            // Datenbankverbindung öffnen
            using (SqlConnection db = new SqlConnection(_ConnectionString))
            {
                db.Open();
                if (db.State == ConnectionState.Closed || db.State == ConnectionState.Broken)
                {
                    throw new SqlServerNotConnectedException();
                }
                else if (db.State != ConnectionState.Open)
                {
                    return string.Empty;
                }

                string query = "SELECT time, temp FROM Temperature WHERE time >= @from AND time <= @until FOR XML PATH;";
                SqlCommand cmd = new SqlCommand(query, db);

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@until", until);

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    // Daten holen
                    while (rd.Read())
                    {
                        result += rd.GetString(0);
                    }
                }
            }

            if (result != string.Empty)
            {
                return result;
            }

            return "<xml>INVALID REQUEST</xml>";
        }

        private bool SqlInsertTemperature(Double temperature)
        {
            // Datenbankverbindung öffnen
            using (SqlConnection db = new SqlConnection(_ConnectionString))
            {
                db.Open();
                temperature = Math.Round(temperature, 13, MidpointRounding.AwayFromZero);
                if (db.State == ConnectionState.Closed || db.State == ConnectionState.Broken)
                {
                    throw new SqlServerNotConnectedException();
                }
                else if (db.State != ConnectionState.Open)
                {
                    return false;
                }

                string query = "INSERT INTO Temperature(time, temp) VALUES (CURRENT_TIMESTAMP, @Temperature);"
                                + "SELECT TOP 1 temp FROM Temperature ORDER BY time DESC;";
                SqlCommand cmd = new SqlCommand(query, db);

                cmd.Parameters.AddWithValue("@Temperature", temperature);

                object row = cmd.ExecuteScalar();

                return row.Equals(temperature) ? true : false;
            }
        }
        #endregion Database
    }
}
