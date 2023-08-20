<!--app with audio recorder in the middle-->
<template>
  <div id="app">
    <div class="container">
      <div class="row">
        <div class="col-12">
          <h1 class="text-center">NOVA CHAT</h1>
          <AudioRecorder/>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import AudioRecorder from "@/components/AudioRecorder.vue";
</script>
