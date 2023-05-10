﻿using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace GeneratorDungeons
{
    public static class TileDungeon
    {
        public static MapDungeon GenerationDungeon(int seed,
            int minRoomSize,
            int maxRoomSize,
            int minRoomCount,
            int maxRoomCount,
            int width,
            int height)
        {
            var map = Generation(
                seed,
                minRoomSize,
                maxRoomSize,
                minRoomCount,
                maxRoomCount,
                width,
                height);

            map = OverGenerationEnemies(map);

            return map;
        }

        public static MapDungeon GenerationDungeon(MapDungeon mapDungeon)
        {
            var map = Generation(
                mapDungeon.Seed,
                mapDungeon.MinRoomSize,
                mapDungeon.MaxRoomSize,
                mapDungeon.MinRoomSize,
                mapDungeon.MaxRoomSize,
                mapDungeon.Width,
                mapDungeon.Height);

            map = OverGenerationEnemies(map);

            return map;
        }

        public static MapDungeon OverGenerationEnemies(MapDungeon mapDungeon)
        {
            var rooms = mapDungeon.Rooms;
            var startRoom = mapDungeon.StartRoom;
            var tilesMap = mapDungeon.TilesMap;
            var height = mapDungeon.Height;
            var width = mapDungeon.Width;

            var enemyPosition = GetEnemiesPosition(rooms, startRoom, tilesMap, height, width);

            return new MapDungeon(enemyPosition, mapDungeon);
        }


        #region Generation Map

        private static MapDungeon Generation(int seed,
            int minRoomSize,
            int maxRoomSize,
            int minRoomCount,
            int maxRoomCount,
            int width,
            int height)
        {
            var tilesMap = FillInWalls(height, width);

            AddRooms(
                ref tilesMap,
                out var rooms,
                seed,
                minRoomSize,
                maxRoomSize,
                minRoomCount,
                maxRoomCount,
                width,
                height);

            AddCorridors(ref tilesMap, ref rooms);
            AddPlayer(ref rooms, ref tilesMap, out var startRoom);
            ClearWall(height, width, ref tilesMap);

            return new MapDungeon(
                tilesMap,
                rooms,
                startRoom,
                seed,
                minRoomSize,
                maxRoomSize,
                minRoomCount,
                maxRoomCount,
                width,
                height);
        }

        private static Tiles[,] FillInWalls(int height, int width)
        {
            var tilesMap = new Tiles[height, width];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                tilesMap[y, x] = Tiles.EMPTY;

            return tilesMap;
        }

        private static void AddRooms(ref Tiles[,] tilesMap, out List<Room> rooms, int seed, int minRoomSize,
            int maxRoomSize, int minRoomCount,
            int maxRoomCount, int width, int height)
        {
            rooms = new List<Room>();
            var roomCount = RandomRange(ref seed, minRoomCount, maxRoomCount);
            Console.WriteLine(roomCount);
            for (var i = 0; i != roomCount; ++i)
            {
                var room = new Room
                {
                    x = RandomRange(ref seed, maxRoomSize, width - maxRoomSize),
                    y = RandomRange(ref seed, maxRoomSize, height - maxRoomSize),
                    width = RandomRange(ref seed, minRoomSize, maxRoomSize),
                    height = RandomRange(ref seed, minRoomSize, maxRoomSize)
                };

                Console.WriteLine(room.x + " " + room.y);

                CarveFloor(
                    ref tilesMap,
                    room.y - room.height / 2,
                    room.x - room.width / 2,
                    room.height,
                    room.width);

                rooms.Add(room);
            }
        }


        private static void AddCorridors(ref Tiles[,] tilesMap, ref List<Room> rooms)
        {
            for (var i = 0; i != rooms.Count - 1; ++i)
            {
                CarveFloor(
                    ref tilesMap,
                    rooms[i].y,
                    Math.Min(rooms[i].x, rooms[i + 1].x),
                    2,
                    1 + Math.Abs(rooms[i + 1].x - rooms[i].x));

                CarveFloor(
                    ref tilesMap,
                    Math.Min(rooms[i].y, rooms[i + 1].y),
                    rooms[i + 1].x,
                    1 + Math.Abs(rooms[i + 1].y - rooms[i].y),
                    2);
            }
        }

        //TODO: переделать, сделать поиск наименьшей комнаты, не пересекающейся с другими
        private static void AddPlayer(ref List<Room> rooms, ref Tiles[,] tilesMap, out Room startRoom)
        {
            var distHi = 0;
            var startIdx = -1;
            for (var i = 0; i != rooms.Count; ++i)
            for (var j = 0; j != rooms.Count; ++j)
            {
                var dist = Math.Abs(rooms[i].x - rooms[j].x) + Math.Abs(rooms[i].y - rooms[j].y);
                if (dist <= distHi) continue;
                distHi = dist;
                startIdx = i;
            }

            startRoom = rooms[startIdx];
            tilesMap[startRoom.y, startRoom.x] = Tiles.PLAYER;
            rooms.Remove(startRoom);
        }

        private static void ClearWall(int height, int width, ref Tiles[,] tilesMap)
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                if (tilesMap[y, x] == Tiles.FLOOR)
                    for (var dy = -1; dy <= 1; ++dy)
                    for (var dx = -1; dx <= 1; ++dx)
                        if (tilesMap[y + dy, x + dx] == Tiles.EMPTY)
                            tilesMap[y + dy, x + dx] = Tiles.WALL;
        }

        #endregion

        #region Generation Enemy

        private static List<(int, int)> GetEnemiesPosition(
            List<Room> rooms, Room startRoom, Tiles[,] tilesMap, int height, int width)
        {
            var enemyPosition = new List<(int, int)>();

            for (var y = startRoom.y - startRoom.height / 2 - 5; y < startRoom.y - startRoom.height / 2 + 7; y++)
            {
                if (y < 0 || y > height - 1) continue;
                for (var x = startRoom.x - startRoom.width / 2 - 5; x < startRoom.x - startRoom.width / 2 + 7; x++)
                {
                    if (x < 0 || x > width - 1) continue;
                    if (tilesMap[y, x] == Tiles.FLOOR)
                        tilesMap[y, x] = Tiles.PLAYER;
                }
            }

            foreach (var room in rooms)
                for (var i = 0; i < (room.height * room.width / 20 == 0 ? 1 : room.height * room.width / 20); i++)
                for (var flag = 0; flag < 25; flag++)
                {
                    var position = (
                        Random.Range(room.x - room.width / 2, room.x + room.width / 2),
                        Random.Range(room.y - room.height / 2, room.y + room.height / 2));

                    if (tilesMap[position.Item1, position.Item2] != Tiles.FLOOR) continue;

                    enemyPosition.Add(position);
                    break;
                }

            return enemyPosition;
        }

        #endregion

        #region Tools

        private static int IntHash(ref int x)
        {
            if (x == 0)
            {
                x = 1;
            }

            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = ((x >> 16) ^ x) * 0x45d9f3b;
            x = (x >> 16) ^ x;
            return x;
        }

        private static int RandomRange(ref int seed, int min, int max)
        {
            var range = IntHash(ref seed) % ((max + 1) - min);
            return min + range;
        }

        private static void CarveFloor(ref Tiles[,] tilesMap, int y, int x, int height, int width)
        {
            if (width < 1 || height < 1)
            {
                return;
            }

            for (var dy = y; dy < y + height; dy++)
            for (var dx = x; dx < x + width; dx++)
                tilesMap[dy, dx] = Tiles.FLOOR;
        }

        #endregion
    }
}