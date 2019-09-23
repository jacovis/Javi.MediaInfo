using System;
using System.Reflection;
using Javi.MediaInfo;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // change this line to the location of the  appropriate version (32 or 64 bit) of the mediainfo.dll
            string MediaInfoDLLFullName = @".\MediaInfo.dll";

            MediaInfo MediaInfo = new MediaInfo(MediaInfoDLLFullName);
            MediaInfo.ReadMediaInformation(@"Sample.mp4");

            Console.WriteLine("");
            Console.WriteLine("General");
            Console.WriteLine("----------------------");
            OutputPropertyValues(MediaInfo.General);

            for (int i = 0; i < MediaInfo.Video.Count; i++)
            {
                Console.WriteLine("");
                Console.WriteLine($"Video stream {i}");
                Console.WriteLine("----------------------");
                OutputPropertyValues(MediaInfo.Video[i]);
            }

            for (int i = 0; i < MediaInfo.Audio.Count; i++)
            {
                Console.WriteLine("");
                Console.WriteLine($"Audio stream {i}");
                Console.WriteLine("----------------------");
                OutputPropertyValues(MediaInfo.Audio[i]);
            }

            for (int i = 0; i < MediaInfo.Text.Count; i++)
            {
                Console.WriteLine("");
                Console.WriteLine($"Text stream {i}");
                Console.WriteLine("----------------------");
                OutputPropertyValues(MediaInfo.Text[i]);
            }

            Console.WriteLine("");
            Console.WriteLine("Information");
            Console.WriteLine("----------------------");
            foreach (var s in MediaInfo.Information)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine("");
            Console.WriteLine("Press ENTER to close...");
            Console.ReadLine();
        }

        private static void OutputPropertyValues(object obj)
        {
            foreach (MemberInfo m in obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                var fi = m as FieldInfo;
                var pi = m as PropertyInfo;
                if (fi != null || pi != null)
                {
                    Console.WriteLine(m.Name + "=" + ValueToString(fi != null ? fi.GetValue(obj) : pi.GetValue(obj)));
                }
            }
        }

        private static string ValueToString(object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            else if (obj is ValueType || obj is string)
            {
                return obj.ToString();
            }
            else
            {
                return "{" + obj.ToString() + "}";
            }
        }
    }
}
