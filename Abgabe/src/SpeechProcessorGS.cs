using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace SpeechProcessor
{
    public static class SpeechProcessorGS
    {
        private static SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        public static void Speak(string message)
        {
            Task.Factory.StartNew(() =>
            {
                // Configure the audio output.   
                speechSynthesizer.SetOutputToDefaultAudioDevice();

                // Speak a string.  
                speechSynthesizer.Speak(message);
            });
        }
    }
}
