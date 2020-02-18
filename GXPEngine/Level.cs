﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GXPEngine.Core;
using TiledMapParserExtended;

namespace GXPEngine
{
    public class Level : GameObject
    {
        private FollowCamera _cam;

        private MapGameObject _map;

        private Vector2 _spawnPoint;

        private List<Airplane> _airplanes;

        private StorkManager _storkManager;
        private Stork _stork;

        private DroneManager _dronesManager;

        private readonly Sprite[] _pizzasPool = new Sprite[20];
        private int _pizzaPoolIndex = 0;
        private HuntersManager _huntersManager;

        public Level(FollowCamera pCam, MapGameObject pMap)
        {
            _cam = pCam;
            _map = pMap;

            _airplanes = new List<Airplane>();

            var spawnPointObject = _map.ObjectGroup.Objects.FirstOrDefault(o => o.Name == "spawn point");
            _spawnPoint = new Vector2(spawnPointObject.X, spawnPointObject.Y);

            AddChild(_map);

            //Create delivery point
            var deliveryPointObject = _map.ObjectGroup.Objects.FirstOrDefault(o => o.Name == "delivery point");
            var deliveryPoint = new DeliveryPoint(deliveryPointObject.X, deliveryPointObject.Y,
                deliveryPointObject.Width, deliveryPointObject.Height);
            AddChild(deliveryPoint);

            var playerInput = new PlayerInput();
            AddChild(playerInput);

            _stork = new Stork
            {
                x = _spawnPoint.x,
                y = _spawnPoint.y,
                StorkInput = playerInput
            };
            
            AddChild(_stork);
            
            var hunterBulletManager = new HunterBulletManager(this);
            AddChild(hunterBulletManager);
            
            _huntersManager = new HuntersManager(this, hunterBulletManager);
            _huntersManager.SpawnHunters();
            _huntersManager.SetHuntersTarget(_stork);
            
            _dronesManager = new DroneManager(this);
            _dronesManager.SpawnDrones();

            _dronesManager.SetDronesTarget(_stork);

            for (int i = 0; i < _pizzasPool.Length; i++)
            {
                var pizza = new PizzaGameObject("data/pizza00.png");
                pizza.visible = false;
                this.AddChild(pizza);

                _pizzasPool[i] = pizza;
            }

            SpawnAirplanes();

            CoroutineManager.StartCoroutine(SetCamTargetRoutine(_stork), this);

            _storkManager = new StorkManager(_stork, this);

            AddChild(_cam);

            var hud = new HUD(_cam);
        }

        void Update()
        {
            if (Input.GetKeyDown(Key.D))
            {
                CoroutineManager.StartCoroutine(_storkManager.DropPizzaRoutine(_stork.Pos), this);
            }
        }

        public void SpawnAirplanes()
        {
            //Load Airplanes
            var airPlanesObjects = _map.ObjectGroup.Objects.Where(o => o.Name.StartsWith("airplane ")).ToArray();

            for (int i = 0; i < airPlanesObjects.Length; i++)
            {
                var airData = airPlanesObjects[i];

                float airSpeed = airData.GetFloatProperty("speed", 200);
                int lifeTime = (int) (airData.GetFloatProperty("life_time", 12) * 1000);

                var airplane = new Airplane(airData.X, airData.Y, airData.Width, airData.Height, this, airSpeed,
                    airData.rotation, lifeTime);

                _airplanes.Add(airplane);

                AddChild(airplane);
            }

            for (int i = _airplanes.Count() - 1; i > -1; i--)
            {
                if (_airplanes[i].Destroyed)
                {
                    _airplanes.RemoveAt(i);
                }
            }
        }

        public void RemoveAndDestroyAirplane(Airplane plane)
        {
            if (_airplanes.Contains(plane))
            {
                _airplanes.Remove(plane);
                plane.Destroy();
            }
        }

        private IEnumerator SetCamTargetRoutine(Stork stork)
        {
            yield return new WaitForMilliSeconds(500);
            _cam.Target = stork;
            _cam.TargetFrontDistance = 200;
        }

        public Sprite GetPizzaFromPool()
        {
            var pizza = _pizzasPool[_pizzaPoolIndex];
            _pizzaPoolIndex++;
            _pizzaPoolIndex %= _pizzasPool.Length;

            return pizza;
        }

        public string ChildrenToString()
        {
            return string.Join(Environment.NewLine, GetChildren().Select(c => c.name));
        }

        public MapGameObject Map => _map;

        public List<Airplane> AirPlanes => _airplanes;

        public int FirstAirplaneIndex => _airplanes.Count > 0 ? _airplanes[0].Index : GetChildren().Count;

        public IHasSpeed PlayerHasSpeed => _stork;
    }
}