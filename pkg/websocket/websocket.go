package websocket

import (
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

type SpeechAssistant interface {
	SpeechToText
	TextToSpeech
}

type WebSocket struct {
	upgrader        websocket.Upgrader
	speachAssistant SpeechAssistant
}

func NewWebSocket(speachAssistant SpeechAssistant) *WebSocket {
	return &WebSocket{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		speachAssistant: speachAssistant,
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

		text, err := ws.speachAssistant.ConvertSpeechToText(p)
		if err != nil {
			log.Println(err)
			return
		}

		if err := conn.WriteMessage(messageType, []byte("Converted to Text: "+text)); err != nil {
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

		audio, err := ws.speachAssistant.ConvertTextToSpeech(string(p))
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
