package main

import (
	"bytes"
	"fmt"
	"io"
	"net/http"
	"os"
)

func main() {
	speechKey := "b96fde6174d945ec8d91219bfc87896e"
	speechRegion := "eastus"

	url := fmt.Sprintf("https://%s.tts.speech.microsoft.com/cognitiveservices/v1", speechRegion)
	xmlPayload := `<speak version='1.0' xml:lang='uk-UA'>
    <voice xml:lang='uk-UA' xml:gender='Female' name='uk-UA-PolinaNeural'>
        привіт привіт 
    </voice>
</speak>`

	req, err := http.NewRequest("POST", url, bytes.NewBuffer([]byte(xmlPayload)))
	if err != nil {
		fmt.Println("Error creating request:", err)
		return
	}

	req.Header.Set("Ocp-Apim-Subscription-Key", speechKey)
	req.Header.Set("Content-Type", "application/ssml+xml")
	req.Header.Set("X-Microsoft-OutputFormat", "audio-16khz-128kbitrate-mono-mp3")
	req.Header.Set("User-Agent", "curl")

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Error making request:", err)
		return
	}
	defer resp.Body.Close()

	// Save the response to output.mp3
	outputFile, err := os.Create("output.mp3")
	if err != nil {
		fmt.Println("Error creating output file:", err)
		return
	}
	defer outputFile.Close()

	_, err = io.Copy(outputFile, resp.Body)
	if err != nil {
		fmt.Println("Error writing to output file:", err)
		return
	}

	fmt.Println("File saved as output.mp3")
}
