using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Urd.Error;
using Urd.Services.Navigation;

namespace Urd.Services
{
    public class NavigationService : BaseService, INavigationService
    {
        private List<INavigable> _navigableOpened = new List<INavigable>();
        private List<INavigable> _navigableHistory = new List<INavigable>();

        private List<INavigationManager> _navigationManagers = new List<INavigationManager>();
        
        public override void Init()
        {
            _navigationManagers.Add(new NavigationPopupManager());
        }

        public void Open(INavigable navigable, Action<bool> onOpenNavigableCallback)
        {
            var navigationManager = GetNavigationManager(navigable);
            if (navigationManager == null)
            {
                var error = new ErrorModel(
                    $"[NavigationService] There no manager for the navigable {navigable}",
                    ErrorCode.Error_404_Not_Found,
                    UnityWebRequest.Result.DataProcessingError);
                Debug.LogWarning(error.ToString());

                onOpenNavigableCallback?.Invoke(false);
                return;
            }

            if (!navigationManager.CanOpen(navigable))
            {
                onOpenNavigableCallback?.Invoke(false);
                return;
            }

            navigationManager.Open(navigable,
                (success) => OnOpenNavigable(success, navigable, onOpenNavigableCallback));
        }


        private void OnOpenNavigable(bool success, INavigable navigable, Action<bool> onOpenNavigableCallback)
        {
            if (success)
            {
                _navigableOpened.Add(navigable);
                AddToHistory(navigable);
            }

            onOpenNavigableCallback?.Invoke(success);
        }
        
        private INavigationManager GetNavigationManager(INavigable navigable)
        {
            for (int i = 0; i < _navigationManagers.Count; i++)
            {
                if (_navigationManagers[i].CanHandle(navigable))
                {
                    return _navigationManagers[i];
                }
            }

            return null;
        }

        public bool IsOpen(INavigable navigable)
        {
            return _navigableOpened.Exists(navigableOpened => navigableOpened.Id == navigable.Id);
        }

        private void AddToHistory(INavigable navigable)
        {
            _navigableHistory.Add(navigable);
        }

        public void Close(INavigable navigable, Action<bool> onCloseNavigableCallback)
        {
            var navigationManager = GetNavigationManager(navigable);
            if (navigationManager == null)
            {
                var error = new ErrorModel(
                    $"[NavigationService] There no manager for the navigable {navigable}",
                    ErrorCode.Error_404_Not_Found,
                    UnityWebRequest.Result.DataProcessingError);
                Debug.LogWarning(error.ToString());
                onCloseNavigableCallback?.Invoke(false);
                
                return;
            }
            
            navigationManager.Close(navigable, (success) => OnCloseNavigable(success, navigable, onCloseNavigableCallback));
        }

        private void OnCloseNavigable(bool success, INavigable navigable, Action<bool> onCloseNavigable)
        {
            if (success)
            {
                _navigableOpened.Remove(navigable);
                navigable.ChangeStatus(NavigableStatus.Closed);
            }

            onCloseNavigable?.Invoke(success);
        }
    }
}