import unittest

try:
    from bot import extract_youtube_id
except Exception:
    extract_youtube_id = None

@unittest.skipIf(extract_youtube_id is None, "bot dependencies are missing")
class TestExtractYouTubeId(unittest.TestCase):
    def test_video_and_playlist(self):
        url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PL12345"
        vid, playlist = extract_youtube_id(url)
        self.assertEqual(vid, "dQw4w9WgXcQ")
        self.assertEqual(playlist, "PL12345")

    def test_video_only(self):
        url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
        vid, playlist = extract_youtube_id(url)
        self.assertEqual(vid, "dQw4w9WgXcQ")
        self.assertIsNone(playlist)

if __name__ == '__main__':
    unittest.main()
