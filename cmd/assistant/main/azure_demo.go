package main

import (
	"bufio"
	"encoding/binary"
	"fmt"
	"io"
	"os"
	"strings"
	"time"

	"github.com/Microsoft/cognitive-services-speech-sdk-go/speech"
)

func synthesizeStartedHandler(event speech.SpeechSynthesisEventArgs) {
	defer event.Close()
	fmt.Println("Synthesis started.")
}

func synthesizingHandler(event speech.SpeechSynthesisEventArgs) {
	defer event.Close()
	fmt.Printf("Synthesizing, audio chunk size %d.\n", len(event.Result.AudioData))
}

func synthesizedHandler(event speech.SpeechSynthesisEventArgs) {
	defer event.Close()
	fmt.Printf("Synthesized, audio length %d.\n", len(event.Result.AudioData))
}

func cancelledHandler(event speech.SpeechSynthesisEventArgs) {
	defer event.Close()
	fmt.Println("Received a cancellation.")
}

type WavHeader struct {
	RiffID        [4]byte
	RiffSize      uint32
	WaveID        [4]byte
	FmtID         [4]byte
	FmtSize       uint32
	AudioFormat   uint16
	NumChannels   uint16
	SampleRate    uint32
	ByteRate      uint32
	BlockAlign    uint16
	BitsPerSample uint16
	DataID        [4]byte
	DataSize      uint32
}

// Function to create a WAV header
func createWavHeader(dataSize uint32) WavHeader {
	return WavHeader{
		RiffID:        [4]byte{'R', 'I', 'F', 'F'},
		RiffSize:      36 + dataSize,
		WaveID:        [4]byte{'W', 'A', 'V', 'E'},
		FmtID:         [4]byte{'f', 'm', 't', ' '},
		FmtSize:       16,
		AudioFormat:   1,
		NumChannels:   1,
		SampleRate:    16000,
		ByteRate:      32000,
		BlockAlign:    2,
		BitsPerSample: 16,
		DataID:        [4]byte{'d', 'a', 't', 'a'},
		DataSize:      dataSize,
	}
}

// Function to save audio data to a .wav file with a proper header
func saveToWav(filename string, audioData []byte) error {
	file, err := os.Create(filename)
	if err != nil {
		return err
	}
	defer file.Close()

	header := createWavHeader(uint32(len(audioData)))
	binary.Write(file, binary.LittleEndian, header)
	file.Write(audioData)

	return nil
}

func main() {
	subscription := "b96fde6174d945ec8d91219bfc87896e"
	region := "eastus"

	speechConfig, err := speech.NewSpeechConfigFromSubscription(subscription, region)
	if err != nil {
		fmt.Println("Got an error: ", err)
		return
	}
	defer speechConfig.Close()

	speechConfig.SetSpeechSynthesisLanguage("uk-UA")

	speechSynthesizer, err := speech.NewSpeechSynthesizerFromConfig(speechConfig, nil)
	if err != nil {
		fmt.Println("Got an error: ", err)
		return
	}
	defer speechSynthesizer.Close()

	speechSynthesizer.SynthesisStarted(synthesizeStartedHandler)
	speechSynthesizer.Synthesizing(synthesizingHandler)
	speechSynthesizer.SynthesisCompleted(synthesizedHandler)
	speechSynthesizer.SynthesisCanceled(cancelledHandler)

	for {
		fmt.Printf("Enter some text that you want to speak, or enter empty text to exit.\n> ")
		text, _ := bufio.NewReader(os.Stdin).ReadString('\n')
		text = strings.TrimSuffix(text, "\n")
		if len(text) == 0 {
			break
		}

		// StartSpeakingTextAsync sends the result to channel when the synthesis starts.
		task := speechSynthesizer.StartSpeakingTextAsync(text)
		var outcome speech.SpeechSynthesisOutcome
		select {
		case outcome = <-task:
		case <-time.After(60 * time.Second):
			fmt.Println("Timed out")
			return
		}
		defer outcome.Close()
		if outcome.Error != nil {
			fmt.Println("Got an error: ", outcome.Error)
			return
		}

		// In most cases, we want to streaming receive the audio to lower the latency.
		// We can use AudioDataStream to do so.
		stream, err := speech.NewAudioDataStreamFromSpeechSynthesisResult(outcome.Result)
		defer stream.Close()
		if err != nil {
			fmt.Println("Got an error: ", err)
			return
		}

		var all_audio []byte
		audio_chunk := make([]byte, 2048)
		for {
			n, err := stream.Read(audio_chunk)

			if err == io.EOF {
				break
			}

			all_audio = append(all_audio, audio_chunk[:n]...)
		}

		fmt.Printf("Read [%d] bytes from audio data stream.\n", len(all_audio))
		// save the audio to file
		err = saveToWav("output.wav", all_audio)
		if err != nil {
			fmt.Println("Failed to save audio to .wav file:", err)
		} else {
			fmt.Println("Audio saved to output.wav")
		}
	}
}
