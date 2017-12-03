using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIF.SWE1.Interfaces;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;
using System.Xml;

namespace MyWebServer
{
    class TempMeasurementPlugin : IPlugin
    {
        #region Properties
        public const string _Url = "/gettemperature/"; //Web
        public const string _RestUrl = "/temperature/"; //Rest

        SqlConnection _Sql;
        #endregion Properties

        #region Constructor
        public TempMeasurementPlugin()
        {
            OpenSqlConnection();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                ReadSensor();
            }).Start();
        }
        #endregion Constructor

        #region Methods
        public float CanHandle(IRequest req)
        {
            if (req.Url.RawUrl.StartsWith(_Url) || req.Url.RawUrl.StartsWith(_RestUrl))
            {
                return 1.0f;
            }
            return 0.1f;
        }

        public IResponse Handle(IRequest req)
        {
            return req.Url.RawUrl.StartsWith(_RestUrl) ? HandleRest(req) : HandleWeb(req);
        }

        private IResponse HandleWeb(IRequest req)
        {
            throw new NotImplementedException();
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
                catch (ArgumentOutOfRangeException)
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
                float x = oldTemp + (float)ran.NextDouble() * 2;

                Console.WriteLine("Adding Temperature " + x + "°C");
                SqlInsertTemperature(x);

                Thread.Sleep(1 * 60 * 1000); // 1 minute
            }
        }
        #endregion Methods

        #region Database
        private void OpenSqlConnection()
        {
            string connectionString = "Data Source=(local);Initial Catalog=MyWebServer;"
                                        + "Integrated Security=SSPI;";
            _Sql = new SqlConnection(connectionString);
            _Sql.Open();
        }

        private string SqlGetRestData(string from, string until)
        {
            if (_Sql.State == ConnectionState.Closed || _Sql.State == ConnectionState.Broken)
            {
                throw new SqlServerNotConnectedException();
            }
            else if (_Sql.State != ConnectionState.Open)
            {
                return string.Empty;
            }

            string query = "SELECT time, temp FROM Temperature WHERE time >= @from AND time <= @until FOR XML PATH;";
            SqlCommand cmd = new SqlCommand(query, _Sql);

            cmd.Parameters.Add("@from", SqlDbType.Date);
            cmd.Parameters["@from"].Value = from;

            cmd.Parameters.Add("@until", SqlDbType.Date);
            cmd.Parameters["@until"].Value = until;

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return (string)(reader)[0];
            }

            return string.Empty;
        }

        private bool SqlInsertTemperature(float temperature)
        {
            if (_Sql.State == ConnectionState.Closed || _Sql.State == ConnectionState.Broken)
            {
                throw new SqlServerNotConnectedException();
            }
            else if (_Sql.State != ConnectionState.Open)
            {
                return false;
            }

            string query = "INSERT INTO Temperature(time, temp) VALUES (CURRENT_TIMESTAMP, @Temperature);";
            SqlCommand cmd = new SqlCommand(query, _Sql);

            cmd.Parameters.Add("@Temperature", SqlDbType.Float);
            cmd.Parameters["@Temperature"].Value = temperature;

            Int32 rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine("RowsAffected: {0}", rowsAffected);

            return rowsAffected > 0 ? true : false;
        }
        #endregion Database
    }
}
