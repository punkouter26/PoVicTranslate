// Function to play Base64 encoded audio data
window.playAudio = function (base64Audio) {
    try {
        // Create audio element if it doesn't exist, or reuse existing one
        let audio = document.getElementById('victorianTtsAudio');
        if (!audio) {
            console.log("Creating new audio element for TTS.");
            audio = document.createElement('audio');
            audio.id = 'victorianTtsAudio';
            // Append somewhere, or maybe not needed if just playing directly
            // document.body.appendChild(audio); 
        } else {
             console.log("Reusing existing audio element for TTS.");
        }

        // Decode Base64
        const byteCharacters = atob(base64Audio);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        
        // Determine MIME type (Azure Speech SDK often outputs MP3 with default settings)
        // Adjust if using a different format like WAV (audio/wav) or OGG (audio/ogg)
        const blob = new Blob([byteArray], { type: 'audio/mpeg' }); 
        
        // Create a URL for the blob
        const url = URL.createObjectURL(blob);
        console.log("Created Blob URL for audio.");

        // Set source and play
        audio.src = url;
        audio.play()
            .then(() => {
                console.log("Audio playback started.");
            })
            .catch(error => {
                 console.error("Audio playback failed:", error);
                 // Clean up blob URL even if playback fails
                 URL.revokeObjectURL(url);
            });

        // Clean up the blob URL once playback is finished
        audio.onended = function() {
            console.log("Audio playback finished, revoking Blob URL.");
            URL.revokeObjectURL(url);
        };
         audio.onerror = function() {
             console.error("Error occurred during audio playback, revoking Blob URL.");
             URL.revokeObjectURL(url);
         };

    } catch (error) {
        console.error("Error in playAudio function:", error);
    }
}
