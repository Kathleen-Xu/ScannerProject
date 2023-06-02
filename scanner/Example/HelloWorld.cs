using System;
public class HelloWorld
{
    public string info = "HelloWorld";
    public void SayHello()
    {
        if (info.Length > 0)
        {
            Console.WriteLine(info);
        }
    }
}
