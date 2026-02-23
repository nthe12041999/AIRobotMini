#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Edge TTS converter - standalone script
Usage: python edge_tts_convert.py <text_file> <voice> <rate> <pitch> <volume> <output_path>
"""

import sys
import asyncio

def main():
    if len(sys.argv) < 7:
        print("Usage: edge_tts_convert.py <text_file> <voice> <rate> <pitch> <volume> <output_path>")
        sys.exit(1)
    
    text_file = sys.argv[1]
    voice = sys.argv[2]
    rate = sys.argv[3]
    pitch = sys.argv[4]
    volume = sys.argv[5]
    output_path = sys.argv[6]
    
    try:
        import edge_tts
    except ImportError:
        print("ERROR: edge-tts not installed. Run: pip install edge-tts")
        sys.exit(1)
    
    # Read text from file
    with open(text_file, 'r', encoding='utf-8') as f:
        text = f.read()
    
    async def convert():
        communicate = edge_tts.Communicate(
            text=text,
            voice=voice,
            rate=rate,
            pitch=pitch,
            volume=volume
        )
        await communicate.save(output_path)
        print("SUCCESS")
    
    asyncio.run(convert())

if __name__ == "__main__":
    main()
