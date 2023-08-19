package main

import (
	"os"

	"github.com/Its-OP/novaposhtahack/pkg/websocket"
)

func main() {
	speechKey := "b96fde6174d945ec8d91219bfc87896e"
	speechRegion := "eastus"

	text := `Для отримання відправлення, вам потрібно пред'явити один з наступних документів:

	✔️ посвідчення водія, видане в Україні;
	✔️ пенсійне посвідчення;
	✔️ студентський квиток;
	✔️ картку мігранта;
	✔️ посвідчення особи на повернення в Україну;
	✔️ дипломатичний паспорт України;
	✔️ службовий паспорт України;
	✔️ проїзний документ біженця;
	✔️ посвідчення учасника бойових дій;
	✔️ посвідчення особи, яка потребує додаткового захисту;
	✔️ проїзний документ особи, якій надано додатковий захист;
	✔️ посвідчення особи, яка потребує тимчасового захисту;
	✔️ картку ідентифікації бізнес-клієнта;
	✔️ картку клієнта пластикового або електронного (з мобільного додатка) формату.
	
	Якщо вартість відправлення менше ніж 30 000 гривень, ви можете пред'явити будь-який з вищезазначених документів. Якщо вартість відправлення 30 000 гривень або більше, вам потрібно буде надати документ, що підтверджує вашу особу, відповідно до переліку, зазначеного в пп. 12.2.1 – 12.2.11 цих Умов, та назвати СМС-код, який був надісланий на номер телефону, зазначений в експрес-накладній.`

	bytes, err := websocket.NewAzureTTS(speechRegion, speechKey).ConvertTextToSpeech(text)
	if err != nil {
		panic(err)
	}

	// save to file
	err = os.WriteFile("/files/output.mp3", bytes, 0644)
	if err != nil {
		panic(err)
	}
}
