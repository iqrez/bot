# bot

## Installation

Install the Python dependencies:

```bash
pip install -r requirements.txt
```

Copy the example configuration and edit it with your credentials:

```bash
cp config.example.env .env
# edit .env to add your tokens
```

## Configuration

The bot reads credentials from environment variables:

- `DISCORD_TOKEN` – token for your Discord bot
- `YOUTUBE_API_KEY` – API key used for YouTube searches

Ensure these variables are set before running the bot (for example by using a `.env` file).

## Usage

Run the bot directly:

```bash
./bot.py
```

## Commands

The bot provides several slash commands:

- `/ping` – check if the bot is responsive
- `/play <song>` – play a song or add it to the queue
- `/skip` – skip the current song
- `/pause` – pause playback
- `/resume` – resume playback
- `/stop` – stop playing and clear the queue
- `/volume <0-100>` – set playback volume
- `/queue` – show queued songs
- `/nowplaying` – show the current song
- `/disconnect` – disconnect from the voice channel
- `/clear` – clear the queue
- `/shuffle` – shuffle the queue
- `/loop <off|track|queue>` – set loop mode
- `/remove <position>` – remove a song from the queue
- `/search <song>` – search without playing
- `/stats` – show bot statistics
- `/cache` – manage the song cache (admin only)
- `/menu` – show the music control panel

