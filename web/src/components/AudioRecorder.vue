<template>
  <div class="d-flex justify-content-center">
    <button class="btn btn-primary" @click="startRecording" v-if="!isRecording">Start Recording</button>
    <button class="btn btn-danger" @click="stopRecording" v-if="isRecording">Stop Recording</button>
  </div>
</template>

<script>
import { ref } from 'vue';
import RecordRTC from 'recordrtc';

export default {
  name: 'AudioRecorder',
  setup() {
    const recorder = ref(null);
    const isRecording = ref(false);
    const mediaStream = ref(null);

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
      mediaStream.value = await navigator.mediaDevices.getUserMedia({ audio: true });
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
