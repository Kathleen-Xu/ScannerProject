using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string d = Directory.GetCurrentDirectory();
            string code = @"<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" 
                    xmlns:controls=""clr-namespace:TestDemo.Controls"">
    <Style TargetType=""{x:Type controls:CountButton}"" x:Key=""DefaultCountButtonStyle"">
        <Setter Property=""Foreground"" Value=""DarkGreen"" />
        <Setter Property=""Template"">
            <Setter.Value>
                <ControlTemplate TargetType=""{x:Type controls:CountButton}"">
                    <Button Name=""B1"" Content=""{TemplateBinding Content}"" Background=""Red"" />
                    <ControlTemplate.Triggers>
                        <Trigger Property=""Count"" Value=""5"">
                            <Setter TargetName=""B1"" Property=""Background"" Value=""Green""/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
";
            /*ScannerUsage scannerUsage = new ScannerUsage("xaml", code, new string[] { }, null);
            string result = scannerUsage.Migrate();*/
            //Console.WriteLine(result);
        }
    }
}
