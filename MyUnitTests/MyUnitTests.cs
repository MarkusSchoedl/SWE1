using System;
using MyWebServer;
using MyWebServer.Plugins;
using NUnit.Framework;
using System.Linq;
using BIF.SWE1.Interfaces;
using System.Text;
using System.IO;

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
        public void UrlSegmentsShouldntContainParameters()
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
        public void LowerPluginHandleSpecialChars()
        {
            var plugin = new ToLowerPlugin();
            Assert.That(plugin, Is.Not.Null, "ToLowerPlugin returned null");

            var url = plugin.Url;
            Assert.That(url, Is.Not.Null, "ToLowerUrl returned null");

            string textToTest = string.Format("Hello M4RKU$! äöüÄÖÜ!§$%&/()=\\\"`´²³ {0}", Guid.NewGuid());
            
            var req = GetRequest(url, "text="+textToTest);

            Assert.That(req, Is.Not.Null, "IUEB6.GetRequest returned null");

            Assert.That(plugin.CanHandle(req), Is.GreaterThan(0).And.LessThanOrEqualTo(1));

            var resp = plugin.Handle(req);
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.StatusCode, Is.EqualTo(200));
            Assert.That(resp.ContentLength, Is.GreaterThan(0));

            StringBuilder body = GetBody(resp);
            Assert.That(body.ToString(), Does.Contain(textToTest.ToLower()));
        }
    }
}
