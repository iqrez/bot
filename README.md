# bot

## Configuration

The bot reads required credentials from environment variables:

- `DISCORD_TOKEN` – token for your Discord bot
- `YOUTUBE_API_KEY` – API key used for YouTube searches

Set these variables before running the bot.

## Setup

1. Install **Python 3.10** or newer.
2. Install the required packages:

   ```bash
   python -m pip install --upgrade \
       "discord.py>=2.3.0" \
       "sqlalchemy>=2.0.0" \
       "psycopg2-binary>=2.9.0" \
       "PyNaCl>=1.5.0" \
       "ratelimit>=2.2.1" \
       "google-api-python-client>=2.100.0"
   ```
3. Run the bot:

   ```bash
   python bot.py
   ```

Optional variables:
- `DATABASE_URL` – connection string for the database (defaults to SQLite).
- `MASTER_PIN` – PIN for privileged commands.

