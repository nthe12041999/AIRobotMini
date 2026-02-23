#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
Get YouTube transcript using youtube-transcript-api
Usage: python get_transcript.py <video_id> [language_code]
"""

import sys

sys.stdout.reconfigure(encoding='utf-8')
sys.stderr.reconfigure(encoding='utf-8')

from youtube_transcript_api import YouTubeTranscriptApi

def main():
    if len(sys.argv) < 2:
        print("ERROR: Missing video_id argument", file=sys.stderr)
        sys.exit(1)
    
    video_id = sys.argv[1]
    language_code = sys.argv[2] if len(sys.argv) > 2 else None
    
    try:
        api = YouTubeTranscriptApi()
        
        if language_code:
            transcript = api.fetch(video_id, languages=[language_code])
        else:
            # Try to get any available transcript
            transcript_list = api.list(video_id)
            first_transcript = next(iter(transcript_list))
            transcript = first_transcript.fetch()
        
        text = ' '.join([s.text for s in transcript.snippets])
        print(text)
    except Exception as e:
        print(f'ERROR: {e}', file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
