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
using System.Threading.Tasks;
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


            //List<string> ssssssssssajdhf = copyFiles("d:\\lempty", "localhost", "\\d$\\damage");


            if (IsNullOrEmpty(Arguments) || Arguments[0].ToLower() == "/h" || Arguments[0].ToLower() == "/help" || Arguments[0].ToLower() == "/?")
            {
                Console.WriteLine(helptext("fullhelp").ToString());
                return 0;
            };

            string DiscoverExceptions = null;
            string distributionFile = null;
            string Logfile = null;
            string[] ExceptionsArray = null;
            string listofmachines = null;

            for (int i = 0; i < Arguments.Length; i++)
            {
                Arguments[i] = Arguments[i].ToLower();


                if (Arguments[i].Equals("discoverexceptions"))
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
                else if (Arguments[i].Equals("listofmachines"))
                {// file this machines list

                    try
                    {
                        listofmachines = Arguments[i + 1];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Please type a valid argument for the /listofmachines location" + '\n');
                        Console.WriteLine(helptext("EXAMPLES").ToString());
                        return 0;
                    }


                    bool invalidcharacters = listofmachines.IndexOfAny(Path.GetInvalidPathChars()) == -1;
                    if (!invalidcharacters)
                    {
                        Console.WriteLine("The path " + "\"" + listofmachines + "\"" + "contains invalid charactevrs" + '\n');
                        Console.WriteLine(helptext("EXAMPLES").ToString());
                        return 0;
                    }
                    else if (!File.Exists(listofmachines))
                    {
                        Console.WriteLine("Couldnt find the file " + "\"" + listofmachines + "\"" + '\n');
                        Console.WriteLine(helptext("EXAMPLES").ToString());
                        return 0;
                    }

                }
                else if (Arguments[i].Equals("logfile"))
                {// location of log folder validations

                    try
                    {
                        Logfile = Arguments[i + 1];
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }
                else if (Arguments[i].Equals("distributionfile"))
                {// location the file that contains the rules 

                    try
                    {
                        distributionFile = Arguments[i + 1];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Please type a valid argument for the /distributionfile location" + '\n');
                        Console.WriteLine(helptext("EXAMPLES").ToString());
                        return 0;
                    }





                    bool invalidcharacters = distributionFile.IndexOfAny(Path.GetInvalidPathChars()) == -1;
                    if (!invalidcharacters)
                    {
                        Console.WriteLine("The path " + "\"" + distributionFile + "\"" + "contains invalid charactevrs" + '\n');
                        Console.WriteLine(helptext("EXAMPLES").ToString());
                        return 0;
                    }
                    else if (!File.Exists(distributionFile))
                    {
                        Console.WriteLine("Couldnt find the file " + "\"" + distributionFile + "\"" + '\n');
                        Console.WriteLine(helptext("EXAMPLES").ToString());
                        return 0;
                    }


                }



            }

            List<string> machinesListCSV = new List<string>();
            List<string> distributionRulesCSV = new List<string>();
            if (Logfile != null)
            {

                //validate mandatory parameters

                if (distributionFile != null && listofmachines != null)
                {


                    machinesListCSV = splitlinesCSV(listofmachines);
                    distributionRulesCSV = splitlinesCSV(distributionFile);
                    if (machinesListCSV == null || distributionRulesCSV == null)
                    {
                        Console.WriteLine("Unable to parce the distributionFile" + distributionFile + '\n');
                        return 0;
                    }


                }
                else if (distributionFile != null && DiscoverExceptions != null)
                {

                    distributionRulesCSV = splitlinesCSV(distributionFile);


                    //DiscoverExceptions
                }
                else
                {
                    //specify the list of machines to deploy  
                    Console.Write(helptext("EXAMPLES").ToString());
                    return 0;

                }

            }
            else
            {
                //mising log file o ditribution file
                Console.Write(helptext("EXAMPLES").ToString());
                return 0;
            }


            ////////////////////////////////////////////////////////////
            //machinesListCSV = splitlinesCSV(listofmachines);
            //distributionRulesCSV = splitlinesCSV(distributionFile);
            startlogic(distributionRulesCSV, machinesListCSV, Logfile, ExceptionsArray);

            return 0;


        }

        public static bool IsNullOrEmpty(this Array array)
        {
            return (array == null || array.Length == 0);
        }
        static int startlogic(List<string> DistributionsRules, List<string> DestinationMachines)
        {

            string Logfile = "%temp%";
            int returnval = 0;
            string[] DiscoverExceptions = { "value 1", "value 2", "value 3" };
            startlogic(DistributionsRules, DestinationMachines, Logfile, DiscoverExceptions);
            return returnval;
        }

        static int startlogic(List<string> DistributionsRules, List<string> DestinationMachines, string Logfile)
        {
            int returnval = 0;
            string[] DiscoverExceptions = { "value 1", "value 2", "value 3" };
            startlogic(DistributionsRules, DestinationMachines, Logfile, DiscoverExceptions);
            return returnval;
        }
        static int startlogic(List<string> DistributionsRules, List<string> DestinationMachines, string Logfile, string[] DiscoverExceptions)
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
            Dictionary<string, string> listOfMachinesInTheSubnet = resolveName(listOfDevicesInTheSubnet, DiscoverExceptions);
            

            foreach (var item in listOfMachinesInTheSubnet.Keys)
            {
                System.IO.File.AppendAllText(Logfile + "\\listOfMachinesInTheSubnet.txt", item + '\n');
            }
            Console.WriteLine("discover filter complete");

            //Console.WriteLine(DistributionsRules);
            //Console.WriteLine(DestinationMachines);
            //Console.ReadKey();



            List<List<string>> vervose = CopyFileParallel(DestinationMachines, DistributionsRules);
            foreach (var items in vervose)
            {

                foreach (var item in items)
                {
                    System.IO.File.AppendAllText(Logfile + "\\vervose.txt", item.ToString() + '\n');
                }

            }





            return 0;
        }


        static string helptext(string helpPart)
        {


            string paramtext = @"

Distributefiles mirrors a folder content in all the machines of the local subnet. 


    /DistributionFile       Specifies the Location of the the file that contains the rules to deploy 
                            the files.
    
    /Logfile                Specifies the folder destination of the log file. The default value is 
                            the temp folder.
                    
    /ListOfMachines         This parameter overrides the discovery of The networksubnet and allows, to
                            distribute the folder, to a list of machines in a .txt file.
    
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

            string outstring = "";

            ConsoleKeyInfo cki;
            switch (helpPart.ToLower())
            {
                case "fullhelp":
                    outstring = paramtext + Exampletext;
                    break;
                case "help":
                    outstring = paramtext;
                    break;
                case "examples":
                    Console.Write("Press Y to show examples" + '\n');
                    cki = Console.ReadKey();
                    cki.Key.ToString();


                    if (cki.KeyChar.Equals('Y') || cki.KeyChar.Equals('y'))
                    {
                        outstring = Exampletext;
                    }

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
            ipList2.Add("192.168.0.24");
            ipList2.Add("192.168.0.12");
            ipList2.Add("192.168.0.222");
            //string[,] myCollection ;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();



            foreach (var ip in ipList2)
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

                Console.WriteLine("se han encontrado interfaces activas");
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
                    Console.WriteLine("No se ha podido contactar con el dominio");
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



        static List<string> splitlinesCSV(string filepath)
        {


            var reader = new StreamReader(File.OpenRead(filepath));
            List<string> csvLines = new List<string>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                csvLines.Add(line);
            }
            return csvLines;
        }

        private static List<List<string>> CopyFileParallel(List<string> machines, List<string> fileRules)
        {







            List<List<string>> collection = new List<List<string>>();
            //string Machine = "localhost";
            List<string> collection2 = new List<string>();
            //Parallel.ForEach(machines, (machine) =>
            //{

            foreach (var machine in machines)
            {
                Console.WriteLine("-----------------------------------"+ machine + "-----------------------------------");
                //test conection to the machine 
                PingReply reply = null;
                Ping ping = new Ping();
                try
                {
                     reply = ping.Send(machine);
                }
                catch (Exception)
                {
                  


                }


                if (reply == null) {
                    Console.WriteLine("unable to contact the machine" + machine);
                }
               
                else if ( reply.Status == IPStatus.Success )
                {


                    foreach (var fileRule in fileRules)
                    {
                        string parameters = null;
                        string source = null;
                        string destination = null;
                        string filename = null;

                        //formats the source an destination rules
                        var rules = fileRule.Split(',');
                        try
                        {
                            source = rules[0];
                            destination = rules[1];

                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Unable to parse the file rule," + "\"" + rules.ToString() + "\"" + "while copying to" + machine);
                            continue;
                        }






                        if (Directory.Exists(source))
                        {
                            source = source.TrimEnd('\\');
                            destination = destination.TrimEnd('\\');
                            if (destination.StartsWith(@"\"))
                            {

                                parameters = String.Format(" \"{0}\" \"\\\\{1}{2}\" /mir ", source, machine, destination);
                            }
                            else
                            {
                                parameters = String.Format(" \"{0}\" \"{1}\" /mir ", source, destination);
                            }
                        }
                        else if (File.Exists(source))
                        {
                            filename = Path.GetFileName(source);
                            destination = Path.GetDirectoryName(destination);
                            source = Path.GetDirectoryName(source);
                            source = source.TrimEnd('\\');
                            destination = destination.TrimEnd('\\');
                            if (destination.StartsWith(@"\"))
                            {
                                if (Directory.Exists(destination))
                                {
                                }





                                parameters = String.Format(" \"{0}\" \"\\\\{1}{2}\" {3}  ", source, machine, destination, filename);
                            }
                            else
                            {
                                parameters = String.Format(" \"{0}\" \"{1}\" {2}   ", source, destination, filename);
                            }
                        }
                        else
                        {
                            Console.WriteLine("couldnt find the source file or folder while coping to" + machine);
                            continue;

                        }




                        //create parameter string 
                        //string parameters = "\"" + folderOrigin + "\" " + "\"\\\\" + machine + folderDestination + machine + "\" " + "/mir /s";
                        string logInfo;


                        // start copy for each file rule to the current machine 
                        Process process = new Process();
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = "robocopy.exe";
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.Arguments = parameters;

                        process.Start();

                        //set the maximun allowed time in which the robocopy lives 
                        if (process.WaitForExit(15000))
                        {
                            logInfo = process.StandardOutput.ReadToEnd();
                            string[] FL = logInfo.Split(new[] { '\r', '\n' });
                            var Ended = Array.FindIndex(FL, x => x.Contains("Ended"));
                            var Started = Array.FindIndex(FL, x => x.Contains("Started"));
                            var Source = Array.FindIndex(FL, x => x.Contains("Source"));
                            var Dest = Array.FindIndex(FL, x => x.Contains("Dest"));
                            var Total = Array.FindIndex(FL, x => x.Contains("Total"));
                            var Dirs = Array.FindIndex(FL, x => x.Contains("Dirs"));
                            var Files = Array.FindIndex(FL, x => x.Contains("Files :  "));
                            var line = Array.FindIndex(FL, x => x.Contains("-----"));

                            try
                            {
                                List<string> formatlogfile = new List<string> { FL[Ended], FL[Started], FL[Source], FL[Dest], FL[line], FL[Total], FL[Dirs], FL[Files], FL[line] };
                                foreach (string logLine in formatlogfile)
                                {
                                    //Console.WriteLine(logLine);
                                }


                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("The copy to was succesfull" + parameters);
                                Console.ResetColor();

                                collection.Add(formatlogfile);

                            }
                            catch (Exception)
                            {
                                Console.WriteLine("There was and error while coping to" + parameters);
                                collection.Add(FL.ToList());
                                //savefullerrordetail(logInfo, parameters);
                            }

                        }
                        else
                        {


                            Console.WriteLine("The process exceeded the maximum allowed time" + parameters);




                            process.Kill();
                        }


                    }

                }
                else
                {
                    Console.WriteLine("unable to contact the machine" + machine);
                }

                
                //});

            }

            Console.WriteLine("Finished copy execution");


            return collection;

        }




    }
}


