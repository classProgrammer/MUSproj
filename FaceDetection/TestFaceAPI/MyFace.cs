using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhatsappMessager;

namespace TestFaceAPI
{
    public class MyFace
    {
        const string subscriptionKey = "69b5a031d4b24aeda9ff5ace088b784f";
        const string faceEndpoint = "https://australiaeast.api.cognitive.microsoft.com/";


        private readonly IFaceClient faceClient = new FaceClient(
            new ApiKeyServiceClientCredentials(subscriptionKey),
            new System.Net.Http.DelegatingHandler[] { });

        private string friendGroup = "myfriends";
        private string baseImageFolder = "C:/Users/GeraldSpenlingwimmer/Desktop/trainingSet";

        public MyFace()
        {
            faceClient.Endpoint = faceEndpoint;
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

            Console.WriteLine(gerald.PersonId);
            Console.WriteLine(edith.PersonId);
            Console.WriteLine(helmut.PersonId);

            Console.WriteLine("Persons created");

            AddImageToPersonInGroup(friendGroup, gerald.PersonId, baseImageFolder + "/gerald", "Gerald");
            AddImageToPersonInGroup(friendGroup, edith.PersonId, baseImageFolder + "/edith", "Edith");
            AddImageToPersonInGroup(friendGroup, helmut.PersonId, baseImageFolder + "/helmut", "Helmut");
        }

        public async void Identify(string imagePath)
        {
            using (Stream s = File.OpenRead(imagePath))
            {
                var faces = await faceClient.Face.DetectWithStreamAsync(s);
                var faceIds = faces.Where(x => x.FaceId.HasValue).Select(face => face.FaceId.Value).ToArray();

                var results = await faceClient.Face.IdentifyAsync(faceIds, friendGroup);

                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.ToArray().Length == 0)
                    {
                        Console.WriteLine("No one identified");
                        string message = "Hello, An unknown person is waiting at your door";
                        SpeechProcessor.SpeechProcessorGS.Speak(message);

                        WhatsappSender.Send("");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.PersonGroupPerson.GetAsync(friendGroup, candidateId);

                        string message = $"Hello, {person.Name} is waiting at your door";
                        SpeechProcessor.SpeechProcessorGS.Speak(message);

                        Console.WriteLine("Identified as {0}", person.Name);
                        WhatsappSender.Send(person.Name);
                    }
                }
            }
        }


        public async void AddImageToPersonInGroup(string group, Guid personId, string directory, string personName)
        {

            foreach (string imagePath in Directory.GetFiles(directory, "*.jpg"))
            {
                using (Stream s = File.OpenRead(imagePath))
                {
                    // Detect faces in the image and add to Anna
                    await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(friendGroup, personId, s);
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
