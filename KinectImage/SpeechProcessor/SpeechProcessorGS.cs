using System.Speech.Synthesis;

namespace SpeechProcessor
{
    public static class SpeechProcessorGS
    {
        private static SpeechSynthesizer synth = new SpeechSynthesizer();

        public static void Speak(string message)
        {
            // Configure the audio output.   
            synth.SetOutputToDefaultAudioDevice();

            // Speak a string.  
            synth.Speak(message);
        }
    }
}
