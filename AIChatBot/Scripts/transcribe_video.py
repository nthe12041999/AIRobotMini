#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Transcribe video using OpenAI Whisper
Usage: python transcribe_video.py <video_path> [language]
"""

import sys
import os
import subprocess

sys.stdout.reconfigure(encoding='utf-8')
sys.stderr.reconfigure(encoding='utf-8')

if len(sys.argv) < 2:
    print('ERROR: Missing video path', file=sys.stderr)
    sys.exit(1)

video_path = sys.argv[1]
language = sys.argv[2] if len(sys.argv) > 2 else 'vi'

# Tìm ffmpeg trong folder Tools/ffmpeg relative to script
script_dir = os.path.dirname(os.path.abspath(__file__))
ffmpeg_path = os.path.abspath(os.path.join(script_dir, '..', 'Tools', 'ffmpeg'))

if os.path.exists(ffmpeg_path):
    os.environ['PATH'] = ffmpeg_path + os.pathsep + os.environ.get('PATH', '')

# Verify ffmpeg is accessible
try:
    subprocess.run(['ffmpeg', '-version'], capture_output=True, check=True)
except Exception as e:
    print(f'ERROR: ffmpeg not found - {e}', file=sys.stderr)
    print(f'FFmpeg path: {ffmpeg_path}', file=sys.stderr)
    print(f'PATH: {os.environ.get("PATH", "")}', file=sys.stderr)
    sys.exit(1)

import whisper

try:
    model = whisper.load_model('base')
    result = model.transcribe(video_path, language=language)
    print(result['text'])
except Exception as e:
    print(f'ERROR: {e}', file=sys.stderr)
    sys.exit(1)
