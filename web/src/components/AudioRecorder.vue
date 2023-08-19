<template>
  <div class="d-flex justify-content-center">
    <button class="btn btn-primary" @click="startRecording" v-if="!isRecording">Start Recording</button>
    <button class="btn btn-danger" @click="stopRecording" v-if="isRecording">Stop Recording</button>
  </div>
</template>

<script>
import {ref} from 'vue';
import RecordRTC from 'recordrtc';

export default {
  name: 'AudioRecorder',
  setup() {
    // const socket = this.$socket;
    const recorder = ref(null);
    const isRecording = ref(false);
    const mediaStream = ref(null);

    const startRecording = async () => {
      mediaStream.value = await navigator.mediaDevices.getUserMedia({ audio: true });
      recorder.value = new RecordRTC(mediaStream.value, {type: 'audio'});
      recorder.value.startRecording();
      isRecording.value = true;
    };

    const stopRecording = () => {
      recorder.value.stopRecording(async () => {
        const blob = recorder.value.getBlob();
        const reader = new FileReader();
        reader.readAsArrayBuffer(blob);
        reader.onloadend = (event) => {
          // socket.emit('send-audio', reader.result);
        };
        isRecording.value = false;
      });
      if (mediaStream.value) {
        mediaStream.value.getTracks().forEach(track => track.stop());
        mediaStream.value = null;
      }
    };

    return {
      isRecording,
      startRecording,
      stopRecording
    };
  }
};
</script>
