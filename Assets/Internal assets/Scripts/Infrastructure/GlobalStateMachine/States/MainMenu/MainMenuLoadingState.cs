﻿using Data.Addressable;
using Infrastructure.Factory.UIFactory;
using Infrastructure.GlobalStateMachine.StateMachine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.GlobalStateMachine.States.MainMenu
{
    public class MainMenuLoadingState : State<GameInstance>
    {
        public MainMenuLoadingState(GameInstance context, IUIFactory uiFactory) : base(context)
        {
            _uiFactory = uiFactory;
        }

        private readonly IUIFactory _uiFactory;

        public override async void Enter()
        {
            await _uiFactory.CreateLoadingScreen();

            var asyncOperationHandle = Addressables.LoadSceneAsync(AssetsAddressablesConstants.MAIN_MENU_SCENE_NAME);
            await asyncOperationHandle.Task;

            Context.StateMachine.SwitchState<MainMenuSetUpState>();
        }
    }
}