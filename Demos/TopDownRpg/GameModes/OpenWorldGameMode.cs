﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demos.Common;
using Demos.TopDownRpg.Entities;
using Demos.TopDownRpg.Factory;
using GameFrame;
using GameFrame.Camera;
using GameFrame.CollisionSystems;
using GameFrame.CollisionSystems.SpatialHash;
using GameFrame.CollisionSystems.Tiled;
using GameFrame.Content;
using GameFrame.Controllers;
using GameFrame.Ink;
using GameFrame.Movers;
using GameFrame.PathFinding;
using GameFrame.PathFinding.PossibleMovements;
using GameFrame.Paths;
using GameFrame.Renderers;
using GameFrame.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace Demos.TopDownRpg.GameModes
{
    public delegate void AddEntity(Entity entity);

    public delegate void RemoveEntity(Entity entity);

    public delegate void Say(GameFrameStory story);

    public class OpenWorldGameMode : AbstractRpgGameMode
    {
        public TiledMap Map;
        private IMapRenderer _mapRenderer;
        private Vector2 _tileSize;
        private readonly ContentManager _content;
        public AbstractCollisionSystem CollisionSystem;
        public AbstractPathRenderer PathRenderer;
        private readonly IPossibleMovements _possibleMovements;
        public Dictionary<Entity, AbstractEntityRenderer> EntityRenderersDict;
        public List<IRenderable> RenderList;
        private readonly ExpiringSpatialHashCollisionSystem<Entity> _expiringSpatialHash;
        private readonly SpatialHashMoverManager<Entity> _spatialHashMover;
        private readonly EntityManager _entityManager;
        private readonly MoverManager _moverManager;
        public AbstractCameraTracker CameraTracker;

        public OpenWorldGameMode(ViewportAdapter viewPort, IPossibleMovements possibleMovements, string worldName, EntityManager entityManager, StoryEngine storyEngine, EventHandler clickEvent): base(clickEvent)
        {
            _entityManager = entityManager;
            EntityRenderersDict = new Dictionary<Entity, AbstractEntityRenderer>();
            _possibleMovements = possibleMovements;
            _content = ContentManagerFactory.RequestContentManager();
            RenderList = new List<IRenderable>();
            Map = _content.Load<TiledMap>($"TopDownRpg/{worldName}");
            var graphics = StaticServiceLocator.GetService<GraphicsDevice>();
            _mapRenderer = new FullMapRenderer(graphics);
            _mapRenderer.SwapMap(Map);
            _tileSize = new Vector2(Map.TileWidth, Map.TileHeight);
            _moverManager = new MoverManager();
            var collisionSystem = new CompositeAbstractCollisionSystem(_possibleMovements);
            _expiringSpatialHash = new ExpiringSpatialHashCollisionSystem<Entity>(_possibleMovements);
            _spatialHashMover = new SpatialHashMoverManager<Entity>(collisionSystem, _expiringSpatialHash);
            AddPlayer();
            var entityController = EntityControllerFactory.AddEntityController(PlayerEntity.Instance, _possibleMovements, _moverManager);
            var texture = _content.Load<Texture2D>("TopDownRpg/Path");
            var endTexture = _content.Load<Texture2D>("TopDownRpg/BluePathEnd");

            collisionSystem.AddCollisionSystem(new TiledCollisionSystem(_possibleMovements, Map, "Collision-Layer"));
            collisionSystem.AddCollisionSystem(_expiringSpatialHash);
            CollisionSystem = collisionSystem;
            AddClickController(PlayerEntity.Instance);
            PathRenderer = new PathRenderer(_moverManager, PlayerEntity.Instance, texture, endTexture, _tileSize.ToPoint(), Map.Width, Map.Height);
            UpdateList.Add(_expiringSpatialHash);
            UpdateList.Add(entityController);
            UpdateList.Add(_spatialHashMover);
            UpdateList.Add(_moverManager);
            CameraTracker = CameraTrackerFactory.CreateTracker(viewPort, EntityRenderersDict[PlayerEntity.Instance], Map);
            UpdateList.Add(CameraTracker);
            LoadEntities();
            var dialogFont = _content.Load<SpriteFont>("dialog");
            var settings = StaticServiceLocator.GetService<IControllerSettings>();
            DialogBox = new EntityStoryBoxDialog(ScreenSize.Size, dialogFont, settings.GamePadEnabled);
            GuiManager.AddGuiLayer(DialogBox);
            storyEngine.LoadWorld(AddEntity, RemoveEntity, CollisionSystem.CheckMovementCollision, worldName);
            InteractEvent += (sender, args) =>
            {
                var facingDirection = PlayerEntity.Instance.FacingDirection;
                var interactTarget = (PlayerEntity.Instance.Position + facingDirection).ToPoint();
                Interact(interactTarget);
            };
            AddInteractionController();
            CameraController.AddCameraZoomController(CameraTracker, ClickController);
            CameraController.AddCameraMovementController(CameraTracker, ClickController);
        }

        public void LoadEntities()
        {
            var entityObjects = Map.GetObjectGroup("Entity-Layer");
            if (entityObjects != null)
            {
                foreach (var entityObject in entityObjects.Objects)
                {
                    var position = entityObject.Position / _tileSize;
                    var entity = _entityManager.Import(entityObject.Name);
                    entity.Position = position;
                    AddEntity(entity);
                }
            }
        }

        public void RemoveEntity(Entity entity)
        {
            var position = entity.Position.ToPoint();
            _expiringSpatialHash.RemoveNode(position);
            var entityRenderer = EntityRenderersDict[entity];
            RenderList.Remove(entityRenderer);
            EntityRenderersDict.Remove(entity);
            _spatialHashMover.Remove(entity);
        }

        public void AddPlayer()
        {
            var entityRenderer = new PlayerEntityRenderer(_content, _expiringSpatialHash, PlayerEntity.Instance, _tileSize.ToPoint());
            _expiringSpatialHash.AddNode(PlayerEntity.Instance.Position.ToPoint(), PlayerEntity.Instance);
            RenderList.Add(entityRenderer);
            EntityRenderersDict[PlayerEntity.Instance] = entityRenderer;
            _spatialHashMover.Add(PlayerEntity.Instance);
        }

        public void AddEntity(Entity entity)
        {
            var entityRenderer = new EntityRenderer(_content, _expiringSpatialHash, entity, _tileSize.ToPoint());
            _expiringSpatialHash.AddNode(entity.Position.ToPoint(), entity);
            RenderList.Add(entityRenderer);
            EntityRenderersDict[entity] = entityRenderer;
            _spatialHashMover.Add(entity);
        }

        public void AddClickController(Entity entity)
        {
            ClickEvent = point =>
            {
                var endPoint = CameraTracker.ScreenToWorld(point.X, point.Y).ToPoint();
                BeginMoveTo(entity, endPoint / _tileSize.ToPoint());
            };
        }

        public void BeginMoveTo(Entity entity, Point endPoint)
        {
            var moveTo = endPoint;
            var collision = _expiringSpatialHash.CheckCollision(moveTo) || WaterCollision(endPoint);
            var valid = true;
            if (collision)
            {
                var heuristic = _possibleMovements.Heuristic;
                var startPosition = entity.Position.ToPoint();
                if (Math.Abs(heuristic.GetTraversalCost(startPosition, endPoint) - 1.0f) < 0.1f)
                {
                    Interact(endPoint);
                    valid = false;
                }
                else
                {
                    var alternatuvePositions = FourWayPossibleMovement.FourWayAdjacentLocations(moveTo);
                    var minCost = double.MaxValue;
                    foreach (var position in alternatuvePositions)
                    {
                        if (!CollisionSystem.CheckCollision(position))
                        {
                            var cost = heuristic.GetTraversalCost(startPosition, position);
                            if (cost < minCost)
                            {
                                minCost = cost;
                                moveTo = position;
                            }
                        }
                    }
                    if (moveTo == endPoint)
                    {
                        valid = false;
                    }
                }
            }
            else
            {
                valid = !CollisionSystem.CheckCollision(moveTo);
            }
            if (valid)
            {
                MoveEntityTo(moveTo, entity, _tileSize.ToPoint(), _moverManager, collision, endPoint);
            }
        }

        public void Interact(Point interactTarget)
        {
            var validInteraction = FourWayPossibleMovement.FourWayAdjacentLocations(PlayerEntity.Instance.Position.ToPoint()).Contains(interactTarget);
            if (validInteraction)
            {
                PlayerEntity.Instance.FacingDirection = interactTarget.ToVector2() - PlayerEntity.Instance.Position;
                var interactWith = _expiringSpatialHash.ValueAt(interactTarget);
                if (interactWith != null)
                {
                    var story = interactWith.Interact();
                    story.Continue();
                    var entityDialogBox = DialogBox as EntityStoryBoxDialog;
                    entityDialogBox?.StartStory(story, interactWith);
                }
                else if (Flags.AcquireRod)
                {
                    if (WaterCollision(interactTarget))
                    {
                        var random = new Random();
                        var fishComplete = random.Next(3) == 0;
                        var scriptName = fishComplete ? "fish_success.ink" : "fish_fail.ink";
                        var fishScript = StoryImporter.ReadStory(scriptName);
                        var story = new GameFrameStory(fishScript);
                        if (fishComplete)
                        {
                            Flags.FishCount++;
                            story.ChoosePathString("dialog");
                            story.SetVariableState("fish_count", Flags.FishCount);
                        }
                        story.Continue();
                        DialogBox.StartStory(story);
                    }
                }
            }
        }

        public bool WaterCollision(Point p)
        {
            const string layerName = "Water-Layer";
            var waterLayer = Map.GetLayer<TiledTileLayer>(layerName);
            var collision = false;
            if (waterLayer != null)
            {
                var waterCollision = new TiledCollisionSystem(_possibleMovements, Map, layerName);
                collision = waterCollision.CheckCollision(p);
            }
            return collision;
        }

        public async void MoveEntityTo(Point endPoint, Entity entity, Point tileSize, MoverManager moverManager, bool interact = false, Point? interactWith = null)
        {
            var searchParams = new SearchParameters(entity.Position.ToPoint(), endPoint, CollisionSystem, new Size(Map.Width, Map.Height));
            await Task.Run(() =>
            {
                var path = new AStarPathFinder(searchParams, _possibleMovements).FindPath();
                var pathMover = new PathMover(entity, new FinitePath(path), new ExpiringSpatialHashMovementComplete<Entity>(_expiringSpatialHash, PlayerEntity.Instance));
                pathMover.OnCancelEvent += (sender, args) => entity.MovingDirection = new Vector2();
                if (interact && interactWith != null)
                {
                    pathMover.OnCompleteEvent += (sender, args) => Interact(interactWith.Value);
                }
                moverManager.AddMover(pathMover);
            });
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var transformMatrix = CameraTracker.TransformationMatrix;
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix,
                              sortMode: SpriteSortMode.BackToFront, depthStencilState: DepthStencilState.Default);
            _mapRenderer.Draw(transformMatrix);
            foreach (var toRender in RenderList)
            {
                if (CameraTracker.Contains(toRender.Area))
                {
                    toRender.Draw(spriteBatch);
                }
            }
            PathRenderer.Draw(spriteBatch);
            spriteBatch.End();
            GuiManager.Draw(spriteBatch);
        }

        public override void Dispose()
        {
            _content.Unload();
            _content.Dispose();
        }
    }
}
