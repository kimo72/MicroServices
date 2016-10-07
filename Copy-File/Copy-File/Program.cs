using System.Collections.Generic;
using LukeSkywalker.IPNetwork;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.IO;
using System;



namespace google
{
    static class Program
    {

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint DestIP, uint SrcIP, byte[] pMacAddr, ref int PhyAddrLen);



        static int Main(string[] Arguments)
        {


            


            if (IsNullOrEmpty(Arguments) || Arguments[0].ToLower() == "/h" || Arguments[0].ToLower() == "/help" || Arguments[0].ToLower() == "/?")
            {
                Console.Write(helptext("fullhelp").ToString());
                return 0;
            };

            string SourceFolder = null;
            string Destinationfolder = null;
            string DiscoverExceptions = null;
            string Logfile = null;
            string[] ExceptionsArray = null;

            for (int i = 0; i < Arguments.Length; i++)
            {
                Arguments[i] = Arguments[i].ToLower();
                if (Arguments[i].StartsWith("sourcefolder"))
                {// source folde validations

                    try
                    {
                        SourceFolder = Arguments[i + 1];
                    }
                    catch (Exception)
                    {
                        Console.Write("Please type a valid argument for the /sourcefolder location" + '\n');
                        Console.Write(helptext("EXAMPLES").ToString());
                        return 0;
                    }
                    

                    bool invalidcharacters = SourceFolder.IndexOfAny(Path.GetInvalidPathChars()) == -1;
                    if (!invalidcharacters)
                    {
                        Console.Write("The path " + "\"" +SourceFolder+ "\"" + "contains invalid charactevrs" + '\n');
                        Console.Write(helptext("EXAMPLES").ToString());
                        return 0;
                    }
                    else if (!Directory.Exists(SourceFolder))
                    {
                        Console.Write("Couldnt find path "+ "\"" + SourceFolder + "\"" +'\n');
                        Console.Write(helptext("EXAMPLES").ToString());
                        return 0;
                    }

                }
                else if (Arguments[i].StartsWith("destinationfolder"))
                {// destination folder validations
                   

                    try
                    {
                        Destinationfolder = Arguments[i + 1];
                    }
                    catch (Exception)
                    {
                        Console.Write("Please type a valid argument for the /destinationfolder location" + '\n');
                        Console.Write(helptext("EXAMPLES").ToString());
                        return 0;
                    }

                    bool invalidcharacters = Destinationfolder.IndexOfAny(Path.GetInvalidPathChars()) == -1;
                    if (!invalidcharacters)
                    {
                        Console.Write("The path " + Destinationfolder + "contains invalid characters" + '\n');
                        Console.Write(helptext("EXAMPLES").ToString());
                        return 0;
                    }



                }
                else if (Arguments[i].StartsWith("discoverexceptions"))
                {// discover exceptions validations

                    DiscoverExceptions = Arguments[i + 1];

                    try
                    {
                        DiscoverExceptions = Arguments[i + 1];
                    }
                    catch (Exception)
                    {
                        Console.Write("Please type a valid argument for /discoverexceptions " + '\n');
                        Console.Write(helptext("EXAMPLES").ToString());
                        return 0;
                    }
                    ExceptionsArray = DiscoverExceptions.Split(',');



                }
                else if (Arguments[i].StartsWith("logfile"))
                {// location of log folder validations

                    try
                    {
                        Logfile = Arguments[i + 1];
                    }
                    catch (Exception)
                    {

                        throw;
                    }                        
                    
                    if (true)
                    {

                    }
                }
         

            }

             if (SourceFolder == null ||  Destinationfolder == null || Logfile == null)
            {
                Console.Write("There are missing mandatory parameters");
                Console.Write(helptext("EXAMPLES").ToString());
                return 0;
            }



            ////////////////////////////////////////////////////////////

            startlogic(SourceFolder, Destinationfolder, Logfile, ExceptionsArray);

            return 0;


        }

        public static bool IsNullOrEmpty(this Array array)
        {
            return (array == null || array.Length == 0);
        }
        
        static int startlogic(string SourceFolder, string Destinationfolder, string Logfile)
        {
            int returnval = 0;
            string[] DiscoverExceptions = { "value 1", "value 2", "value 3" };
            startlogic(SourceFolder, Destinationfolder, Logfile, DiscoverExceptions);
            return returnval;
        }
            static int startlogic(string SourceFolder, string Destinationfolder, string Logfile, string[] DiscoverExceptions )
        {

        //Clean server arp chache
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
                System.IO.File.AppendAllText(Logfile + "\\listOfDevicesInTheSubnet.txt", item + '\n');
            }


            //NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            Dictionary<string, string> listOfMachinesInTheSubnet = resolveName(listOfDevicesInTheSubnet,DiscoverExceptions);


            foreach (var item in listOfMachinesInTheSubnet.Keys)
            {
                System.IO.File.AppendAllText(Logfile + "\\listOfMachinesInTheSubnet.txt", item + '\n');
            }
            Console.WriteLine("discover filter complete");

            Console.WriteLine(SourceFolder);
            Console.WriteLine(Destinationfolder);
            Console.ReadKey();
            foreach (var machine in listOfMachinesInTheSubnet.Keys)
            {
                string vervose = copyFiles(SourceFolder, machine, Destinationfolder);
                System.IO.File.AppendAllText(Logfile + "\\vervose.txt", vervose);

            }


            return 0;
        }


        static string helptext(string helpPart)
        {


            string paramtext = @"

DistributeFolder mirrors a folder content in all the machines of the local subnet. 



    /SourceFolder           Specifies the Location of the folder where the files 
                            to deploy are located.

    /DestinationFolder      Specifies the destination of the folder where the files 
                            to deploy .(if the folder doesnt exist it will be created)
                                
                        *** All the files in destination folder that arent in the source folder 
                            will be DELETED !

    /Logfile                Specifies the folder destination of the log file.
                    
    /DiscoverExceptions     This filter the machines in the subnet that name starts With the
                            filter. 
";
            string Exampletext = @"
--EXAMPLES

        The command copies to the machines that start with PL, AD or CJ.
        
            DistributeFolder.exe  /SourceFolder  ""c:\Empty folder"" /DestinationFolder 
            ""\\D$\Mis Documentos\trabajo""

        The command copies to the machines that start with PL, AD or CJ.
           
            DistributeFolder.exe  /SourceFolder  ""c:\Empty folder"" /DestinationFolder 
            ""\\D$\Mis Documentos\trabajo""

        The command copies to the machines that start with PL, AD or CJ.

            DistributeFolder.exe  /SourceFolder  ""c:\Empty folder"" /DestinationFolder 
            ""\\D$\Mis Documentos\trabajo""
        
        The command copies to the machines that start with PL, AD or CJ.

            DistributeFolder.exe  /SourceFolder  ""c:\Empty folder"" /DestinationFolder 
            ""\\D$\Mis Documentos\trabajo"" /DiscoverExceptions ""PL,AD,CJ""

";

            string outstring = null;


            switch (helpPart.ToLower())
            {
                case "fullhelp":
                    outstring = paramtext + Exampletext;
                    break;
                case "help":
                    outstring = paramtext;
                    break;
                case "examples":
                    outstring = Exampletext;
                    break;
            }

            return outstring;
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
        public static Dictionary<string, string> resolveName(Dictionary<string, string> ipList, string[] DNSExceptions)
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

                    
                    
                    string[] Exceptions = DNSExceptions;

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


