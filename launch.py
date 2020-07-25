import asyncio
import config
from cogs.utils.db_constants import GUILD_CONFIG_COLS
from cogs.utils.db import TamamoDatabase, Table
from bot import SnekTamamo

def main():
    loop = asyncio.get_event_loop()
    db = TamamoDatabase()

    try:
        loop.run_until_complete(db.create_pool(config.pg_uri))
    except Exception as e:
        print(f'Failed to initialize PostgreSQL connection pool.\n{e}')
        return

    db.add_table(Table('guild_configs', GUILD_CONFIG_COLS))

    bot = SnekTamamo(db)
    bot.run()

if __name__ == "__main__":
    main()