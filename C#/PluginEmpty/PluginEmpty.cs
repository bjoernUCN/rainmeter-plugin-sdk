using Rainmeter;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;


// Overview: This is a blank canvas on which to build your plugin
// Note: GetString, ExecuteBang and an unnamed function for use as a section variable
// have been commented out. If you need GetString, ExecuteBang, and/or section variables 
// and you have read what they are used for from the SDK docs, uncomment the function(s)
// and/or add a function name to use for the section variable function(s). 
// Otherwise leave them commented out (or get rid of them)!

namespace PluginSqlTable
{
    class Measure
    {
        public string sql;
        public string connectionString;
        public string output="";
        static public implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        public IntPtr buffer = IntPtr.Zero;

        /*internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            //sql = api.ReadString("sql", "Server=hildur.ucn.dk; Initial Catalog=DMA-CSD-V222_10434661;User Id=DMA-CSD-V222_10434661;Password=Password1!;");
            //connectionString = api.ReadString("con", "select * from MovieInfos");
        }*/
    }

    public class Plugin
    {
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
            Rainmeter.API api = (Rainmeter.API)rm;
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)data;
            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
            }
            GCHandle.FromIntPtr(data).Free();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)data;
            Rainmeter.API api = (Rainmeter.API)rm;

            measure.sql = api.ReadString("sql", "");
            measure.connectionString = api.ReadString("con", "");
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)data;
            //measure.output = GetTable(measure.sql, measure.connectionString);
            //if (measure.output.Length > 0)
            //   return 1.0;
            return 0.4;
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)data;
            string output = "";
            try
            {
                output = GetTable(measure.sql, measure.connectionString);

            }
            catch (Exception)
            {

            }


            if (measure.buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(measure.buffer);
                measure.buffer = IntPtr.Zero;
            }


            measure.buffer = Marshal.StringToHGlobalUni(output);
            return measure.buffer;
        }


        public static string GetTable(string sql, string connectionString)
        {
            SqlConnection sqlCON = new SqlConnection(connectionString);
            sqlCON.Open();

            SqlCommand command = new SqlCommand(sql, sqlCON);
            DbDataReader dataReader = command.ExecuteReader();

            List<int> rowLongest = new List<int>(); //How long are the rows?
            List<List<string>> lines = new List<List<string>>(); //The rows we needs to get, and the fields as strings within

            int i = 0;

            List<string> columns = new List<string>(); //Get column header names
            for (int _i = 0; _i < dataReader.FieldCount; _i++)
                columns.Add(dataReader.GetName(_i));

            lines.Add(new List<string>()); //add names to rows
            lines[0].AddRange(columns);

            for (int iC = 0; iC < lines[0].Count; iC++)
            {
                TryLongest(iC, lines[0][iC].Length);
            }
            i++;

            while (dataReader.Read())
            {
                lines.Add(new List<string>());

                for (int o = 0; o < dataReader.FieldCount; o++)
                {
                    lines[i].Add(dataReader.GetValue(o).ToString());
                    TryLongest(o, lines[i][o].Length);
                }
                i++;
            }
            dataReader.Close();

            string verticalSeperator = "-";

            List<string> fillerBar = new List<string>();
            for (int iB = 0; iB < rowLongest.Count; iB++)
            {
                fillerBar.Add(new String(verticalSeperator.First(), rowLongest[iB]));
            }
            lines.Insert(1, fillerBar);

            string output = "";
            foreach (List<string> columnFields in lines)
            {
                for (int iS = 0; iS < columnFields.Count; iS++)
                {
                    output += (columnFields[iS]);
                    if (iS + 1 < columnFields.Count)
                        output += Spacing(columnFields[iS].Length, rowLongest[iS]);
                }
                output += ("#CRLF#");
            }

            return output;

            string Spacing(int input, int longest)
            {
                string horizontalSeperator = " | ";

                int spaceing = 0 + (longest - input);
                if (spaceing < 0) spaceing = 0;

                return new String(' ', spaceing) + horizontalSeperator;
            }

            void TryLongest(int pos, int value)
            {
                if (rowLongest.Count <= pos)
                    rowLongest.Add(0);

                if (rowLongest[pos] < value)
                    rowLongest[pos] = value;
            }

        }


    }
}

