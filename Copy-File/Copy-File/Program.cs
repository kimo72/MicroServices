using LukeSkywalker.IPNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;



namespace google
{
    class Program
    {

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint DestIP, uint SrcIP, byte[] pMacAddr, ref int PhyAddrLen);



        static void Main(string sourcefolder, string destination)
        {

            

          



            ////////////////////////////////////////////////////////////

            System.Diagnostics.Process.Start("arp.exe", "/d");


            //get Ip 
            string serverIP;
            serverIP = GetLocalIPAddress();

            //Get the list of machines in the subnet
            IPNetwork startAndEndIPs = IPNetwork.Parse(serverIP);
            List<string> listOfMachinesInSubnet = getsubnetMachines(startAndEndIPs.FirstUsable.ToString(), startAndEndIPs.LastUsable.ToString());

            //// get machines arp
            Dictionary<string, string> listOfDevicesInTheSubnet = SendArpRequest(listOfMachinesInSubnet);

            Console.WriteLine("discover complete");
            foreach (var item in listOfDevicesInTheSubnet.Keys)
            {
                System.IO.File.AppendAllText("c:\\listOfDevicesInTheSubnet.txt", item);
            }



            //NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            Dictionary<string, string> listOfMachinesInTheSubnet = resolveName(listOfDevicesInTheSubnet);


            foreach (var item in listOfMachinesInTheSubnet.Keys)
            {
                System.IO.File.AppendAllText("c:\\listOfMachinesInTheSubnet.txt", item);
            }
            Console.WriteLine("discover filter complete");

            Console.WriteLine("D:\\Herramientas_Comerciales");
            Console.WriteLine("\\D$\\Mis documentos\\Herramientas_Comerciales");
            Console.ReadKey();
            foreach(var machine in listOfMachinesInTheSubnet.Keys)
            {
                string vervose = copyFiles("D:\\Herramientas_Comerciales", machine, "\\D$\\Mis documentos\\Herramientas_Comerciales");
                Console.WriteLine(vervose);
                Console.ReadKey();
                System.IO.File.AppendAllText("c:\\vervose.txt", vervose);

            }






            Console.ReadLine();

            //GetSubnetMask();


        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                //selectes the ip of the non backup network adapter 10.11.11.1
                if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString() != "10.11.11.1")
                {
                    returnIp = ip.ToString();
                    return returnIp;

                }
            }
            throw new Exception("Local IP Address Not Found!");
        }







        public static Dictionary<string, string> filtermachines(params string[] ipList)
        {

            Dictionary<string, string> dictionary = new Dictionary<string, string>();


            string nameo = null;
            nameo = Dns.GetHostEntry("173.252.120.68").ToString();

            dictionary.Add(nameo, nameo);

            return dictionary;
        }









        public static void GetSubnetMask()
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {


                        Console.Write(unicastIPAddressInformation);
                        //return unicastIPAddressInformation.IPv4Mask;
                    }

                }
            }
        }





        static string returnIp;





        // send arp requests and returns all that respond
        public static Dictionary<string, string> SendArpRequest(List<string> ipList)
        {

            List<string> ipList2 = new List<string>();
            ipList2.Add("192.168.0.1");
            ipList2.Add("192.168.0.18");
            ipList2.Add("192.168.0.22");
            ipList2.Add("192.168.0.114");
            ipList2.Add("192.168.0.232");
            //string[,] myCollection ;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();



            foreach (var ip in ipList)
            {

                IPAddress dst = IPAddress.Parse(ip);

                uint uintAddress = BitConverter.ToUInt32(dst.GetAddressBytes(), 0);
                byte[] macAddr = new byte[6];
                int macAddrLen = macAddr.Length;
                int retValue = SendARP(uintAddress, 0, macAddr, ref macAddrLen);
                if (retValue == 0)
                {

                    string[] str = new string[(int)macAddrLen];
                    for (int i = 0; i < macAddrLen; i++)
                        str[i] = macAddr[i].ToString("x2");
                    dictionary.Add(ip, string.Join(":", str));
                }

            }

            return dictionary;

        }

        //filter subnet machines get by arp sweep, to remove printers an another devices
        public static Dictionary<string, string> resolveName(Dictionary<string, string> ipList)
        {
            Dictionary<string, string> FilteredComputers = new Dictionary<string, string>();

            foreach (var machine in ipList.Keys)
            {
                IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                bool networkInterfaceWorking = new bool();
                networkInterfaceWorking = NetworkInterface.GetIsNetworkAvailable();

                Console.Write("se han encontrado interfaces activas");
                try
                {

                    string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    string hostName = System.Net.Dns.GetHostEntry(machine).HostName;
                    domainName = "." + domainName;



                    string[] Exceptions = { "CJ", "PL", "AD", "w" };

                    foreach (var Exception in Exceptions)
                    {
                        if (hostName.StartsWith(Exception, true, null))  // validate if the machine is CJ AD,PL 
                        {
                            FilteredComputers.Add(hostName, machine);   // add the machines to the array
                        }
                    }


                }
                catch
                {
                    Console.Write("No se ha podido contactar con el dominio");
                }

            }

            //FilteredComputers.Clear();
            //FilteredComputers.Add("localhost", "localhosst");

            return FilteredComputers;

        }


        static uint str2ip(string ip)
        {
            string[] numbers = ip.Split('.');

            uint x1 = (uint)(Convert.ToByte(numbers[0]) << 24);
            uint x2 = (uint)(Convert.ToByte(numbers[1]) << 16);
            uint x3 = (uint)(Convert.ToByte(numbers[2]) << 8);
            uint x4 = (uint)(Convert.ToByte(numbers[3]));

            return x1 + x2 + x3 + x4;
        }




        static List<string> getsubnetMachines(string startIp, string endIp)
        {
            uint startIP = str2ip(startIp);
            uint endIP = str2ip(endIp);
            List<string> list = new List<string>();
            for (uint currentIP = startIP; currentIP <= endIP; currentIP++)
            {
                string thisIP = ip2str(currentIP);
                //Console.WriteLine(thisIP);
                list.Add(thisIP);
            }

            return list;
        }


        static string ip2str(uint ip)
        {
            string s1 = ((ip & 0xff000000) >> 24).ToString() + ".";
            string s2 = ((ip & 0x00ff0000) >> 16).ToString() + ".";
            string s3 = ((ip & 0x0000ff00) >> 8).ToString() + ".";
            string s4 = (ip & 0x000000ff).ToString();

            string ip2 = s1 + s2 + s3 + s4;
            return ip2;
        }


        static string copyFiles(string FolderOrigin, string Machine, string FolderDestination)
        {
            string logInfo = null;

            String parameters;
            // creates the string that contain the parameters for execution of the process
            parameters = "\"" + FolderOrigin + "\" " + "\"\\\\" + Machine + FolderDestination + "\" " + "/mir /s";

            Console.Write(parameters);
            Process Process = new Process();

            try
            {
                Process.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                Process.StartInfo.FileName = "robocopy.exe";
                Process.StartInfo.CreateNoWindow = true;
                Process.StartInfo.RedirectStandardOutput = true;
                Process.StartInfo.Arguments = parameters;
              
                Process.Start();
                Process.WaitForExit();
                logInfo = Process.StandardOutput.ReadToEnd();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return logInfo;
        }



    }
}


