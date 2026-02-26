#!/usr/bin/env python3
"""
Simple Edge TTS Server
Chạy: python tts_server.py
"""

from flask import Flask, request, send_file, jsonify
from flask_cors import CORS
import edge_tts
import asyncio
import tempfile
import os

app = Flask(__name__)
CORS(app)

@app.route('/api/tts', methods=['POST'])
def text_to_speech():
    try:
        data = request.json
        text = data.get('text', '')
        voice = data.get('voice', 'vi-VN-HoaiMyNeural')
        
        if not text:
            return jsonify({'error': 'Text is required'}), 400
        
        print(f"Generating TTS: '{text}' with voice: {voice}")
        
        # Create temp file
        temp_file = tempfile.NamedTemporaryFile(delete=False, suffix='.mp3')
        temp_path = temp_file.name
        temp_file.close()
        
        # Generate audio
        asyncio.run(generate_audio(text, voice, temp_path))
        
        print(f"✓ Generated audio: {os.path.getsize(temp_path)} bytes")
        
        # Send file and cleanup
        response = send_file(temp_path, mimetype='audio/mpeg')
        
        # Cleanup after sending
        @response.call_on_close
        def cleanup():
            try:
                os.unlink(temp_path)
            except:
                pass
        
        return response
        
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        return jsonify({'error': str(e)}), 500

async def generate_audio(text, voice, output_path):
    """Generate audio using edge-tts"""
    communicate = edge_tts.Communicate(text, voice)
    await communicate.save(output_path)

if __name__ == '__main__':
    print("""
╔════════════════════════════════════════╗
║   🤖 Edge TTS Server (Python)         ║
╚════════════════════════════════════════╝

Server: http://localhost:5000
API:    http://localhost:5000/api/tts

Cài đặt:
  pip install flask flask-cors edge-tts

Chạy:
  python tts_server.py
    """)
    app.run(host='0.0.0.0', port=5000, debug=False)
