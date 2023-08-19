package websocket

import (
	"bytes"
	"fmt"
	"io"
	"net/http"
)

type AzureTTS struct {
	region string
	key    string
}

func NewAzureTTS(region, key string) *AzureTTS {
	return &AzureTTS{region: region, key: key}
}

func (tts *AzureTTS) ConvertTextToSpeech(text string) ([]byte, error) {
	url := fmt.Sprintf("https://%s.tts.speech.microsoft.com/cognitiveservices/v1", tts.region)
	xmlPayload := fmt.Sprintf(`<speak version='1.0' xml:lang='uk-UA'>
    <voice xml:lang='uk-UA' xml:gender='Female' name='uk-UA-PolinaNeural'>
        %s 
    </voice>
</speak>`, text)

	req, err := http.NewRequest("POST", url, bytes.NewBuffer([]byte(xmlPayload)))
	if err != nil {
		fmt.Println("Error creating request:", err)
		return nil, err
	}

	req.Header.Set("Ocp-Apim-Subscription-Key", tts.key)
	req.Header.Set("Content-Type", "application/ssml+xml")
	req.Header.Set("X-Microsoft-OutputFormat", "audio-16khz-128kbitrate-mono-mp3")
	req.Header.Set("User-Agent", "curl")

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Error making request:", err)
		return nil, err
	}
	defer resp.Body.Close()

	return io.ReadAll(resp.Body)
}
