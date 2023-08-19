package websocket

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
)

type AzureSTT struct {
	region string
	key    string
}

func NewAzureSTT(region, key string) *AzureSTT {
	return &AzureSTT{region: region, key: key}
}

func (stt *AzureSTT) ConvertSpeechToText(audio []byte) (string, error) {
	url := fmt.Sprintf("https://%s.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=uk-UA&format=detailed", stt.region)

	client := &http.Client{}

	req, err := http.NewRequest("POST", url, bytes.NewReader(audio))
	if err != nil {
		fmt.Println("Error creating request:", err)
		return "", err
	}

	req.Header.Set("Ocp-Apim-Subscription-Key", stt.key)
	req.Header.Set("Content-Type", "audio/wav")

	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Error sending request:", err)
		return "", err
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		fmt.Println("Error reading response:", err)
		return "", err
	}

	// fmt.Println("Response:", resp.Status)
	// fmt.Println("Body:", extractText(body))
	return extractText(body), nil
}

func extractText(jsonData []byte) string {

	var data map[string]interface{}
	if err := json.Unmarshal(jsonData, &data); err != nil {
		fmt.Println("Error:", err)
		return ""
	}

	nBestList := data["NBest"].([]interface{})
	if len(nBestList) > 0 {
		nBestItem := nBestList[0].(map[string]interface{})
		displayValue := nBestItem["Display"].(string)
		return displayValue
	} else {
		fmt.Println("No NBest data found.")
	}
	return ""
}
