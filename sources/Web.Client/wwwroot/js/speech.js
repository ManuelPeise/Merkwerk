// LP-007: read-aloud with the Web Speech API. Speech is generated on the device; no text leaves it.
const synth = window.speechSynthesis;

// Chrome may garbage-collect an utterance before 'end' fires – keep a reference.
let current = null;

export function isSupported() {
    return 'speechSynthesis' in window && 'SpeechSynthesisUtterance' in window;
}

// Voices load asynchronously (often empty on the first call, especially on Android).
export function getVoices(timeoutMs) {
    const map = () => synth.getVoices().map(v => ({
        name: v.name,
        lang: v.lang,
        voiceUri: v.voiceURI,
        localService: v.localService,
        isDefault: v.default,
    }));

    return new Promise(resolve => {
        if (synth.getVoices().length > 0) {
            resolve(map());
            return;
        }

        const done = () => {
            synth.removeEventListener('voiceschanged', done);
            clearTimeout(timer);
            resolve(map());
        };
        const timer = setTimeout(done, timeoutMs);
        synth.addEventListener('voiceschanged', done);
    });
}

// Resolves when speaking ended or failed, with the measured delay until the voice started.
export function speak(text, lang, voiceUri, rate, startTimeoutMs) {
    return new Promise(resolve => {
        if (synth.speaking || synth.pending) {
            synth.cancel();
        }

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = lang;
        utterance.rate = rate;
        const voice = synth.getVoices().find(v => v.voiceURI === voiceUri);
        if (voice) {
            utterance.voice = voice;
        }

        const requestedAt = performance.now();
        let startedAt = null;
        let settled = false;
        const finish = (ok, error) => {
            if (settled) {
                return;
            }
            settled = true;
            clearTimeout(startTimer);
            if (current === utterance) {
                current = null;
            }
            const now = performance.now();
            resolve({
                ok,
                error,
                startDelayMs: startedAt === null ? null : startedAt - requestedAt,
                durationMs: startedAt === null ? 0 : now - startedAt,
                voiceName: voice ? voice.name : null,
            });
        };

        const startTimer = setTimeout(() => {
            if (startedAt === null) {
                finish(false, 'no-start');
            }
        }, startTimeoutMs);

        utterance.onstart = () => { startedAt = performance.now(); };
        utterance.onend = () => finish(true, null);
        utterance.onerror = e => finish(false, e.error);

        current = utterance;
        synth.speak(utterance);
    });
}

export function cancel() {
    synth.cancel();
}
