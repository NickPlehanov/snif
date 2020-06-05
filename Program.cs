using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace snif {
    class Program {
        static Stopwatch stopwatch = new Stopwatch();
        static List<Ports> Ports = new List<Ports>();
        static int i = 0;
        static DateTime last_dt;
        static double total_min;
        static int pid ;
        static ICaptureDevice captureDevice;
        static void Main(string[] args) {
            Start();
        }

           

        static void Start() {
            pid = Process.GetProcessesByName("viber")[0].Id;

            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "netstat",
                    Arguments = "-on",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            Regex r = new Regex(@"\S+\s+(?<address>\S+)\s+\S+\s+\S+\s+(?<pid>\d+)");
            while (!proc.StandardOutput.EndOfStream) {
                var res = r.Match(proc.StandardOutput.ReadLine());
                if (res.Success) {
                    if (res.Groups["pid"].Value == Process.GetProcessesByName("viber")[0].Id.ToString()) {
                        //  var pid = int.Parse(res.Groups["pid"].Value);
                        var address = res.Groups["address"].Value;
                        Ports.Add(new Ports() { num_port = address.ToString().Substring(address.ToString().IndexOf(':') + 1, address.ToString().Length - address.ToString().IndexOf(':') - 1) });
                        //address.ToString().Substring(address.ToString().IndexOf(':')+1,address.ToString().Length-address.ToString().IndexOf(':')-1)
                        Console.WriteLine("{0} - {1}", address, Process.GetProcessById(pid).ProcessName);
                    }
                }
            }
            //Console.ReadKey();

            // метод для получения списка устройств
            CaptureDeviceList deviceList = CaptureDeviceList.Instance;
            // выбираем первое устройство в спсике (для примера)
            captureDevice = deviceList[0];
            // регистрируем событие, которое срабатывает, когда пришел новый пакет
            captureDevice.OnPacketArrival += new PacketArrivalEventHandler(Program_OnPacketArrival);
            // открываем в режиме promiscuous, поддерживается также нормальный режим
            captureDevice.Open(DeviceMode.Promiscuous, 1000);
            // начинаем захват пакетов
            captureDevice.Capture();
        }

        static void Program_OnPacketArrival(object sender, CaptureEventArgs e) {
            //TimeSpan ts = stopwatch.Elapsed;
            try {
                if (TimeSpan.Parse(stopwatch.Elapsed.ToString()).TotalSeconds >=10) {
                    //ts = TimeSpan.Zero;
                    //stopwatch.Stop();
                    stopwatch.Reset();
                    //Ports.Clear();
                    Process process = Process.GetProcessById(Process.GetProcessesByName("viber")[0].Id);
                    process.Kill();
                    captureDevice.StopCapture();
                    //Thread.Sleep(5000);
                    System.Diagnostics.Process MyProc = new System.Diagnostics.Process();
                    MyProc.StartInfo.FileName = @"C:\Users\pna\AppData\Local\Viber\Viber.exe";
                    MyProc.Start();
                    Start();
                }
            }
            catch { }
            // парсинг всего пакета
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            // получение только TCP пакета из всего фрейма
            var tcpPacket = TcpPacket.GetEncapsulated(packet);
            // получение только IP пакета из всего фрейма
            var ipPacket = IpPacket.GetEncapsulated(packet);
            if (tcpPacket != null && ipPacket != null) {
                //DateTime time = e.Packet.Timeval.Date;
                //int len = e.Packet.Data.Length;

                //// IP адрес отправителя
                //var srcIp = ipPacket.SourceAddress.ToString();
                //// IP адрес получателя
                //var dstIp = ipPacket.DestinationAddress.ToString();

                //// порт отправителя
                //var srcPort = tcpPacket.SourcePort.ToString();
                //// порт получателя
                //var dstPort = tcpPacket.DestinationPort.ToString();
                //// данные пакета
                //var data = tcpPacket.PayloadPacket;
                //tcpPacket.DestinationPort.ToString()
                if (Ports.Any(x => x.num_port.Contains(tcpPacket.DestinationPort.ToString())))
                    if (ipPacket.DestinationAddress.ToString() == "192.168.1.28") {
                        stopwatch.Restart();
                        //stopwatch.Start();
                        Console.WriteLine("{0}({1}) {2} - {3}", i++, DateTime.Now.ToString(), tcpPacket.DestinationPort.ToString(), e.Packet.Data.Length);
                        //if (total_min >= 45) {
                            
                        //}
                    }
            }
        }
    }
}
