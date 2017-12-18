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
using MyWebServer.MyExceptions;


namespace MyWebServer.Plugins
{
    /// <summary>
    /// A plugin that stores a Temperature and Date+Time in a DB.
    /// Lets you read all of the Temperature Data in a specified timespan as XML format.<para/>
    /// Also reads a Temperature sensor in the background and stores the Data in the DB.
    /// </summary>
    [AttributePlugins]
    public class TempMeasurementPlugin : IPlugin
    {
        #region Fields
        /// <summary>The string how the URL has to start with we use in the Browser.</summary>
        public const string _Url = "/temperature/"; //Web
        /// <summary>The string how the REST URL has to start with.</summary>
        public const string _RestUrl = "/gettemperature/"; //Rest

        private string _ConnectionString = "Data Source=(local);Initial Catalog=MyWebServer; Integrated Security=SSPI;";

        private const float _CanHandleReturn = 1.0f;
        private const float _CannotHandleReturn = 0.1f;
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
            if (req.Url.Path.StartsWith(_Url) || req.Url.Path.StartsWith(_RestUrl))
            {
                return _CanHandleReturn;
            }
            return _CannotHandleReturn;
        }

        /// <summary>
        /// Depending on the Request-URL the plugin decides if the client gets REST Data or HTML Data containing the Temperature Data.
        /// </summary>
        /// <param name="req">
        /// The request the Browser/Client sent to us.
        /// Adding "?test=1" at the end of the URL returns a test-response without accessing the database.
        /// </param>
        /// <returns>
        /// A response which just needs to be sent. The content of the response is either XML or HTML containing the Temperature Data.
        /// </returns>
        public IResponse Handle(IRequest req)
        {
            // For testing the plugin without database connection
            if (req.Url.ParameterCount > 0 && req.Url.Parameter.Contains(new KeyValuePair<string, string>("test", "1")))
            {
                Response rsp = new Response(req);
                rsp.StatusCode = 200;
                rsp.SetContent("<row><time>2017-12-11T13:14:36.093</time><temp>2.118251151926000e+001</temp></row><row><time>2017-12-11T13:14:36.087</time><temp>2.046490146520780e+001</temp></row><row><time>2017-12-11T13:14:36.073</time><temp>2.174729141115550e+001</temp></row>");
                rsp.ContentType = req.Url.Path.StartsWith(_RestUrl) ? "text/xml" : "text/html";
                return rsp;
            }
            return req.Url.Path.StartsWith(_RestUrl) ? HandleRest(req) : HandleWeb(req);
        }

        private IResponse HandleWeb(IRequest req)
        {
            Response rsp = new Response(req);
            rsp.StatusCode = 200;
            string content = string.Empty;

            string[] dates = new string[2];

            //Convert date in Url to database format
            try
            {
                try
                {
                    for (int i = 0; i < 2; i++)
                    {
                        string[] nums = req.Url.Segments[i + 1].Split('-');
                        dates[i] = nums[2] + "-" + nums[1] + "-" + nums[0];
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    rsp.SetContent("<table><tr><th>The given Request was not valid</th></tr></table>");
                    return rsp;
                }
            }
            catch (FormatException)
            {
                rsp.SetContent("<table><tr><th>The given Request was not valid</th></tr></table>");
                return rsp;
            }

            // Check if its a valid date
            DateTime[] dateTime = new DateTime[2];
            for (int i = 0; i < 2; i++)
            {
                if (!DateTime.TryParse(dates[i], out dateTime[i]))
                {
                    rsp.SetContent("<table><tr><th>The given date wasnt valid</th></tr><tr><td>Format: YYYY-MM-DD</td></tr></table>");
                    return rsp;
                }
            }

            //Check if date range is valid
            if (dateTime[0] > dateTime[1])
            {
                rsp.SetContent("<table><tr><th>The given dates were not valid</th></tr><tr><td>There was not a valid Date-Range</td></tr></table>");
                return rsp;
            }

            // Check if a page was set correctly
            int page = 0;
            if (req.Url.Parameter.Count() == 1 && req.Url.Parameter.ContainsKey("page") && !string.IsNullOrEmpty(req.Url.Parameter["page"]))
            {
                if (!Int32.TryParse(req.Url.Parameter["page"], out page))
                {
                    page = 1;
                }
            }
            else
            {
                page = 1;
            }

            rsp.ContentType = "text/html";
            rsp.SetContent(SqlGetHTMLData(dates[0], dates[1], page));

            return rsp;
        }

        private IResponse HandleRest(IRequest req)
        {
            Response rsp = new Response(req);
            rsp.StatusCode = 400;

            string date = string.Empty;

            //Containing "/temperature/[YYYY]-[DD]-[MM]/" .. The slash at the end doesnt bother us, if nothing comes after it
            if (req.Url.Segments.Count() == 3 && !string.IsNullOrEmpty(req.Url.Segments[2]))
            {
                rsp.SetContent("<Info>The given Request was not valid</Info>");
                return rsp;
            }

            //Convert date in Url to database format
            try
            {
                try
                {
                    string[] nums = req.Url.Segments[1].Split('-');
                    date = nums[2] + "-" + nums[1] + "-" + nums[0];
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

            // Check if its a valid date
            DateTime temp;
            if (!DateTime.TryParse(date, out temp))
            {
                rsp.SetContent("<table><tr><th>The given date wasnt valid</th></tr><tr><td>Format: YYYY-MM-DD</td></tr></table>");
                return rsp;
            }

            rsp.StatusCode = 200;
            rsp.ContentType = "text/xml";
            rsp.SetContent(SqlGetRestData(date));

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
        private string SqlGetRestData(string date)
        {
            string result = string.Empty;
            // Datenbankverbindung öffnen
            using (SqlConnection db = new SqlConnection(_ConnectionString))
            {
                try
                {
                    db.Open();
                }
                catch (Exception)
                {
                    return "<Info>The DB-Connection has failed.</Info>";
                }

                if (db.State == ConnectionState.Closed || db.State == ConnectionState.Broken)
                {
                    throw new SqlServerNotConnectedException();
                }
                else if (db.State != ConnectionState.Open)
                {
                    return string.Empty;
                }

                string query = "SELECT time, temp FROM Temperature WHERE CAST(time as date) = @date FOR XML PATH;";
                SqlCommand cmd = new SqlCommand(query, db);

                cmd.Parameters.AddWithValue("@date", date);

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
                try
                {
                    db.Open();
                }
                catch (Exception)
                {
                    Console.WriteLine("TempPlugin: Couldnt Connect to DB!");
                    return false;
                }
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

        private string SqlGetHTMLData(string from, string until, int page)
        {
            if (page <= 0)
            {
                return "<ul><li>The page cannot be below one.</li></ul>";
            }

            string result = string.Empty;
            // Datenbankverbindung öffnen
            using (SqlConnection db = new SqlConnection(_ConnectionString))
            {
                try
                {
                    db.Open();
                }
                catch (Exception)
                {
                    return "<ul><li>The DB-Connection has failed.</ul></li>";
                }
                if (db.State == ConnectionState.Closed || db.State == ConnectionState.Broken)
                {
                    throw new SqlServerNotConnectedException();
                }
                else if (db.State != ConnectionState.Open)
                {
                    return string.Empty;
                }

                int rowFrom, rowTo;
                rowFrom = 10 * (page - 1) + 1; // 1, 11, 21, 41
                rowTo = 10 * (page); // 10, 20, 30, 40

                //we cast our column "time" to date so the time saved in there doesnt matter for the outcome
                string query =
                      "SELECT FORMAT(time, 'dd.MM.yyyy HH:mm', 'de-de') as time, CAST(CONVERT(decimal(15,3), temp) AS VARCHAR) + ' °C' AS temp "
                    + "FROM( "
                        + "SELECT time, temp, ROW_NUMBER() OVER(ORDER BY time) AS RowNum "
                        + "FROM Temperature "
                        + "WHERE CAST(time as date) >= @from AND CAST(time as date) <= @until "
                    + ") AS MyDerivedTable "
                    + "WHERE MyDerivedTable.RowNum BETWEEN @startRow AND @endRow "
                    + "FOR XML PATH; ";

                SqlCommand cmd = new SqlCommand(query, db);

                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@until", until);
                cmd.Parameters.AddWithValue("@startRow", rowFrom);
                cmd.Parameters.AddWithValue("@endRow", rowTo);

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
                result = "<table><tr><th>Time</th><th>Temp</th></tr>" + result + "</table>";
                result = result.Replace("row", "tr");
                result = result.Replace("time", "td");
                result = result.Replace("temp", "td");

                Console.Write("\n\n" + result + "\n\n");
                return result;
            }

            return "<ul><li>This Page is empty.</ul></li>";
        }
        #endregion Database
    }
}
