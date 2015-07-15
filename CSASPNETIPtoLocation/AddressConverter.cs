using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AddressTransfer
{
    class AddressConverter
    {
        private Dictionary<String,GeoCode> recorder;
        public int indexLoader { get; set; }

        static void Main(string[] args)
        {
            String address = String.Empty;
            StreamReader file = new StreamReader("address.txt", System.Text.Encoding.Default);
            StreamWriter output = new StreamWriter("LatandLng.txt");
            AddressConverter ac = new AddressConverter();
            int index = 0;
            
            while((address = file.ReadLine()) != null)
            {
                GeoCode geoCode = null;
                index++;

                if (ac.isNew(address))
                {
                    WebResponse response = ac.getResponse(address);
                    geoCode = ac.xmlParser(response);
                    ac.saveRecord(address,geoCode);

                    Thread.Sleep(1000);
                }
                else
                {
                    geoCode = ac.getRecord(address);
                }

                String res = address + " , " + geoCode.Lat + " , " + geoCode.Lng;
                Console.WriteLine("Case #" + index + ":");
                Console.WriteLine(res);
                output.WriteLine(res);
            }

        }

        public AddressConverter()
        {
            try
            {
                StreamReader file = new StreamReader("load.txt");
                indexLoader = Convert.ToInt32(file.ReadLine());
            }
            catch
            {
                Console.WriteLine("未能讀取load.txt，程式將從文件第1行開始轉換");
                indexLoader = 0;
            }
            finally
            {
                recorder = new Dictionary<String, GeoCode>();
            }
        }

        public WebResponse getResponse(String address)
        {
            //XML
            String url = String.Format("HTTP://maps.google.com/maps/api/geocode/xml?sensor=false&address={0}", address);
            
            /*
             * json
            String url = String.Format("HTTP://maps.google.com/maps/api/geocode/json?sensor=false&address={0}", address);
             */
            
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            return request.GetResponse();
        }

        public GeoCode xmlParser(WebResponse response)
        {
            XDocument xdoc = XDocument.Load(response.GetResponseStream());

            XElement result = xdoc.Element("GeocodeResponse").Element("result");
            XElement locationElement = result.Element("geometry").Element("location");

            String lat = locationElement.Element("lat").Value;
            String lng = locationElement.Element("lng").Value;

            return new GeoCode(lat,lng);
        }

        public void jspnParser(WebResponse response)
        {

        }

        public GeoCode getRecord(String address)
        {
            return recorder[address];
        }

        public void saveRecord(String address,GeoCode geoCode)
        {
            recorder.Add(address, geoCode);
        }

        public Boolean isNew(String address)
        {
            return !recorder.ContainsKey(address);
        }
    }

    class GeoCode
    {
        public GeoCode(String lat,String lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public String Lat { get; set; }
        public String Lng { get; set; }
    }
}
