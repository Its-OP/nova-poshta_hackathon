package main

import (
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/Its-OP/novaposhtahack/pkg/websocket"
)

func main() {
	speechKey := os.Getenv("AZURE_KEY")
	if speechKey == "" {
		log.Fatal("AZURE_KEY environment variable is not set")
	}

	speechRegion := os.Getenv("AZURE_REGION")
	if speechRegion == "" {
		log.Fatal("AZURE_REGION environment variable is not set")
	}

	stt := websocket.NewAzureSTT(speechRegion, speechKey)
	tts := websocket.NewAzureTTS(speechRegion, speechKey)

	websocket := websocket.NewWebSocket(tts, stt)

	http.HandleFunc("/audio-to-text", websocket.AudioToTextHandler)
	http.HandleFunc("/text-to-audio", websocket.TextToAudioHandler)
	fmt.Println("WebSocket server started on :8080")
	log.Fatal(http.ListenAndServe("0.0.0.0:8080", nil))
}
