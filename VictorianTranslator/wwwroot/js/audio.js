window.playAudio = function (base64Audio) {
    // Create audio element if it doesn't exist
    let audio = document.getElementById('victorianAudio');
    if (!audio) {
        audio = document.createElement('audio');
        audio.id = 'victorianAudio';
        document.body.appendChild(audio);
    }

    // Convert base64 to blob URL
    const byteCharacters = atob(base64Audio);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'audio/mpeg' });
    const url = URL.createObjectURL(blob);

    // Set source and play
    audio.src = url;
    audio.play();

    // Clean up old blob URL after playback
    audio.onended = function() {
        URL.revokeObjectURL(url);
    };
}
