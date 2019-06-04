using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace TestFaceAPI
{
    class Program
    {       
        static void Main(string[] args)
        {


            var tut = new MyFace();
            //tut.Reset();
            
            //tut.Tutorial();
            //tut.TrainModel();
            //tut.TrainingStatus();
            tut.Identify("C:/Users/GeraldSpenlingwimmer/Desktop/testImages/gerald/2.jpg");
            while (true) { }
        }
    }
}
