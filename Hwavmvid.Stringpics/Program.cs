using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Oqtane.ChatHubs.Stringpics {

    public partial class Program {

        public static void Main(string[] args)
        {

            StringpicsOutputType typeparameter = StringpicsOutputType.undefined;

            if (args.Any())
                typeparameter = (StringpicsOutputType) Enum.Parse(typeof(StringpicsOutputType), args[0]);

            var stringpicsitellisense = new StringpicsItellisense();

            if (typeparameter == StringpicsOutputType.undefined)
            {
                StringpicsMethods stringpicsmethodsinstance = new StringpicsMethods();
                Type classtype = stringpicsmethodsinstance.GetType();

                List<MethodInfo> availablemethods = classtype.GetMethods().Where(methodinfo => methodinfo.GetCustomAttribute<StringpicsMethodAttribute>() != null).ToList();
                foreach (MethodInfo methodinfo in availablemethods)
                {
                    try
                    {
                        Console.WriteLine("__________________________________________");
                        Console.WriteLine(string.Concat("Method name: ", methodinfo.Name));
                        Console.WriteLine(stringpicsitellisense.GetStringPic(methodinfo.Name, StringpicsOutputType.console));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }                    
                }
            }
        }
    }

    public class StringpicsItellisense
    {
        public string? GetStringPic (string methodtype, StringpicsOutputType stringpictype)
        {
            StringpicsMethods stringpicsmethodsinstance = new StringpicsMethods();
            Type classtype = stringpicsmethodsinstance.GetType();
            MethodInfo? targetMethod = classtype.GetMethod(methodtype);

            if (targetMethod != null)
            {
                object? pic = targetMethod?.Invoke(stringpicsmethodsinstance, new[] { methodtype, stringpictype.ToString() });

                if (pic != null)
                    return pic as string;
            }

            return "No pic found.";
        }
    }

    public class StringpicsMethods
    {

        [StringpicsMethodAttribute]
        public string car(string methodtype, string stringpictype)
        {

            string picstring = string.Empty;
            string linebreak = stringpictype == StringpicsOutputType.html.ToString() ? "<br />" : "\n";

            if (!string.IsNullOrEmpty(methodtype) && !string.IsNullOrEmpty(stringpictype))
            {                
                picstring += @"********************" + linebreak;
                picstring += @"******-----*********" + linebreak;
                picstring += @"*****/     \********" + linebreak;
                picstring += @"***--       ----***" + linebreak;
                picstring += @"***O------------O***" + linebreak;
                picstring += @"********************" + linebreak;
            }

            return picstring;
        }

        [StringpicsMethodAttribute]
        public string water(string methodtype, string stringpictype)
        {
            string picstring = string.Empty;
            string linebreak = stringpictype == StringpicsOutputType.html.ToString() ? "<br />" : "\n";

            picstring += @"********************" + linebreak;
            picstring += @"********--**********" + linebreak;
            picstring += @"*******/  \*********" + linebreak;
            picstring += @"*******|  |*********" + linebreak;
            picstring += @"*******|  |*********" + linebreak;
            picstring += @"*******|__|*********" + linebreak;
            picstring += @"********************" + linebreak;

            return picstring;
        }

        [StringpicsMethodAttribute]
        public string burger(string methodtype, string stringpictype)
        {

            string picstring = string.Empty;
            string linebreak = stringpictype == StringpicsOutputType.html.ToString() ? "<br />" : "\n";

            picstring += @"********************" + linebreak;
            picstring += @"*****----------*****" + linebreak;
            picstring += @"****/          \****" + linebreak;
            picstring += @"**----------------**" + linebreak;
            picstring += @"**|>>>>>>><<<<<<<|**" + linebreak;
            picstring += @"**----------------**" + linebreak;
            picstring += @"****\          /****" + linebreak;
            picstring += @"*****----------*****" + linebreak;
            picstring += @"********************" + linebreak;

            return picstring;
        }
    }

    public class StringpicsMethodAttribute : Attribute {}

    public enum StringpicsOutputType
    {
        undefined,
        html,
        console,
    }

}