package main

import (
	"os"

	"github.com/Its-OP/novaposhtahack/pkg/websocket"
)

func main() {
	speechKey := "b96fde6174d945ec8d91219bfc87896e"
	speechRegion := "eastus"

	bytes, err := websocket.NewAzureTTS(speechRegion, speechKey).ConvertTextToSpeech("привіт привіт")
	if err != nil {
		panic(err)
	}

	// save to file
	err = os.WriteFile("/files/output.mp3", bytes, 0644)
	if err != nil {
		panic(err)
	}
}
