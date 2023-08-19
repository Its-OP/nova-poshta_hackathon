<template>
  <div class="d-flex justify-content-center">
    <div class="input-group mb-3">
      <input type="text" class="form-control" placeholder="Enter your message" v-model="message">

      <button class="btn btn-success" @click="sendMessage">Send Message</button>
      <button class="btn btn-primary" @click="startRecording" v-if="!isRecording">Start Recording</button>
      <button class="btn btn-danger" @click="stopRecording" v-if="isRecording">Stop Recording</button>
    </div>


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

    // TODO: specify the correct websocket URL
    const socket = new WebSocket('ws://localhost:8080/echo');

    onMounted(() => {
      socket.addEventListener('message', (event) => {
        const data = JSON.parse(event.data);
        if (data.type === 'wav') {
          playReceivedAudio(new Uint8Array(data.payload));
        } else if (data.type === 'text') {
          console.log('Received text message:', data.payload);
          // Handle the text message as needed
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
        saveRecordingToLocal(blob);

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
        payload: message.value
      };
      socket.send(JSON.stringify(data));
      message.value = '';
    };


    return {
      isRecording,
      message,
      startRecording,
      stopRecording,
      sendMessage,
    };
  }
};
</script>
