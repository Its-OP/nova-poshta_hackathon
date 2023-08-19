package azure

import (
	"bytes"
	"io"
	"net/http"
)

type SpeechAssistant struct {
	client          *http.Client
	SubscriptionKey string
	Endpoint        string
}

func NewAzureTTS(subscriptionKey string, endpoint string) *SpeechAssistant {
	return &SpeechAssistant{
		client:          &http.Client{},
		SubscriptionKey: subscriptionKey,
		Endpoint:        endpoint}
}

func (sa *SpeechAssistant) ConvertTextToSpeech(text string) ([]byte, error) {
	req, err := http.NewRequest("POST", sa.Endpoint, bytes.NewBuffer([]byte(text)))
	if err != nil {
		return nil, err
	}

	req.Header.Add("Content-Type", "application/ssml+xml")
	req.Header.Add("Authorization", "Bearer "+sa.SubscriptionKey)
	req.Header.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm")

	resp, err := sa.client.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	data, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, err
	}

	return data, nil
}

func (sa *SpeechAssistant) ConvertSpeechToText(data []byte) (string, error) {
	req, err := http.NewRequest("POST", sa.Endpoint, bytes.NewBuffer(data))
	if err != nil {
		return "", err
	}

	req.Header.Add("Content-Type", "audio/wav")

	resp, err := sa.client.Do(req)
	if err != nil {
		return "", err
	}

	defer resp.Body.Close()

	data, err = io.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	return string(data), nil
}
