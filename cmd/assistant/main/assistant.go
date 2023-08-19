package main

import (
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/Its-OP/novaposhtahack/pkg/speech_assistant/azure"
	ws "github.com/Its-OP/novaposhtahack/pkg/websocket"
)

func main1() {
	azureKey := os.Getenv("AZURE_KEY")
	if azureKey == "" {
		log.Fatal("AZURE_KEY environment variable is not set")
	}

	azureRegion := os.Getenv("AZURE_REGION")
	if azureRegion == "" {
		log.Fatal("AZURE_REGION environment variable is not set")
	}

	endpoint := fmt.Sprintf("https://%s.tts.speech.microsoft.com/cognitiveservices/v1", azureRegion)
	azureTTS := azure.NewAzureTTS(azureKey, endpoint)

	websocket := ws.NewWebSocket(azureTTS)

	http.HandleFunc("/audio-to-text", websocket.AudioToTextHandler)
	http.HandleFunc("/text-to-audio", websocket.TextToAudioHandler)
	fmt.Println("WebSocket server started on :8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}
