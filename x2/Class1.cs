using System.Runtime.Loader;

namespace x2
{
    public class Class1
    {
        public Class1()
        {
            Init();
        }

        public void Init()
        {
            Console.WriteLine(new x3.Class1().X);
        }

        public class SubClass2 : x3.Class1
        {
            public override string X => "x2";
        }
    }
}