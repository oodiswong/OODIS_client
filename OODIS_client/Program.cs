using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading;
using System.Net.NetworkInformation;

namespace OODIS_client
{
    class Program
    {
        private const int BufferSize = 1024;
        private static TcpClient client;
        //private static NetworkStream netstream;
        public static string IPA;
        public static int PortN;

        private static StreamReader Reader_stream;

        [STAThread]
        static void Main(string[] args)
        {

            byte[] SendingBuffer = null;
            //TcpClient client = null;
            client = null;
            //NetworkStream netstream = null;
            //netstream = null;

            string file_name;

            Console.WriteLine("Hello");
            IPA = "127.0.0.1";
            PortN = 5000;
            file_name = "text.txt";

            Thread[] t = new Thread[400];          
            for (int i = 0; i < 1; i++)
            {
                t[i]= new Thread(() => exec_task(args));          // Kick off a new thread
                t[i].Start();
                Thread.Sleep(2000);
            }

            for (int i = 0; i < 1; i++)
            {
                t[i].Join();
            }

            //netstream.Close();
            //client.Close();
            
            Console.WriteLine("Done...");
        }


        private static void exec_task(string[] args)
        {
            try
            {
                Byte[] bytes = new byte[2560];
                string command_main;

                string line;
                string ws_filename;

                Console.WriteLine("Before connect to server");

                client = new TcpClient(IPA, PortN);

                if (!client.Client.Connected)
                {
                    Console.WriteLine("Connect to server failed, aborting.");
                    System.Environment.Exit(-255);

                }
                NetworkStream netstream = client.GetStream();

                Console.WriteLine("Before while loop ["+ netstream.CanWrite.ToString()+"]");

                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation c in tcpConnections)
                {
                    TcpState stateOfConnection = c.State;

                    if (c.LocalEndPoint.Equals(client.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(client.Client.RemoteEndPoint))
                    {
                        if (stateOfConnection == TcpState.Established)
                        {
                            Console.WriteLine("Server is connected.");
                        }
                        else
                        {
                            Console.WriteLine("Server is not connected.");
                        }

                    }

                }

                Console.WriteLine("after Local IP and PORT [" + client.Client.LocalEndPoint.ToString() + "]");

                ws_filename = "text.txt";
                if (args.Length > 0)
                {
                    ws_filename = args[0];
                }
                StreamReader sr = new StreamReader(ws_filename);

                //Read the first line of text
                line = sr.ReadLine();

                //Continue to read until you reach end of file
                while (line != null || sr.Peek() != -1)
                {
                    Console.WriteLine("LOOP Local IP and PORT [" + client.Client.LocalEndPoint.ToString() + "]");
                    //write the lie to console window
                    Console.WriteLine("rec [" + line + "]");
                    bytes = Encoding.ASCII.GetBytes(line + "\n");

                    string[] tmp1 = line.Split(' ');
                    command_main = tmp1[0];
                    string[] argumnts = new string[tmp1.Length - 1];
                    Array.Copy(tmp1, 1, argumnts, 0, tmp1.Length - 1);

                    //Console.WriteLine(argumnts.Length);
                    Console.WriteLine("cmd " + command_main+" "+bytes.Length.ToString());

                    try
                    {
                        netstream.Write(bytes, 0, (int)bytes.Length);
                        netstream.Flush();
                    }
                    catch (Exception strm_write_err)
                    {
                        Console.Write("Write to netstream failed " + strm_write_err.ToString());
                        System.Environment.Exit(-255);
                    }
                    //Thread.Sleep(500);

                    Console.WriteLine("command_main [" + command_main + "]");

                    if (command_main == "sleep")
                    {
                        Console.WriteLine("SleepFunc " + argumnts[0]);

                        int ws_sleep_sec = Int32.Parse(argumnts[0]);

                        Console.WriteLine("SleepFunc " + argumnts[0] + " " + ws_sleep_sec.ToString());

                        Thread.Sleep(ws_sleep_sec);
                    }

                    if (command_main == "thread")
                    {
                        Console.WriteLine("in thread portion");

                        byte[] thread_data = new byte[1024];
                        try
                        {
                            //String thread_str = gbl_tot_thread.ToString() + " " + Process.GetCurrentProcess().Threads.Count.ToString();
                            String thread_str = "hello";
                            //client.GetStream().Write(Encoding.ASCII.GetBytes(thread_str), 0, thread_str.Length);
                            int thread_len = (byte)client.GetStream().Read(thread_data, 0, thread_data.Length);
                            //client.GetStream().Write(Encoding.ASCII.GetBytes(local_remote), 0, total_size);
                            thread_str = System.Text.Encoding.Default.GetString(thread_data).Substring(0, thread_len);
                            Console.WriteLine("return from server thread [" + thread_str+ "]");
                        }
                        catch (Exception wr_err)
                        {
                            Console.WriteLine("Writethread  to client error " + wr_err.ToString());
                            //Thread.CurrentThread.Abort();       // Kill thread.
                                                                //s.Release();
                        }
                    }

                    if (command_main == "whoami")
                    {
                        String local_remote = "       ";
                        int total_size = local_remote.Length;
                        /*
                        int remote_info_size = remote_info.Length;
                        int local_info_size = local_info.Length;
                        String local_remote = local_info + " " + remote_info;

                        int total_size = local_remote.Length;
                        //Console.WriteLine("remote_info_size " + remote_info_size.ToString());
                        */

                        byte[] data = new byte[1024];

                        try
                        {
                            int pByte = (byte)client.GetStream().Read(data, 0, data.Length);
                            //client.GetStream().Write(Encoding.ASCII.GetBytes(local_remote), 0, total_size);

                            local_remote = System.Text.Encoding.Default.GetString(data).Substring(0, pByte);
                            Console.WriteLine("return from server [" + local_remote + "]");

                        }
                        catch (Exception wr_err)
                        {
                            Console.WriteLine("Write to client error " + wr_err.ToString());
                            //Thread.CurrentThread.Abort();       // Kill thread.
                            //s.Release();
                        }
                    }

                    if (command_main == "runbatch")
                    {
                        String local_remote = "       ";
                        int total_size = local_remote.Length;
                        /*
                        int remote_info_size = remote_info.Length;
                        int local_info_size = local_info.Length;
                        String local_remote = local_info + " " + remote_info;

                        int total_size = local_remote.Length;
                        //Console.WriteLine("remote_info_size " + remote_info_size.ToString());
                        */

                        byte[] data = new byte[1024];

                        try
                        {
                            int pByte = (byte)client.GetStream().Read(data, 0, data.Length);
                            //client.GetStream().Write(Encoding.ASCII.GetBytes(local_remote), 0, total_size);

                            local_remote = System.Text.Encoding.Default.GetString(data).Substring(0, pByte);
                            Console.WriteLine("return from server [" + local_remote + "]");

                        }
                        catch (Exception wr_err)
                        {
                            Console.WriteLine("Write to client error " + wr_err.ToString());
                            //Thread.CurrentThread.Abort();       // Kill thread.
                            //s.Release();
                        }
                    }
                    /* else
                    {
                        netstream.Write(bytes, 0, (int)bytes.Length);
                    }
                    */

                    /*
                    if (lCommands.ContainsKey(command_main))
                    {
                        Action<string[]> function_to_execute = null;
                        //string[] pass_args = { "" };
                        string[] pass_args = argumnts;

                        lCommands.TryGetValue(command_main, out function_to_execute);

                        if (command_main == "transfer")
                        {
                            string transfer_cmd = "transferfile " + tmp1[1] + " " + tmp1[2] + "\n";

                            Console.WriteLine("transfer_cmd [" + transfer_cmd + "]");

                            bytes = Encoding.ASCII.GetBytes(transfer_cmd);
                        }

                        Console.WriteLine("total byte write to stream " + bytes.Length.ToString());
                        netstream.Write(bytes, 0, (int)bytes.Length);

                        function_to_execute(pass_args);
                    }
                    else
                    {
                        netstream.Write(bytes, 0, (int)bytes.Length);
                    }
                    */

                    Thread.Sleep(500);

                    //Read the next line
                    line = sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[exec_task]" + ex.Message);
                //Console.WriteLine("exception Local IP and PORT [" + client.Client.LocalEndPoint.ToString() + "]");   
                System.Environment.Exit(-255);
            }
            finally
            {
                Console.WriteLine("in finally");
                //netstream.Close();
                //client.Close();

            }
        }

        private static Dictionary<string, Action<string[]>> lCommands =
        new Dictionary<string, Action<string[]>>()
        {
                //{ "help", HelpFunc },
                //{ "cp" , CopyFunc },
                //{ "ls" , LsFunc }
                { "where<EOF>", WhereFunc },
                { "thread", ThreadFunc },
                { "transfer", transferFunc },
                { "runbatch" , RunBatchFunc },
                { "sleep" , SleepFunc }
                //{ "run" , RunFunc }
        };

        //TRANSFER function
        private static void transferFunc(string[] obj)
        {
            string line;
            Byte[] bytes = new byte[2560];

            Console.WriteLine("transferFunc "+obj[0]+" "+obj[1]);
           
            String org_dir_file = obj[0];

            Console.WriteLine("Transfer ["+org_dir_file+"]");

            using (BinaryReader b = new BinaryReader(
            File.Open(org_dir_file, FileMode.Open)))
            {
                // 2.
                // Position and length variables.
                int pos = 0;
                // 2A.
                // Use BaseStream.
                int length = (int)b.BaseStream.Length;
                Console.WriteLine("file length " + length.ToString());
                while (pos < length)
                {
                    byte[] v = b.ReadBytes(1024);
                    //Console.WriteLine(v);
                    //netstream.Write(v, 0, v.Length);
                    // 4.
                    // Advance our position variable.
                    pos += v.Length;
                }
                Console.WriteLine("total bytes written " + pos.ToString());
            }

            //netstream.Close();
            client = new TcpClient(IPA, PortN);
            //netstream = client.GetStream();

        }

        //RunBatch file
        private static void RunBatchFunc(string[] obj)
        {
            Console.WriteLine(obj.Length);
            Console.WriteLine("RunBatchFunc length " + obj[0]);

            if (obj.Length != 2) return;
            Console.WriteLine("Run Batch " + obj[0] + " " + obj[1]);

            string cmd_line = obj[1];
            //ProcessStartInfo startInfo = new ProcessStartInfo(obj[0]);
            //startInfo.WindowStyle = ProcessWindowStyle.Normal;

            //startInfo.Arguments = cmd_line;
            //Process.Start(startInfo);

            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + obj[0] + " " + cmd_line);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            // wrap IDisposable into using (in order to release hProcess) 
            using (Process process = new Process())
            {
                process.StartInfo = procStartInfo;
                process.Start();

                // Add this: wait until process does its work
                process.WaitForExit();

                // and only then read the result
                string result = process.StandardOutput.ReadToEnd();
                Console.WriteLine(result);
                byte[] run_result_bytes = Encoding.ASCII.GetBytes(result);

                //netstream.Write(run_result_bytes, 0, run_result_bytes.Length);
            }

        }

        //SLEEP function
        private static void SleepFunc(string[] obj)
        {

            int ws_sleep_sec = Int32.Parse(obj[0]);

            Console.WriteLine("SleepFunc " + obj[0]+" "+ws_sleep_sec.ToString());

            Thread.Sleep(ws_sleep_sec);

        }

        private static void WhereFunc(string[] obj)
        {
            Console.WriteLine("WhereFunc ");

            Byte[] bytes = new byte[2560];
            byte[] remote_info = new byte[300]; 

            bytes = Encoding.ASCII.GetBytes("where");

            //netstream.ReadTimeout = 900;

            //Reader_stream = new StreamReader(netstream);
            //netstream.Write(bytes, 0, (int) bytes.Length);

            Console.WriteLine("Local IP and PORT [" + client.Client.LocalEndPoint.ToString() + "]");
            Console.WriteLine("after write to server");

            //int wsize = netstream.Read(remote_info, 0, (int) remote_info.Length);
            //Console.WriteLine("wsize " + wsize.ToString());
            //Console.WriteLine("after ReadLine [" + Encoding.ASCII.GetString(remote_info).Substring(0, wsize) + "]");
        }

        private static void ThreadFunc(string[] obj)
        {
            Console.WriteLine("ThreadFunc ");

            Byte[] bytes = new byte[2560];
            byte[] remote_info = new byte[300];

            bytes = Encoding.ASCII.GetBytes("thread");

            //netstream.ReadTimeout = 900;

            //Reader_stream = new StreamReader(netstream);
            //netstream.Write(bytes, 0, (int) bytes.Length);

           //Console.WriteLine("Local IP and PORT [" + client.Client.LocalEndPoint.ToString() + "]");
           //Console.WriteLine("after write to server");

            //int wsize = netstream.Read(remote_info, 0, (int)remote_info.Length);
            //Console.WriteLine("wsize " + wsize.ToString());
            //Console.WriteLine("Thread after ReadLine [" + Encoding.ASCII.GetString(remote_info).Substring(0, wsize) + "]");
        }
    }

}
