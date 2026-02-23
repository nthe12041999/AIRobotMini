const express = require('express');
const cors = require('cors');
const { Communicate } = require('edge-tts-universal');

const app = express();
const PORT = 3000;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.static('.')); // Serve static files

// API endpoint để tạo audio từ text
app.post('/api/tts', async (req, res) => {
    try {
        const { text, voice } = req.body;

        if(!text) {
            return res.status(400).json({ error: 'Text is required' });
        }

        const voiceName = voice || 'vi-VN-HoaiMyNeural';

        console.log(`Generating TTS for: "${text}" with voice: ${voiceName}`);

        // Dùng Communicate API thay vì EdgeTTS (stable hơn)
        const communicate = new Communicate(text, { voice: voiceName });

        console.log('⏳ Streaming audio...');
        const buffers = [];

        for await(const chunk of communicate.stream()) {
            if(chunk.type === 'audio' && chunk.data) {
                buffers.push(chunk.data);
                console.log(`  Received chunk: ${chunk.data.length} bytes`);
            }
        }

        const buffer = Buffer.concat(buffers);
        console.log(`✓ Generated ${buffer.length} bytes of audio`);

        // Send audio as response
        res.set({
            'Content-Type': 'audio/mpeg',
            'Content-Length': buffer.length
        });
        res.send(buffer);

    } catch(error) {
        console.error('❌ TTS Error:', error.message);
        console.error('Stack:', error.stack);
        res.status(500).json({
            error: 'Failed to generate speech',
            message: error.message
        });
    }
});

// API endpoint để lấy danh sách giọng
app.get('/api/voices', async (req, res) => {
    try {
        const { VoicesManager } = require('edge-tts-universal');
        const voicesManager = await VoicesManager.create();
        const voices = voicesManager.voices;
        res.json(voices);
    } catch(error) {
        console.error('Error getting voices:', error);
        res.status(500).json({ error: 'Failed to get voices' });
    }
});

app.listen(PORT, () => {
    console.log(`
╔════════════════════════════════════════╗
║   🤖 Robot AI Server Running!         ║
╚════════════════════════════════════════╝

Server: http://localhost:${PORT}
API:    http://localhost:${PORT}/api/tts

Open http://localhost:${PORT} in your browser
    `);
});
