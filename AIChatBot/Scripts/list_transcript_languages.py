#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
List available transcript languages for a YouTube video
Usage: python list_transcript_languages.py <video_id>
Output: JSON array of available languages
"""

import sys
import json

sys.stdout.reconfigure(encoding='utf-8')
sys.stderr.reconfigure(encoding='utf-8')

from youtube_transcript_api import YouTubeTranscriptApi

def main():
    if len(sys.argv) < 2:
        print("ERROR: Missing video_id argument", file=sys.stderr)
        sys.exit(1)
    
    video_id = sys.argv[1]
    
    try:
        api = YouTubeTranscriptApi()
        transcript_list = api.list(video_id)
        
        languages = []
        for transcript in transcript_list:
            languages.append({
                "code": transcript.language_code,
                "name": transcript.language,
                "is_generated": transcript.is_generated
            })
        
        print(json.dumps(languages, ensure_ascii=False))
    except Exception as e:
        print(f'ERROR: {e}', file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
