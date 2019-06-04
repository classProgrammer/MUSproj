using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhatsappMessager;

namespace TestFaceAPI
{
    public class MyFace
    {
        private readonly IFaceClient faceClient;

        private string friendGroup = "myfriends";
        private string baseImageFolder = "C:/Users/GeraldSpenlingwimmer/Desktop/trainingSet";

        private static string configPath = @"C:/Users/GeraldSpenlingwimmer/Desktop/config/faceconfig.json";

        class Config
        {
            public string SubscriptionKey { get; set; }
            public string FaceEndpoint { get; set; }
        }


        public MyFace()
        {
            using (StreamReader r = new StreamReader(configPath))
            {
                string jsonString = r.ReadToEnd();
                Config config = JsonConvert.DeserializeObject<Config>(jsonString);
                faceClient = new FaceClient(
                    new ApiKeyServiceClientCredentials(config.SubscriptionKey),
                    new System.Net.Http.DelegatingHandler[] { });
                faceClient.Endpoint = config.FaceEndpoint;
            }
        }

        public async void Tutorial()
        {
            // create a person group
            await faceClient.PersonGroup.CreateAsync(friendGroup, "My Friends");
            Console.WriteLine("Group created");

            // create persons
            var gerald = await faceClient.PersonGroupPerson.CreateAsync(friendGroup, "gerald");
            var edith = await faceClient.PersonGroupPerson.CreateAsync(friendGroup, "edith");
            var helmut = await faceClient.PersonGroupPerson.CreateAsync(friendGroup, "helmut");

            Console.WriteLine("Persons created");

            AddImageToPersonInGroup(friendGroup, gerald.PersonId, baseImageFolder + "/gerald", "Gerald");
            AddImageToPersonInGroup(friendGroup, edith.PersonId, baseImageFolder + "/edith", "Edith");
            AddImageToPersonInGroup(friendGroup, helmut.PersonId, baseImageFolder + "/helmut", "Helmut");
        }

        public async void Identify(string imagePath)
        {
            try
            {
                using (Stream imageStream = File.OpenRead(imagePath))
                {
                    var faces = await faceClient.Face.DetectWithStreamAsync(imageStream);
                    var faceIds = faces.Where(x => x.FaceId.HasValue).Select(face => face.FaceId.Value).ToArray();

                    var results = await faceClient.Face.IdentifyAsync(faceIds, friendGroup);

                    foreach (var result in results)
                    {
                        if (result.Candidates.ToArray().Length == 0) // no match found
                        {
                            string message = "Hello, An unknown person is waiting at your door";
                            WhatsappSender.Send(message);
                            SpeechProcessor.SpeechProcessorGS.Speak(message);
                        }
                        else // match(es) found
                        {
                            // Get top 1 among all candidates returned
                            var bestMatch = result.Candidates[0];
                            var person = await faceClient.PersonGroupPerson.GetAsync(friendGroup, bestMatch.PersonId);
                            var message = $"{person.Name} is waiting at your door with {(int)(bestMatch.Confidence * 100)}% confidence";
                            WhatsappSender.Send(message);
                            SpeechProcessor.SpeechProcessorGS.Speak(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddImageToPersonInGroup(string group, Guid personId, string directory, string personName)
        {
            foreach (string imagePath in Directory.GetFiles(directory, "*.jpg"))
            {
                using (Stream imageStream = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to personId
                    await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(group, personId, imageStream);
                }
            }
            Console.WriteLine($"Images for {personName} added");
        }

        public async void TrainModel()
        {
            // train the groups model
            await faceClient.PersonGroup.TrainAsync(friendGroup);
            Console.WriteLine("trained model");
        }

        public async void TrainingStatus()
        {
            TrainingStatus trainingStatus = null;
            while (true)
            {
                trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(friendGroup);
                if (trainingStatus.Status != TrainingStatusType.Running)
                {
                    Console.WriteLine("Train model finished");
                    break;
                }

                await Task.Delay(10000);
            }
        }

        public async void Reset()
        {
            await faceClient.PersonGroup.DeleteAsync(friendGroup);
            Console.WriteLine($"Deleted Group {friendGroup}");
        }
    }
}
