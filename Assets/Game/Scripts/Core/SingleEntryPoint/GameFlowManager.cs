using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    /*  [SerializeField] private PlayerChunkStreamer playerChunkhunkStreamer = null;
     private PlayerSpawner playerSpawner = null;
  */[Header("Screens")]
    [SerializeField] private AuthUIController _authScreen;
    [SerializeField] private LevelLoaderScreen _levelLoaderScreen;
    [SerializeField] private LevelDetailUIScreen _levelDetailScreen;
    [SerializeField] private StoryScreen _storyScreen;

    [Header("Services")]
    [SerializeField] private UIService _uiService;
    [SerializeField] private BinarySaveService _saveService;
   /*  [SerializeField] private MonoPool<GameObject> _bulletPool; */
    [SerializeField] UpdateManager _updateManager;
    [SerializeField] FixedUpdateManager _fixedUpdateManager;
    [SerializeField] LateUpdateManager _lateUpdateManager;
    private List<IGameSystem> systems = new();
    private IAuthService _authService;
    private GameContext _ctx;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        //##### Register services
        _authService = new PlayFabAuthService();
        ServiceLocator.RegisterSingleton<IAuthService>(_authService);
        ServiceLocator.RegisterSingleton<IUIService>(_uiService);
        ServiceLocator.RegisterSingleton<ISaveService>(_saveService);

        var sceneDirector = new SceneDirector();
        ServiceLocator.RegisterSingleton<ISceneDirector>(sceneDirector);

        var titleDataSvc = new PlayFabTitleDataService();
        var httpSvc = new UnityHttpService();
        var contentSvc = new ContentService(titleDataSvc, httpSvc);
        ServiceLocator.RegisterSingleton<ITitleDataService>(titleDataSvc);
        ServiceLocator.RegisterSingleton<IHttpService>(httpSvc);
        ServiceLocator.RegisterSingleton<IContentService>(contentSvc);


        /*   ServiceLocator.RegisterSingleton<IObjectPool<GameObject>>(_bulletPool); */
        ServiceLocator.RegisterSingleton<IUpdateManager>(_updateManager);
        ServiceLocator.RegisterSingleton<IFixedUpdateManager>(_fixedUpdateManager); // FixedUpdate
        ServiceLocator.RegisterSingleton<ILateUpdateManager>(_lateUpdateManager); 
        // _uiService.Register("Settings", _settingsController);
    }
    
    public async void Start()
    {
        // Build the context AFTER registrations
        _ctx = GameContext.FromLocator();
        var ui = ServiceLocator.Resolve<IUIService>();
        var save = ServiceLocator.Resolve<ISaveService>();
        /*  var pool  = ServiceLocator.Resolve<IObjectPool<GameObject>>(); */

        //##### Register screens
        _uiService.Register(ScreenTypes.AuthScren, _authScreen);
        _uiService.Register(ScreenTypes.LevelLoaderScreen, _levelLoaderScreen);
        _uiService.Register(ScreenTypes.LevelDetailScreen, _levelDetailScreen);
        _uiService.Register(ScreenTypes.StoryScreen, _storyScreen);

        // Show main menu at game start
        await ui.ShowScreenAsync<object>(ScreenTypes.AuthScren);
        /*  playerSpawner = FindFirstObjectByType<PlayerSpawner>();
          playerSpawner.Initialize(transform.GetComponent<PlayerChunkStreamer>());
          GetComponent<HitDetector>().cam = playerSpawner.GetCamera;
          systems.Add(playerChunkhunkStreamer);
          systems.Add(playerSpawner);

          // at this point every peer finished loading GameScene*/
        RunGameFlowAsync().Forget();
    }

    private async UniTask RunGameFlowAsync()
    {
        // PHASE 1 – heavy boot
         foreach (var sys in systems)
            await sys.InitializeAsync();

        await UniTask.Delay(500);
     /*   //Spawn Player and it prefabs
        await playerSpawner.ActivatePlayer(); */

        /*   uiLoadingSystem.ShowLoadingScreen();

                  foreach (var system in systems)
                  {
                      await system.InitializeAsync();
                  }

                  await gridSystem.CreateGridAsync();
                  uiLoadingSystem.SetProgress(0.25f);

                  await playersManager.SpawnPlayers();
                  uiLoadingSystem.SetProgress(0.50f);

                  await dailyTasksController.UpdateDailyTasks();
                  uiLoadingSystem.SetProgress(0.75f);

                  await shopController.UpdateShop();
                  uiLoadingSystem.SetProgress(1f);

                  await UniTask.Delay(500);
                  uiLoadingSystem.HideLoadingScreen(); */
    }
}