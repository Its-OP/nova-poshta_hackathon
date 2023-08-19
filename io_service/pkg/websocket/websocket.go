package websocket

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"

	"github.com/gorilla/websocket"
)

type SpeechToText interface {
	ConvertSpeechToText([]byte) (string, error)
}

type TextToSpeech interface {
	ConvertTextToSpeech(string) ([]byte, error)
}

type AudioPayload struct {
	Type    string `json:"type"`
	Payload []byte `json:"payload"`
}

type WebSocket struct {
	upgrader websocket.Upgrader
	tts      TextToSpeech
	stt      SpeechToText
}

func NewWebSocket(tts TextToSpeech, stt SpeechToText) *WebSocket {
	return &WebSocket{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		tts: tts,
		stt: stt,
	}
}

func (ws *WebSocket) AudioToTextHandler(w http.ResponseWriter, r *http.Request) {
	conn, err := ws.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println(err)
		return
	}
	defer conn.Close()

	for {
		messageType, p, err := conn.ReadMessage()
		if err != nil {
			log.Println(err)
			return
		}

		input := AudioPayload{}
		err = json.Unmarshal(p, &input)
		if err != nil {
			fmt.Println("Error unmarshaling payload:", err)
			return
		}
		text := ""
		if input.Type == "wav" {
			text, err = ws.stt.ConvertSpeechToText(input.Payload)
			fmt.Printf("text stt: %s\n", text)
			if err != nil {
				log.Println(err)
				return
			}
		} else {
			text = string(input.Payload)
			fmt.Printf("text direct: %s\n", text)
		}
		nlpResponse, err := sendToNlpProcessor(text)
		if err != nil {
			log.Println(err)
			return
		}

		audio, err := ws.tts.ConvertTextToSpeech(string(nlpResponse))
		if err != nil {
			log.Println(err)
			return
		}

		payload := AudioPayload{
			Type:    "wav",
			Payload: audio,
		}

		payloadBytes, err := json.Marshal(payload)
		if err != nil {
			fmt.Println("Error marshaling payload:", err)
			return
		}

		if err := conn.WriteMessage(messageType, payloadBytes); err != nil {
			log.Println(err)
			return
		}

		payload = AudioPayload{
			Type:    "text",
			Payload: []byte(nlpResponse),
		}

		payloadBytes, err = json.Marshal(payload)
		fmt.Println("payloadBytes", string(payloadBytes))
		if err != nil {
			fmt.Println("Error marshaling payload:", err)
			return
		}

		if err := conn.WriteMessage(messageType, payloadBytes); err != nil {
			log.Println(err)
			return
		}
	}
}

func (ws *WebSocket) TextToAudioHandler(w http.ResponseWriter, r *http.Request) {
	conn, err := ws.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println(err)
		return
	}
	defer conn.Close()

	for {
		messageType, p, err := conn.ReadMessage()
		if err != nil {
			log.Println(err)
			return
		}

		audio, err := ws.tts.ConvertTextToSpeech(string(p))
		if err != nil {
			log.Println(err)
			return
		}

		if err := conn.WriteMessage(messageType, audio); err != nil {
			log.Println(err)
			return
		}
	}
}

func sendToNlpProcessor(input string) (string, error) {
	url := "http://localhost:7034/processor/process"
	payload := map[string]string{
		"text": input,
	}

	payloadBytes, err := json.Marshal(payload)
	if err != nil {
		fmt.Println("Error marshaling payload:", err)
		return "", err
	}
	fmt.Printf("payloadBytes: %s\n", string(payloadBytes))
	req, err := http.NewRequest("POST", url, bytes.NewBuffer(payloadBytes))
	if err != nil {
		fmt.Println("Error creating request:", err)
		return "", err
	}

	req.Header.Set("Content-Type", "application/json")

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		fmt.Println("Error making request:", err)
		return "", err
	}
	defer resp.Body.Close()

	respBody, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		fmt.Println("Error reading response body:", err)
		return "", err
	}

	var response map[string]string
	err = json.Unmarshal(respBody, &response)
	if err != nil {
		fmt.Println("Error unmarshaling response:", err)
		return "", err
	}

	if text, ok := response["text"]; ok {
		fmt.Println("Response text:", text)
		return text, nil
	} else {
		fmt.Println("Response did not contain the 'text' field.")
		return "", err
	}
}
