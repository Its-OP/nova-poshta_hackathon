package main

import (
	"fmt"
	"io"
	"net/http"
	"os"
)

func main() {
	speechKey := "b96fde6174d945ec8d91219bfc87896e"
	speechRegion := "eastus"
	audioFilePath := "/files/output.wav"

	url := fmt.Sprintf("https://%s.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=uk-UA&format=detailed", speechRegion)

	file, err := os.Open(audioFilePath)
	if err != nil {
		fmt.Println("Error opening audio file:", err)
		return
	}
	defer file.Close()

	client := &http.Client{}

	req, err := http.NewRequest("POST", url, file)
	if err != nil {
		fmt.Println("Error creating request:", err)
		return
	}

	req.Header.Set("Ocp-Apim-Subscription-Key", speechKey)
	req.Header.Set("Content-Type", "audio/wav")

	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Error sending request:", err)
		return
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		fmt.Println("Error reading response:", err)
		return
	}

	fmt.Println("Response:", resp.Status)
	fmt.Println("Body:", string(body))
}
