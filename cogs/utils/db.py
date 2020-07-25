import asyncpg

class AcquireConnection:
    def __init__(self, pool):
        self.pool = pool
        self.con = None

    async def __aenter__(self):
        self.con = await self.pool.acquire()
        return self.con

    async def __aexit__(self, *args):
        await self.pool.release(self.con)

class Column:
    def __init__(self, *, is_index=False, is_pkey=False, is_nullable=False,
            is_unique=False, default=None, name=None):
        
        self.is_index = is_index
        self.is_pkey = is_pkey
        self.is_nullable = is_nullable
        self.is_unique = is_unique
        self.default = default
        self.name = name
        

class Table:
    def __init__(self, name, cols):
        self.name = name
        self.cols = cols

    async def insert(self, con, **kwargs):
        checked_kwargs = self.__check_kwargs__(**kwargs)
        
        sql = 'INSERT INTO {0} ({1}) VALUES ({2});'.format(
            self.name,
            ', '.join(checked_kwargs),
            ', '.join('$' + str(i) for i in range(1, len(checked_kwargs) + 1))
        )

        await con.execute(sql, *checked_kwargs.values())

    async def update(self, con, **kwargs):
        checked_kwargs = self.__check_kwargs__(**kwargs)

        print('update')

    async def delete(self, con, **kwargs):
        checked_kwargs = self.__check_kwargs__(**kwargs)

        print('update')

    async def get_rows(self, con, **kwargs):
        checked_kwargs = self.__check_kwargs__(**kwargs)

        sql = 'SELECT * FROM {0} WHERE {1};'.format(
            self.name,
            ' AND '.join(f'{_} = ${i}' for i, _ in enumerate(checked_kwargs, 1))
        )

        return await con.fetch(sql, *checked_kwargs.values())

    async def row_exists(self, con, **kwarg):
        print('check if exists')

    def __check_kwargs__(self, **kwargs):
        result = {}

        for col in self.cols:
            try:
                value = kwargs[col.name]
            except KeyError:
                continue

            if value is None and not col.is_nullable:
                print('some error')
            

            result[col.name] = value

        return result

class TamamoDatabase:
    def __init__(self):
        self.tables = {}

    async def create_pool(self, uri, **kwargs):
        self.pool = await asyncpg.create_pool(uri, **kwargs)

    def add_table(self, table: Table):
        self.tables[table.name] = table

    def acquire_connection(self):
        return AcquireConnection(self.pool)

    async def insert(self, table, **kwargs):
        async with self.acquire_connection() as con:
            await self.tables[table].insert(con, **kwargs)

    async def delete(self, table, **kwargs):
        async with self.acquire_connection() as con:
            await self.tables[table].delete(con, **kwargs)

    async def update(self, table, **kwargs):
        async with self.acquire_connection() as con:
            await self.tables[table].update(con, **kwargs)

    async def get_rows(self, table, **kwargs):
        async with self.acquire_connection() as con:
            await self.tables[table].get_rows(con, **kwargs)