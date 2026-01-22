// Audio playback function for text-to-speech
window.playAudio = function (base64Audio) {
    try {
        // Convert base64 to blob
        const byteCharacters = atob(base64Audio);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: 'audio/mpeg' });
        
        // Create audio element and play
        const audioUrl = URL.createObjectURL(blob);
        const audio = new Audio(audioUrl);
        
        audio.onended = function() {
            URL.revokeObjectURL(audioUrl);
        };
        
        audio.onerror = function(e) {
            console.error('Audio playback error:', e);
            URL.revokeObjectURL(audioUrl);
        };
        
        audio.play().catch(error => {
            console.error('Failed to play audio:', error);
        });
    } catch (error) {
        console.error('Error processing audio:', error);
    }
};
