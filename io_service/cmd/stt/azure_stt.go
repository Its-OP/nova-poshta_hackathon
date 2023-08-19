package main

import (
	"fmt"
	"io/ioutil"
	"log"

	"github.com/Its-OP/novaposhtahack/pkg/websocket"
)

func main() {
	speechKey := "b96fde6174d945ec8d91219bfc87896e"
	speechRegion := "eastus"
	audioFilePath := "/home/fort/Downloads/recording (9).wav"

	data, err := ioutil.ReadFile(audioFilePath)
	if err != nil {
		log.Fatal(err)
	}

	text, err := websocket.NewAzureSTT(speechRegion, speechKey).ConvertSpeechToText(data)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(text)
}
