using System;
using MyWebServer;
using MyWebServer.Plugins;
using NUnit.Framework;
using System.Linq;
using BIF.SWE1.Interfaces;
using System.Text;
using System.IO;
using System.Threading;

namespace MyUnitTests
{
    [TestFixture]
    public class MyUnitTests
    {
        private IRequest GetRequest(string rawurl, string content)
        {
            MemoryStream stream = new MemoryStream();
            var sw = new StreamWriter(stream, Encoding.UTF8);

            sw.WriteLine("GET " + rawurl + " HTTP/1.1");
            sw.WriteLine("Host: localhost:8080");
            sw.WriteLine("content-length: " + content.Length);
            sw.WriteLine();
            sw.WriteLine(content);
            sw.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var req = new Request(stream);

            return req;
        }

        // Function from Arthur Zaczek!
        private StringBuilder GetBody(IResponse resp)
        {
            StringBuilder body = new StringBuilder();
            using (var ms = new MemoryStream())
            {
                resp.Send(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(ms);
                while (!sr.EndOfStream)
                {
                    body.AppendLine(sr.ReadLine());
                }
            }
            return body;
        }

        [Test]
        public void Url_Segments_Shouldnt_Contain_Parameters()
        {
            Url url = new Url("localhost:8080/testfolder/testfolder?ShoudlntContain=true");
            Assert.That(url, Is.Not.Null, "Url returned null");
            Assert.That(url.Segments.Count(), Is.EqualTo(2));
            for (int i = 0; i < 2; i++)
            {
                Assert.That(url.Segments[i], Does.Not.Contain("ShoudlntContain"));
            }
        }

        [Test]
        public void Lower_Plugin_Handle_SpecialChars()
        {
            var plugin = new ToLowerPlugin();
            Assert.That(plugin, Is.Not.Null, "ToLowerPlugin returned null");

            var url = plugin.Url;
            Assert.That(url, Is.Not.Null, "ToLowerUrl returned null");

            string textToTest = string.Format("Hello M4RKU$! äöüÄÖÜ!§$%&/()=\\\"`´²³ {0}", Guid.NewGuid());

            var req = GetRequest(url, "text=" + textToTest);

            Assert.That(req, Is.Not.Null, "IUEB6.GetRequest returned null");

            Assert.That(plugin.CanHandle(req), Is.GreaterThan(0).And.LessThanOrEqualTo(1));

            var resp = plugin.Handle(req);
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.StatusCode, Is.EqualTo(200));
            Assert.That(resp.ContentLength, Is.GreaterThan(0));

            StringBuilder body = GetBody(resp);
            Assert.That(body.ToString(), Does.Contain(textToTest.ToLower()));
        }

        [Test]
        public void MyMutex_Should_Work()
        {
            var mymutex = new MyMutex();

            Assert.That(mymutex.IsLocked, Is.False);
            Assert.That(mymutex.TryWait(), Is.True);
            Assert.That(mymutex.TryWait(), Is.False);
            Assert.That(mymutex.IsLocked, Is.True);
            mymutex.Release();
            Assert.That(mymutex.IsLocked, Is.False);
        }

        [Test]
        public void NaviPlugin_Should_Find_Street_Without_Parsing()
        {
            var plugin = new NavigationPlugin();

            var req = GetRequest(NavigationPlugin._Url + "?Update=false", "street=Höchstädtplatz");
            var rsp = (Response)plugin.Handle(req);
            var content = GetBody(rsp).ToString();
            bool success = false;
            for (int i = 1; i < 20; i++)
            {
                if (content.Contains(i + " Orte gefunden"))
                {
                    success = true;
                }
            }
            Assert.That(success, Is.True);
        }

        [Test]
        public void NaviPlugin_Should_Parse_OSM_File_And_Find_Street()
        {
            var plugin = new NavigationPlugin();

            var req = GetRequest(NavigationPlugin._Url + "?Update=true", "");

            Response rsp = (Response)plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("<div>Erfolgreiches Update</div>"));

            req = GetRequest(NavigationPlugin._Url + "?Update=false", "street=Höchstädtplatz");
            rsp = (Response)plugin.Handle(req);
            content = GetBody(rsp).ToString();
            bool success = false;
            for (int i = 1; i < 20; i++)
            {
                if (content.Contains(i + " Orte gefunden"))
                {
                    success = true;
                }
            }
            Assert.That(success, Is.True);
        }

        [Test]
        public void NaviPlugin_Shouldnt_Parse_Parallel()
        {
            var plugin = new NavigationPlugin();

            var req = GetRequest(NavigationPlugin._Url + "?Update=true", "");

            Response rsp = new Response();

            Thread thread1 = new Thread(() => { plugin.Handle(req); });
            Thread thread2 = new Thread(() => { rsp = (Response)plugin.Handle(req); });

            thread1.Start();
            Thread.Sleep(50); // depends on how much time the parsing takes
            thread2.Start();

            thread2.Join();
            thread1.Abort();

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("Das NavigationPlugin kann diese Funktion zurzeit nicht ausführen, sie wird bereits benutzt. Bitte versuchen Sie es später noch einmal"));
        }

        [Test]
        public void TempPlugin_Should_Handle_With_DB_And_No_Page()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "2014-01-01/2014-01-02", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Not.Contain("<ul><li>This Page is empty.</ul></li>"));
        }

        [Test]
        public void TempPlugin_Should_Handle_With_DB_And_With_Page()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "2014-01-01/2014-01-02?page=1", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Not.Contain("<ul><li>This Page is empty.</ul></li>"));
        }

        [Test]
        public void TempPlugin_Should_Handle_Invalid_PageNum()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "2014-01-01/2014-01-02?page=-32", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("<ul><li>The page cannot be below one.</li></ul>"));
        }

        [Test]
        public void TempPlugin_Rest_Should_Handle_Invalid_Dates()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._RestUrl + "201a-01-01", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("The given date wasnt valid"));
        }

        [Test]
        public void TempPlugin_Web_Should_Handle_Invalid_Dates()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "201a-01-01/2013-01-01", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("The given date wasnt valid"));
        }

        [Test]
        public void TempPlugin_Rest_Should_Handle_Date_OutOfRange()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._RestUrl + "2014-13-20", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("The given date wasnt valid"));
        }

        [Test]
        public void TempPlugin_Web_Should_Handle_Date_OutOfRange()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "2014-13-20/2013-12-32", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("The given date wasnt valid"));
        }

        [Test]
        public void TempPlugin_Web_Should_Handle_NoDateRange()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "2014-06-12/2014-06-10", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("not a valid Date-Range"));
        }

        [Test]
        public void TempPlugin_Connect_To_DB()
        {
            var plugin = new TempMeasurementPlugin();

            var req = GetRequest(TempMeasurementPlugin._Url + "2014-06-12/2014-06-12", "");

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Not.Contain("DB-Connection has failed"));
        }

        [Test]
        public void Url_Does_Handle_Everything()
        {
            Url url = new Url("/testfolder/testfolder/testfile#TestAnker?testing=true&didWork=true");

            Assert.That(url.Segments.Count(), Is.EqualTo(3));
            Assert.That(url.Segments[0], Is.EqualTo("testfolder"));
            Assert.That(url.Segments[1], Is.EqualTo("testfolder"));
            Assert.That(url.Segments[2], Is.EqualTo("testfile"));

            Assert.That(url.Fragment, Is.EqualTo("TestAnker"));

            Assert.That(url.Parameter.Count, Is.EqualTo(2));
            Assert.That(url.Parameter, Does.ContainKey("testing"));
            Assert.That(url.Parameter, Does.ContainKey("didWork"));
            Assert.That(url.Parameter["testing"], Is.EqualTo("true"));
            Assert.That(url.Parameter["didWork"], Is.EqualTo("true"));
        }

        [Test]
        public void NaviPlugin_Handle_Wrong_Content()
        {
            var req = GetRequest(NavigationPlugin._Url, "wrongtag=somestuff");
            var plugin = new NavigationPlugin();

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("Bitte geben Sie eine Anfrage ein"));
        }

        [Test]
        public void NaviPlugin_Handle_No_Street_Found()
        {
            var req = GetRequest(NavigationPlugin._Url, "street=NonExistingStreet");
            var plugin = new NavigationPlugin();

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("0 Orte gefunden"));
        }

        [Test]
        public void NaviPlugin_Returns_Test_Data()
        {
            var req = GetRequest(NavigationPlugin._Url + "?test=1", "street=NonExistingStreet");
            var plugin = new NavigationPlugin();

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("This is Test-Data"));
            Assert.That(content, Does.Contain("Wiener Neustadt"));
            Assert.That(content, Does.Contain("Klosterneuburg"));
            Assert.That(content, Does.Contain("Wien"));
        }

        [Test]
        public void NaviPlugin_Handle_Wrong_Parameters()
        {
            var req = GetRequest(NavigationPlugin._Url + "?test=2", "street=NonExistingStreet");
            var plugin = new NavigationPlugin();

            var rsp = plugin.Handle(req);

            var content = GetBody(rsp).ToString();
            Assert.That(content, Does.Contain("0 Orte gefunden"));
        }
    }
}
