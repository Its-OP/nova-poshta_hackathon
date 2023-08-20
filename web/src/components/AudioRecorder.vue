<template>
  <div class="d-flex justify-content-center">
    <div class="input-group mb-3">
      <input type="text" class="form-control" placeholder="Enter your message" v-model="message">

      <button class="btn btn-success" @click="sendMessage">Send Message</button>
      <button class="btn btn-primary" @click="startRecording" v-if="!isRecording">Start Recording</button>
      <button class="btn btn-danger" @click="stopRecording" v-if="isRecording">Stop Recording</button>
      <button class="btn btn-outline-danger" @click="deleteHistory">Delete history</button>
    </div>
  </div>
  <div class="input-group mb-3">
    <p v-if="receivedTextMessage">Асистент: </p> {{ receivedTextMessage }}
  </div>
</template>

<script>
import {onMounted, onUnmounted, ref} from 'vue';
import RecordRTC from 'recordrtc';

export default {
  name: 'AudioRecorder',
  setup() {
    const recorder = ref(null);
    const isRecording = ref(false);
    const mediaStream = ref(null);
    const message = ref(null);
    const receivedTextMessage = ref('');


    // TODO: specify the correct websocket URL
    const socket = new WebSocket('ws://localhost:8080/audio-to-text');

    function base64ToArrayBuffer(base64) {
      var binaryString = atob(base64);
      var bytes = new Uint8Array(binaryString.length);
      for (var i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
      }
      return bytes.buffer;
    }

    function strToUtf8Bytes(str) {
      const utf8 = [];
      for (let ii = 0; ii < str.length; ii++) {
        let charCode = str.charCodeAt(ii);
        if (charCode < 0x80) utf8.push(charCode);
        else if (charCode < 0x800) {
          utf8.push(0xc0 | (charCode >> 6), 0x80 | (charCode & 0x3f));
        } else if (charCode < 0xd800 || charCode >= 0xe000) {
          utf8.push(0xe0 | (charCode >> 12), 0x80 | ((charCode >> 6) & 0x3f), 0x80 | (charCode & 0x3f));
        } else {
          ii++;
          // Surrogate pair:
          // UTF-16 encodes 0x10000-0x10FFFF by subtracting 0x10000 and
          // splitting the 20 bits of 0x0-0xFFFFF into two halves
          charCode = 0x10000 + (((charCode & 0x3ff) << 10) | (str.charCodeAt(ii) & 0x3ff));
          utf8.push(
              0xf0 | (charCode >> 18),
              0x80 | ((charCode >> 12) & 0x3f),
              0x80 | ((charCode >> 6) & 0x3f),
              0x80 | (charCode & 0x3f),
          );
        }
      }
      return utf8;
    }

    function utf8FromBinary(encoded) {
      return decodeURIComponent(escape(window.atob(encoded)));
    }


    onMounted(() => {
      socket.addEventListener('message', (event) => {
        console.log("Received response")
        const data = JSON.parse(event.data);
        console.log(data.payload)
        if (data.type === 'wav') {
          playReceivedAudio(base64ToArrayBuffer(data.payload));
        } else if (data.type === 'text') {
          console.log('Received text message:', data.payload);
          receivedTextMessage.value = utf8FromBinary(data.payload);
        }
      });
    });

    onUnmounted(() => {
      socket.close();
    });

    const playReceivedAudio = (audioData) => {
      const blob = new Blob([audioData], {type: 'audio/wav'});
      const audio = new Audio(URL.createObjectURL(blob));
      audio.play();
    };

    const saveRecordingToLocal = (blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.style.display = 'none';
      a.href = url;
      a.download = 'recording.wav';
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
    };

    const startRecording = async () => {
      mediaStream.value = await navigator.mediaDevices.getUserMedia({audio: true});
      recorder.value = new RecordRTC(mediaStream.value, {
        type: 'audio',
        mimeType: 'audio/wav',
        recorderType: RecordRTC.StereoAudioRecorder,
        numberOfAudioChannels: 1
      });
      recorder.value.startRecording();
      isRecording.value = true;
    };

    const stopRecording = () => {
      recorder.value.stopRecording(() => {
        const blob = recorder.value.getBlob();
        // saveRecordingToLocal(blob);

        const reader = new FileReader();
        reader.readAsArrayBuffer(blob);
        reader.onloadend = (event) => {
          const byteArray = new Uint8Array(reader.result);
          const data = {
            type: 'wav',
            payload: Array.from(byteArray)
          };
          socket.send(JSON.stringify(data));
        };

        isRecording.value = false;
      });

      if (mediaStream.value) {
        mediaStream.value.getTracks().forEach(track => track.stop());
        mediaStream.value = null;
      }
    };

    const sendMessage = (e) => {
      const data = {
        type: 'text',
        payload: strToUtf8Bytes(message.value)
      };
      console.log("Sending request")
      socket.send(JSON.stringify(data));
      message.value = '';
    };

    const deleteHistory = async () => {
      try {
        const response = await fetch('http://localhost:7034/processor/erase', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          }
        });

        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const responseData = await response.json();
        console.log('History deleted:', responseData);
      } catch (error) {
        console.error('There was a problem with the fetch operation:', error.message);
      }
    };

    return {
      isRecording,
      message,
      startRecording,
      stopRecording,
      sendMessage,
      deleteHistory,
      receivedTextMessage,
    };
  }
};
</script>
