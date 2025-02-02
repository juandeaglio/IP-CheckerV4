﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace IP_Checker
{
    class SessionInformationStorage
    {
        public void Serialize(MonitorInformation mi, string fileName)
        {
            // Create a hashtable of values that will eventually be serialized.

            // To serialize the hashtable and its key/value pairs,
            // you must first open a stream for writing.
            // In this case, use a file stream.
            FileStream fs = new FileStream(fileName, FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, mi);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            catch (IOException e)
            {
                Console.WriteLine("Failed to access file: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }
        public MonitorInformation Deserialize(string fileName, VPN_Stability_Monitor vpnMonitor)
        {
            // Declare the hashtable reference.
            MonitorInformation mi;

            // Open the file containing the data that you want to deserialize.
            FileStream fs = new FileStream(fileName, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Deserialize the hashtable from the file and
                // assign the reference to the local variable.
                mi = (MonitorInformation)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            catch(IOException e)
            {
                Console.WriteLine("Failed to access file: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

            // To prove that the table deserialized correctly,
            // display the key/value pairs.
            try
            {
                if (vpnMonitor.IsValidIP(mi.HomeIP))
                    Console.WriteLine(mi.HomeIP);
                else
                    Console.WriteLine("Uh-oh!");
                if (vpnMonitor.IsValidIP(mi.VPNIP))
                    Console.WriteLine(mi.VPNIP);
                else
                    Console.WriteLine("Uh-oh!");
                return mi;
            }
            catch
            {
                //Error!
                return new MonitorInformation();
            }

        }
    }
}
