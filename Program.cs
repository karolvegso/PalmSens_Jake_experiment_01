using PalmSens;
using PalmSens.Comm;
using PalmSens.Devices;
using PalmSens.Plottables;
using PalmSens.Techniques;
using PalmSens.Units;
using PalmSens.Windows;
using PalmSens.Windows.Devices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PalmSens.Analysis.CFALevelList;
using static System.Collections.Specialized.BitVector32;

namespace PalmSens_Jake_experiment_01
{
    internal class Program
    {
        private static int _measurement_is_completed = 0; // flag to indicate whether the measurement is completed
        private static bool _state_of_charging = true; // flag to indicate whether the charging is in progress
        private static float _max_potential = 0.0f; // the maximum potential value to stop the charging 
        private static string _filename_potentials = "Charging_Potentials"; // the name of the text file to save measured potential data
        private static bool _state_of_amperometry = false; // flag to indicate whether the amperometry is in progress
        private static string _filename_amperometry = "Chronoamperometry_Currents"; // the name of the text file to save measured amperometry data
        private static bool _state_of_current_beginning = true; // flag to indicate whether the current at the beginning of amperometry is measured
        private static float _current_beginning = 0.0f; // define current at the beginning of amperometry measurement
        private static float _amperometry_multiplication_factor = 0.1f;
        private static bool _state_of_multisteppotentiometry = false; // flag to indicate whether the multistep chronopotentiometry is in progress
        private static string _filename_multisteppotentiometry = "Multistep_chronopotentiometry_Potentials"; // the name of the text file to save measured amperometry data   
        static void Main(string[] args)
        {
            Console.WriteLine($"The charging current is [mA]: {args[0]}");
            float charging_current = float.Parse(args[0], CultureInfo.InvariantCulture);
            Console.WriteLine($"The applied current range is: {args[1]}");
            string applied_current_range = args[1];
            Console.WriteLine($"The maximum potential is [Volts]: {args[2]}");
            _max_potential = float.Parse(args[2], CultureInfo.InvariantCulture);
            Console.WriteLine($"Chronopotentiometry run time is [s]: {args[3]}");
            float run_time_potentiometry = float.Parse(args[3], CultureInfo.InvariantCulture);
            Console.WriteLine($"Chronopotentiometry interval time is [s]: {args[4]}");
            float interval_time_potentiometry = float.Parse(args[4], CultureInfo.InvariantCulture);
            // define filename to save chronopotentiometry measured data
            Console.WriteLine($"The filename to save measured potential values is: {args[5]}");
            _filename_potentials = args[5] + ".txt";
            Console.WriteLine($"Amperometry run time is [s]: {args[6]}");
            float run_time_amperometry = float.Parse(args[6], CultureInfo.InvariantCulture);
            Console.WriteLine($"Amperometry interval time is [s]: {args[7]}");
            float interval_time_amperometry = float.Parse(args[7], CultureInfo.InvariantCulture);
            Console.WriteLine($"Amperometry multiplication factor for beginning current is: {args[8]}");
            _amperometry_multiplication_factor = float.Parse(args[8], CultureInfo.InvariantCulture);
            // define filename to save chronoamperometry measured data
            Console.WriteLine($"The filename to save measured current values is: {args[9]}");
            _filename_amperometry = args[9] + ".txt";
            // define filename to save multistep chronopotentiometry measured data
            Console.WriteLine($"The filename to save measured potential values in multistep chronopotentiometry is: {args[10]}");
            _filename_multisteppotentiometry = args[10] + ".txt";

            // define path to multistep potentiometry data file
            string path_to_multisteppotentiometry_file = @"C:\PalmSens_measurements\multistep_potentiometry_input_file.txt";

            // inialize dc currents list
            List<float> dc_currents_list = new List<float>();
            // initialize applied current ranges list
            List<string> applied_current_ranges_list = new List<string>();
            // initialize number of cycles list
            List<int> no_cycles_list = new List<int>();
            // initialize duration times list
            List<float> duration_times_list = new List<float>();
            // initialize interval times list
            List<float> interval_times_list = new List<float>();

            // read text file
            using (StreamReader multistep_potentiometry_reader = new StreamReader(path_to_multisteppotentiometry_file))
            {
                string line;
                while ((line = multistep_potentiometry_reader.ReadLine()) != null)
                {
                    // split line
                    string[] values = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    // read dc current value from text file
                    float dc_current_value = float.Parse(values[0], CultureInfo.InvariantCulture);
                    // read applied current range value from text file
                    string applied_current_range_value = values[1];
                    // read number of cycles value from text file
                    int no_cycles_value = int.Parse(values[2]);
                    // read duration time value from text file
                    float duration_time_value = float.Parse(values[3], CultureInfo.InvariantCulture);
                    // read interval time value from text file
                    float interval_time_value = float.Parse(values[4], CultureInfo.InvariantCulture);
                    // add dc current value to list
                    dc_currents_list.Add(dc_current_value);
                    // add applied current range value to list
                    applied_current_ranges_list.Add(applied_current_range_value);   
                    // add number of cycles value to list
                    no_cycles_list.Add(no_cycles_value);
                    // add duration time value to list
                    duration_times_list.Add(duration_time_value);
                    // add interval time value to list
                    interval_times_list.Add(interval_time_value);
                }
            }

            // get length of list of dc currents
            int no_dc_currents = dc_currents_list.Count;
            // initialize dc currents array
            float[] dc_currents_array = new float[no_dc_currents];
            // fill dc currents array
            for (int index_0 = 0; index_0 < no_dc_currents; index_0++)
            {
                dc_currents_array[index_0] = dc_currents_list[index_0];
                Console.WriteLine(dc_currents_array[index_0]);
            }

            // get length of list of applied current ranges
            int no_applied_current_ranges = applied_current_ranges_list.Count;
            // initialize applied current ranges array
            string[] applied_current_ranges_array = new string[no_applied_current_ranges];
            // fill applied current ranges array
            for (int index_0 = 0; index_0 < no_applied_current_ranges; index_0++)
            {
                applied_current_ranges_array[index_0] = applied_current_ranges_list[index_0];
                Console.WriteLine(applied_current_ranges_array[index_0]);
            }

            // get length of list of number of cycles
            int no_no_cycles = no_cycles_list.Count;
            // initialize number ofcycles array
            int[] no_cycles_array = new int[no_no_cycles];
            // fill number of cycles array
            for (int index_0 = 0; index_0 < no_no_cycles; index_0++)
            {
                no_cycles_array[index_0] = no_cycles_list[index_0];
                Console.WriteLine(no_cycles_array[index_0]);
            }

            // get length of list of duration times
            int no_duration_times = duration_times_list.Count;
            // initialize duration times array
            float[] duration_times_array = new float[no_duration_times];
            // fill duration times array
            for (int index_0 = 0; index_0 < no_duration_times; index_0++)
            {
                duration_times_array[index_0] = duration_times_list[index_0];
                Console.WriteLine(duration_times_array[index_0]);
            }

            // get length of list of interval times
            int no_interval_times = interval_times_list.Count;
            // initialize interval times array
            float[] interval_times_array = new float[no_interval_times];
            // fill interval times array
            for (int index_0 = 0; index_0 < no_interval_times; index_0++)
            {
                interval_times_array[index_0] = interval_times_list[index_0];
                Console.WriteLine(interval_times_array[index_0]);
            }

            //Initiate the SDKs core dependencies
            CoreDependencies.Init();

            Console.WriteLine("");
            Console.WriteLine("Discovering connected devices...");
            Console.WriteLine("");

            //Discover connected devices
            Device[] connectedDevices = null;
            while (connectedDevices == null || connectedDevices.Length == 0)
                connectedDevices = DiscoverConnectedDevices();

            Console.WriteLine("");
            Console.WriteLine($"Found {connectedDevices.Length} connected devices.");
            Console.WriteLine("");

            //List connected devices
            for (int i = 0; i < connectedDevices.Length; i++)
                Console.WriteLine($"{i + 1}. {connectedDevices[i].ToString()}");
            Console.WriteLine("");

            int deviceIndex = 0;
            if (connectedDevices.Length > 1)
            {
                //Prompt user which device to connect to
                string msg = $"Specify which device to connect to (a number between 1 and {connectedDevices.Length}):";
                while (true)
                {
                    Console.WriteLine(msg);
                    string input = Console.ReadLine();
                    int index;
                    if (int.TryParse(input, out index) && index > 0 && index <= connectedDevices.Length)
                    {
                        deviceIndex = index - 1;
                        break;
                    }
                }
            }

            Device connectedDevice = connectedDevices[deviceIndex];
            Console.WriteLine("");
            Console.WriteLine($"Connecting to {connectedDevice.ToString()}.");
            Console.WriteLine("");

            //Connect to device
            Connect(connectedDevice);

            // initialize multistep chronopotentiometry method
            PalmSens.Techniques.Potentiometry method_potentiometry = new PalmSens.Techniques.Potentiometry();
            // set applied Current range 
            if (applied_current_range == "cr1mA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1mA);
            }
            else if (applied_current_range == "cr10mA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr10mA);
            }
            else if (applied_current_range == "cr100mA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100mA);
            }
            else if (applied_current_range == "cr100pA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100pA);
            }
            else if (applied_current_range == "cr1nA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1nA);
            }
            else if (applied_current_range == "cr10nA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr10nA);
            }
            else if (applied_current_range == "cr100nA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100nA);
            }
            else if (applied_current_range == "cr1uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1uA);
            }
            else if (applied_current_range == "cr10uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr10uA);
            }
            else if (applied_current_range == "cr100uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100uA);
            }
            else if (applied_current_range == "cr2uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr2uA);
            }
            else if (applied_current_range == "cr4uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr4uA);
            }
            else if (applied_current_range == "cr8uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr8uA);
            }
            else if (applied_current_range == "cr16uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr16uA);
            }
            else if (applied_current_range == "cr32uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr32uA);
            }
            else if (applied_current_range == "cr125uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr125uA);
            }
            else if (applied_current_range == "cr250uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr250uA);
            }
            else if (applied_current_range == "cr500uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr500uA);
            }
            else if (applied_current_range == "cr5mA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr5mA);
            }
            else if (applied_current_range == "cr6uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr6uA);
            }
            else if (applied_current_range == "cr13uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr13uA);
            }
            else if (applied_current_range == "cr25uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr25uA);
            }
            else if (applied_current_range == "cr50uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr50uA);
            }
            else if (applied_current_range == "cr200uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr200uA);
            }
            else if (applied_current_range == "cr63uA")
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr63uA);
            }
            else
            {
                method_potentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1mA);
            }
            // set applied current
            method_potentiometry.Current = charging_current; //Sets the applied current for the measurement
            // set interval time
            method_potentiometry.IntervalTime = interval_time_potentiometry; //Measures the current 10 times per second
            // set run time
            method_potentiometry.RunTime = run_time_potentiometry; //Sets the total run time for the measurement

            // initialize errors variable
            string errors = string.Empty;

            // path to stop text file
            string path_to_stop_file = @"C:\PalmSens_measurements\stop.txt";

            // set flags for chronopotentiometry
            _state_of_charging = true;
            _state_of_amperometry = false;
            _state_of_multisteppotentiometry = false;

            while (_state_of_charging == true)
            {
                //Subscribe to events
                _comm.BeginMeasurement += _comm_BeginMeasurement;
                _comm.BeginReceiveCurve += _comm_BeginReceiveCurve;
                _comm.EndMeasurement += _comm_EndMeasurement;

                //Start the measurement
                //errors = CommInvokeThreadSafe(() => { _measurement_is_completed = 0; string exit_code = _comm.Measure(method_potentiometry); Console.WriteLine($"Exit code is: {exit_code}"); return exit_code; });
                errors = CommInvokeThreadSafe(() => { _measurement_is_completed = 0; return _comm.Measure(method_potentiometry); });

                while (_measurement_is_completed == 0)
                {
                    //wait for the measurement to complete
                    System.Threading.Thread.Sleep(100);
                    // create new file stream object to avoid violation exception
                    FileStream fs1 = new FileStream(path_to_stop_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    // create stream reader object
                    using (StreamReader sr = new StreamReader(fs1))
                    {
                        // read first line
                        string firstLine = sr.ReadLine();
                        //Console.WriteLine(firstLine);
                        // condition
                        if (firstLine == "stop" || firstLine == "exit" || firstLine == "quit" || firstLine == "end")
                        {
                            Console.WriteLine("The chronopotentiometry measurement is stopped.");
                            System.Threading.Thread.Sleep(100);
                            // break main while loop
                            break;
                        }
                        else
                        {
                            // continue main while loop
                            continue;
                        }
                    }
                }

                // create new file stream object to avoid violation exception
                FileStream fs2 = new FileStream(path_to_stop_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                // create stream reader object
                using (StreamReader sr = new StreamReader(fs2))
                {
                    // read first line
                    string firstLine = sr.ReadLine();
                    //Console.WriteLine(firstLine);
                    // condition
                    if (firstLine == "stop" || firstLine == "exit" || firstLine == "quit" || firstLine == "end")
                    {
                        Console.WriteLine("The chronopotentiometry measurement is stopped.");
                        System.Threading.Thread.Sleep(100);
                        // set current to zero
                        _comm.Current = 0.0f;
                        // wait extra time
                        System.Threading.Thread.Sleep(1000);
                        // break main while loop
                        break;
                    }
                    else
                    {
                        Console.WriteLine(_measurement_is_completed);
                        // continue main while loop
                        continue;
                    }
                }
            }

            // detroy communication with PalmSens device
            _comm.BeginMeasurement -= _comm_BeginMeasurement;
            _comm.BeginReceiveCurve -= _comm_BeginReceiveCurve;
            _comm.EndMeasurement -= _comm_EndMeasurement;
            // set current to zero
            _comm.Current = 0.0f;
            // wait extra time
            System.Threading.Thread.Sleep(1000);
            // end potentiometry method
            method_potentiometry.Dispose();

            // initialize multistep chronopotentiometry method
            PalmSens.Techniques.AmperometricDetection method_amperometry = new PalmSens.Techniques.AmperometricDetection();

            // set maximum potential value for chronoamperometry method
            method_amperometry.Potential = _max_potential; //Sets the potential for the measurement
            // set interval time
            method_amperometry.IntervalTime = interval_time_amperometry; //Measures the current 10 times per second
            // set run time
            method_amperometry.RunTime = run_time_amperometry; //Sets the total run time for the measurement

            // set flags for chronoamperometry
            _state_of_charging = false;
            _state_of_amperometry = true;
            _state_of_current_beginning = true;
            _state_of_multisteppotentiometry = false;

            while (_state_of_amperometry == true)
            {
                //Subscribe to events
                _comm.BeginMeasurement += _comm_BeginMeasurement;
                _comm.BeginReceiveCurve += _comm_BeginReceiveCurve;
                _comm.EndMeasurement += _comm_EndMeasurement;

                //Start the measurement
                //errors = CommInvokeThreadSafe(() => { _measurement_is_completed = 0; string exit_code = _comm.Measure(method_potentiometry); Console.WriteLine($"Exit code is: {exit_code}"); return exit_code; });
                errors = CommInvokeThreadSafe(() => { _measurement_is_completed = 0; return _comm.Measure(method_amperometry); });

                while (_measurement_is_completed == 0)
                {
                    //wait for the measurement to complete
                    System.Threading.Thread.Sleep(100);
                    // create new file stream object to avoid violation exception
                    FileStream fs1 = new FileStream(path_to_stop_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    // create stream reader object
                    using (StreamReader sr = new StreamReader(fs1))
                    {
                        // read first line
                        string firstLine = sr.ReadLine();
                        //Console.WriteLine(firstLine);
                        // condition
                        if (firstLine == "stop" || firstLine == "exit" || firstLine == "quit" || firstLine == "end")
                        {
                            Console.WriteLine("The chronoamperometry measurement is stopped.");
                            System.Threading.Thread.Sleep(100);
                            // break main while loop
                            break;
                        }
                        else
                        {
                            // continue main while loop
                            continue;
                        }
                    }
                }

                // create new file stream object to avoid violation exception
                FileStream fs2 = new FileStream(path_to_stop_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                // create stream reader object
                using (StreamReader sr = new StreamReader(fs2))
                {
                    // read first line
                    string firstLine = sr.ReadLine();
                    //Console.WriteLine(firstLine);
                    // condition
                    if (firstLine == "stop" || firstLine == "exit" || firstLine == "quit" || firstLine == "end")
                    {
                        Console.WriteLine("The chronoamperometry measurement is stopped.");
                        System.Threading.Thread.Sleep(100);
                        // set current to zero
                        _comm.Potential = 0.0f;
                        // wait extra time
                        System.Threading.Thread.Sleep(1000);
                        // break main while loop
                        break;
                    }
                    else
                    {
                        Console.WriteLine(_measurement_is_completed);
                        // continue main while loop
                        continue;
                    }
                }
            }

            // detroy communication with PalmSens device
            _comm.BeginMeasurement -= _comm_BeginMeasurement;
            _comm.BeginReceiveCurve -= _comm_BeginReceiveCurve;
            _comm.EndMeasurement -= _comm_EndMeasurement;
            // set potential to zero
            _comm.Potential = 0.0f;
            // wait extra time
            System.Threading.Thread.Sleep(1000);
            // end chronoamperometry method
            method_amperometry.Dispose();

            // initialize multistep chronopotentiometry method
            PalmSens.Techniques.MultistepPotentiometry method_multisteppotentiometry = new PalmSens.Techniques.MultistepPotentiometry();
            // initialize 4 basic levels for multistep chronopotentiometry method
            PalmSens.Techniques.EILevel level1 = new PalmSens.Techniques.EILevel();
            PalmSens.Techniques.EILevel level2 = new PalmSens.Techniques.EILevel();

            // add levels to method
            method_multisteppotentiometry.Levels.Clear();
            method_multisteppotentiometry.Levels.Add(level1);
            method_multisteppotentiometry.Levels.Add(level2);

            // use voltage limits
            method_multisteppotentiometry.UseLimits = false; //Use voltage limits to stop the measurement when the voltage exceeds the defined limits

            // set flags for chronoamperometry
            _state_of_charging = false;
            _state_of_amperometry = false;
            _state_of_multisteppotentiometry = true;

            // main for loop for multistep chronopotentiometry method
            for (int index_0 = 0; index_0 < no_dc_currents; index_0++)
            {
                // print number of applied dc curent
                Console.WriteLine($"The number of applied DC current is: {index_0 + 1}");

                // define current range 
                if (applied_current_ranges_array[index_0] == "cr1mA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1mA);
                }
                else if (applied_current_ranges_array[index_0] == "cr10mA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr10mA);
                }
                else if (applied_current_ranges_array[index_0] == "cr100mA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100mA);
                }
                else if (applied_current_ranges_array[index_0] == "cr100pA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100pA);
                }
                else if (applied_current_ranges_array[index_0] == "cr1nA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1nA);
                }
                else if (applied_current_ranges_array[index_0] == "cr10nA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr10nA);
                }
                else if (applied_current_ranges_array[index_0] == "cr100nA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100nA);
                }
                else if (applied_current_ranges_array[index_0] == "cr1uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr10uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr10uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr100uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr100uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr2uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr2uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr4uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr4uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr8uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr8uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr16uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr16uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr32uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr32uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr125uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr125uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr250uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr250uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr500uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr500uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr5mA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr5mA);
                }
                else if (applied_current_ranges_array[index_0] == "cr6uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr6uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr13uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr13uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr25uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr25uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr50uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr50uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr200uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr200uA);
                }
                else if (applied_current_ranges_array[index_0] == "cr63uA")
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr63uA);
                }
                else
                {
                    method_multisteppotentiometry.AppliedCurrentRange = new CurrentRange(CurrentRanges.cr1mA);
                }

                // define number of cycles
                method_multisteppotentiometry.nCycles = no_cycles_array[index_0]; //Number of cycles

                // set interval time in multistep chronopotentiometry measurement
                method_multisteppotentiometry.IntervalTime = interval_times_array[index_0]; //Measures the current 10 times per second

                level1.Duration = duration_times_array[index_0];
                level1.Level = dc_currents_array[index_0]; //Sets a current of 1.0 (in the defined CurrentRange) for this level

                level2.Duration = duration_times_array[index_0];
                level2.Level = (-1.0f)*dc_currents_array[index_0]; //Sets a current of 1.0 (in the defined CurrentRange) for this level

                // additional settings for multistep chronopotentiometry method
                method_multisteppotentiometry.EquilibrationTime = 1.0f; //Equilabrates the cell at the defined potential for 1 second before starting the measurement
                method_multisteppotentiometry.Ranging.MinimumCurrentRange = new CurrentRange(CurrentRanges.cr10nA); //Min current range 10nA
                method_multisteppotentiometry.Ranging.MaximumCurrentRange = new CurrentRange(CurrentRanges.cr100mA); //Max current range 1mA

                //Subscribe to events
                _comm.BeginMeasurement += _comm_BeginMeasurement;
                _comm.BeginReceiveCurve += _comm_BeginReceiveCurve;
                _comm.EndMeasurement += _comm_EndMeasurement;

                //Start the measurement
                //errors = CommInvokeThreadSafe(() => { _measurement_is_completed = 0; string exit_code = _comm.Measure(method); Console.WriteLine($"Exit code is: {exit_code}"); return exit_code; });
                errors = CommInvokeThreadSafe(() => { _measurement_is_completed = 0; return _comm.Measure(method_multisteppotentiometry); });

                while (_measurement_is_completed == 0)
                {
                    //wait for the measurement to complete
                    System.Threading.Thread.Sleep(100);
                    // create new file stream object to avoid violation exception
                    FileStream fs1 = new FileStream(path_to_stop_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    // create stream reader object
                    using (StreamReader sr = new StreamReader(fs1))
                    {
                        // read first line
                        string firstLine = sr.ReadLine();
                        //Console.WriteLine(firstLine);
                        // condition
                        if (firstLine == "stop" || firstLine == "exit" || firstLine == "quit" || firstLine == "end")
                        {
                            Console.WriteLine("The multistep potentiometry measurement is stopped.");
                            System.Threading.Thread.Sleep(100);
                            // break main while loop
                            break;
                        }
                        else
                        {
                            // continue main while loop
                            continue;
                        }
                    }
                }

                // create new file stream object to avoid violation exception
                FileStream fs2 = new FileStream(path_to_stop_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                // create stream reader object
                using (StreamReader sr = new StreamReader(fs2))
                {
                    // read first line
                    string firstLine = sr.ReadLine();
                    //Console.WriteLine(firstLine);
                    // condition
                    if (firstLine == "stop" || firstLine == "exit" || firstLine == "quit" || firstLine == "end")
                    {
                        Console.WriteLine("The multistep potentiometry measurement is stopped.");
                        System.Threading.Thread.Sleep(100);
                        // set current to zero
                        _comm.Current = 0.0f;
                        // wait extra time
                        System.Threading.Thread.Sleep(1000);
                        // break main while loop
                        break;
                    }
                    else
                    {
                        Console.WriteLine(_measurement_is_completed);
                        // continue main while loop
                        continue;
                    }
                }
            }

            // detroy communication with PalmSens device
            _comm.BeginMeasurement -= _comm_BeginMeasurement;
            _comm.BeginReceiveCurve -= _comm_BeginReceiveCurve;
            _comm.EndMeasurement -= _comm_EndMeasurement;
            // set current to zero
            _comm.Current = 0.0f;
            // wait extra time
            System.Threading.Thread.Sleep(1000);
            // end multistep chronopotentiometry method
            method_multisteppotentiometry.Dispose();

            // dispose communication with PalmSens device
            _comm.Dispose();

            //Prevent program from terminating immediately after starting the measurement
            Console.ReadKey();
        }

        /// <summary>
        /// Thread safe invoker for CommManager.
        /// </summary>
        /// <typeparam name="T">The specified value type the delegate returns</typeparam>
        /// <param name="del">The delegate function (has to return a value).</param>
        /// <returns>The value</returns>
        private static T CommInvokeThreadSafe<T>(Func<T> del)
        {
            Task<T> t = _comm.ClientConnection.Run<T>(new Task<T>(del));
            t.Wait();
            return t.Result;
        }

        /// <summary>
        /// Thread safe invoker for CommManager.
        /// </summary>
        /// <param name="action">The action.</param>
        private static void CommInvokeThreadSafe(Action action)
        {
            _comm.ClientConnection.Run(action).Wait();
        }

        /// <summary>
        /// The connected devices CommManager
        /// </summary>
        public static CommManager _comm;

        /// <summary>
        /// The active curve containing the measurements results
        /// </summary>
        public static Curve _curve;

        /// <summary>
        /// Discovers the Emstat and PalmSens devices connected via USB.
        /// </summary>
        /// <returns>An array of the connected devices</returns>
        public static Device[] DiscoverConnectedDevices()
        {
            //Determine which devices to discover
            List<DeviceList.DiscoverDevicesFunc> discFuncs = new List<DeviceList.DiscoverDevicesFunc>(); //List of device discovery delegates
            discFuncs.Add(USBCDCDevice.DiscoverDevices); //Adds the delegate for discovering connected PalmSens 4 devices
            discFuncs.Add(FTDIDevice.DiscoverDevices); //Adds the delegate for discovering connected Emstat & PalmSens 3 devices

            //Discover the specified devices
            string errors;
            Device[] devices = new DeviceList(discFuncs).GetAvailableDevices(out errors);

            //Check whether errors occurred during discovery
            if (!string.IsNullOrEmpty(errors))
            {
                Console.WriteLine(errors);
                return null;
            }

            return devices;
        }

        /// <summary>
        /// Connects with the specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        public static void Connect(Device device)
        {
            //Connect to device
            try
            {
                device.Open(); //Open the device to allow a connection
                _comm = new CommManager(device); //Connect to the device
                Console.WriteLine($"Connected to {_comm.Device.ToString()}, {_comm.DeviceSerial.ToString()}");
            }
            catch (Exception ex)
            {
                device.Close();
                Console.WriteLine($"Could not connect: {ex.Message} in {ex.Source}");
                return;
            }
        }

        #region events
        /// <summary>
        /// Raised when the measurement has begun.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="newMeasurement">The new measurement.</param>
        private static void _comm_BeginMeasurement(object sender, ActiveMeasurement newMeasurement)
        {
            Console.WriteLine("");
            Console.WriteLine($"Measurement started.");
            Console.WriteLine("");
        }

        /// <summary>
        /// Raised when the curve containing the measurements results is created 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CurveEventArgs"/> instance containing the event data.</param>
        private static void _comm_BeginReceiveCurve(object sender, CurveEventArgs e)
        {
            _curve = e.GetCurve(); //Gets the reference to the active curve
            //subscribe to the curves events to write the data to the console
            _curve.NewDataAdded += _curve_NewDataAdded;
            _curve.Finished += _curve_Finished;

            Console.WriteLine("");
            Console.WriteLine($"Curve received.");
            Console.WriteLine("");
        }

        /// <summary>
        /// Raised when new data is added to the curve
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PalmSens.Data.ArrayDataAddedEventArgs"/> instance containing the event data.</param>
        private static void _curve_NewDataAdded(object sender, PalmSens.Data.ArrayDataAddedEventArgs e)
        {
            int startIndex = e.StartIndex; //the index of the first new data point added to the curve
            int count = e.Count; //the number of new data points added to the curve

            //Write the current readings to the console
            string xUnit = _curve.XUnit.ToString();
            string yUnit = _curve.YUnit.ToString();

            // define average potential value
            float average_potential = 0.0f;
            // define potential counter
            int potential_counter = 0;

            if (_state_of_charging == true)
            {
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    double xValue = _curve.GetXValue(i);
                    double yValue = _curve.GetYValue(i);
                    Console.WriteLine($"index = {i + 1}, {xValue.ToString("F1")} {xUnit}, {yValue.ToString("e3")} {yUnit}");
                    Console.WriteLine($"index = {i + 1}, {xValue.ToString("F1")} {xUnit}, {yValue.ToString("e3")} {yUnit}");

                    // define path to text fileto save measured data
                    string path_to_folder_with_measurements = @"C:\PalmSens_measurements";
                    string path_to_measurements = Path.Combine(path_to_folder_with_measurements, _filename_potentials);

                    // save data to text file
                    using (StreamWriter sw = new StreamWriter(path_to_measurements, append: true))
                    {
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sw.WriteLine($"{xValue}\t{yValue}\t{timestamp}\t");
                    }

                    // calculate average potential
                    average_potential += (float)yValue;
                    potential_counter++;
                }

                // calculate average potential value
                average_potential = average_potential / potential_counter;

                if (average_potential > _max_potential)
                {
                    _state_of_charging = false;
                    average_potential = 0.0f;
                    potential_counter = 0;
                }
            }

            // define average potential value
            float average_current = 0.0f;
            // define potential counter
            int current_counter = 0;

            if (_state_of_amperometry == true)
            {
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    double xValue = _curve.GetXValue(i);
                    double yValue = _curve.GetYValue(i);
                    Console.WriteLine($"index = {i + 1}, {xValue.ToString("F1")} {xUnit}, {yValue.ToString("e3")} {yUnit}");
                    Console.WriteLine($"index = {i + 1}, {xValue.ToString("F1")} {xUnit}, {yValue.ToString("e3")} {yUnit}");

                    // define path to text fileto save measured data
                    string path_to_folder_with_measurements = @"C:\PalmSens_measurements";
                    string path_to_measurements = Path.Combine(path_to_folder_with_measurements, _filename_amperometry);

                    // save data to text file
                    using (StreamWriter sw = new StreamWriter(path_to_measurements, append: true))
                    {
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sw.WriteLine($"{xValue}\t{yValue}\t{timestamp}\t");
                    }
                    
                    // calculate average current
                    average_current += (float)yValue;
                    current_counter++;      
                }

                // calculate average current value
                average_current = average_current / current_counter;

                if (_state_of_current_beginning == true)
                {
                    _current_beginning = average_current;
                    Console.WriteLine($"The current at the beginning of chronoamperometry is: {_current_beginning}");
                    _state_of_current_beginning = false;
                }

                if (Math.Abs(average_current) < (_amperometry_multiplication_factor * Math.Abs(_current_beginning)))
                {
                    _state_of_amperometry = false;
                    average_current = 0.0f;
                    current_counter = 0;
                }
            }

            if (_state_of_multisteppotentiometry == true)
            {
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    double xValue = _curve.GetXValue(i);
                    double yValue = _curve.GetYValue(i);
                    Console.WriteLine($"index = {i + 1}, {xValue.ToString("F1")} {xUnit}, {yValue.ToString("e3")} {yUnit}");
                    Console.WriteLine($"index = {i + 1}, {xValue.ToString("F1")} {xUnit}, {yValue.ToString("e3")} {yUnit}");

                    // define path to text fileto save measured data
                    string path_to_folder_with_measurements = @"C:\PalmSens_measurements";
                    string path_to_measurements = Path.Combine(path_to_folder_with_measurements, _filename_multisteppotentiometry);

                    // save data to text file
                    using (StreamWriter sw = new StreamWriter(path_to_measurements, append: true))
                    {
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sw.WriteLine($"{xValue}\t{yValue}\t{timestamp}\t");
                    }
                }
            }
        }

        /// <summary>
        /// Raised when the curve has finished receiving data
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void _curve_Finished(object sender, EventArgs e)
        {
            //Unsubscribe from the curves events to avoid memory leaks
            _curve.NewDataAdded -= _curve_NewDataAdded;
            _curve.Finished -= _curve_Finished;

            Console.WriteLine("");
            Console.WriteLine($"Curve finished.");
            Console.WriteLine("");

            // set measurement is completed to finish flag
            _measurement_is_completed = 1;
        }

        /// <summary>
        /// Raised when the measurement has ended
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void _comm_EndMeasurement(object sender, EventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine($"Measurement completed, {_curve.NPoints} data points received.");

            // Unsubscribe from the CommManager's events to avoid memory leaks
            _comm.BeginMeasurement -= _comm_BeginMeasurement;
            _comm.BeginReceiveCurve -= _comm_BeginReceiveCurve;
            _comm.EndMeasurement -= _comm_EndMeasurement;

            _measurementFinished?.TrySetResult(true);

            //Dispose data
            _curve.Dispose();
            //_comm.Dispose();

            Console.WriteLine("Press any key to exit");
        }

        static TaskCompletionSource<bool> _measurementFinished;

        #endregion
    }
}
