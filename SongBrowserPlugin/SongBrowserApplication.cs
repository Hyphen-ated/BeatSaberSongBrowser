﻿using UnityEngine;
using System.Linq;
using System;
using SongLoaderPlugin.OverrideClasses;
using UnityEngine.SceneManagement;
using SongLoaderPlugin;
using UnityEngine.UI;
using SongBrowserPlugin.UI;
using System.Collections;
using System.Collections.Generic;

namespace SongBrowserPlugin
{
    public class SongBrowserApplication : MonoBehaviour
    {
        public static SongBrowserApplication Instance;

        private Logger _log = new Logger("SongBrowserApplication");

        // UIs
        private SongBrowserUI _songBrowserUI;
        private SongBrowserSettingsUI _settingUI;

        public Dictionary<String, Sprite> CachedIcons;

        /// <summary>
        /// 
        /// </summary>
        internal static void OnLoad()
        {            
            if (Instance != null)
            {
                return;
            }
            new GameObject("BeatSaber SongBrowser Mod").AddComponent<SongBrowserApplication>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            _log.Trace("Awake()");

            Instance = this;

            _songBrowserUI = gameObject.AddComponent<SongBrowserUI>();
            _settingUI = gameObject.AddComponent<SongBrowserSettingsUI>();
        }

        /// <summary>
        /// Acquire any UI elements from Beat saber that we need.  Wait for the song list to be loaded.
        /// </summary>
        public void Start()
        {
            _log.Trace("Start()");

            AcquireUIElements();

            StartCoroutine(WaitForSongListUI());
        }

        /// <summary>
        /// Wait for the song list to be visible to draw it.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForSongListUI()
        {
            _log.Trace("WaitForSongListUI()");

            yield return new WaitUntil(delegate () { return Resources.FindObjectsOfTypeAll<StandardLevelSelectionFlowCoordinator>().Any(); });

            _log.Debug("Found StandardLevelSelectionFlowCoordinator...");            

            _songBrowserUI.CreateUI();
            _settingUI.CreateUI();

            if (SongLoaderPlugin.SongLoader.AreSongsLoaded)
            {
                OnSongLoaderLoadedSongs(null, SongLoader.CustomLevels);
            }
            else
            {
                SongLoader.SongsLoadedEvent += OnSongLoaderLoadedSongs;
            }

            _songBrowserUI.RefreshSongList();            
        }

        /// <summary>
        /// Only gets called once during boot of BeatSaber.  
        /// </summary>
        /// <param name="loader"></param>
        /// <param name="levels"></param>
        private void OnSongLoaderLoadedSongs(SongLoader loader, List<CustomLevel> levels)
        {
            _log.Trace("OnSongLoaderLoadedSongs");
            try
            {
                _songBrowserUI.UpdateSongList();
            }
            catch (Exception e)
            {
                _log.Exception("Exception during OnSongLoaderLoadedSongs: ", e);
            }
        }

        /// <summary>
        /// Get a handle to the view controllers we are going to add elements to.
        /// </summary>
        public void AcquireUIElements()
        {
            _log.Trace("AcquireUIElements()");        
            try
            {
                CachedIcons = new Dictionary<String, Sprite>();
                foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
                {
                    if (CachedIcons.ContainsKey(sprite.name))
                    {
                        continue;
                    }
                    CachedIcons.Add(sprite.name, sprite);
                }

                // Append our own event to appropriate events so we can refresh the song list before the user sees it.
                MainFlowCoordinator mainFlow = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
                SoloModeSelectionViewController view = Resources.FindObjectsOfTypeAll<SoloModeSelectionViewController>().First();                
                view.didFinishEvent += HandleSoloModeSelectionViewControllerDidSelectMode;
            }
            catch (Exception e)
            {
                _log.Exception("Exception AcquireUIElements(): ", e);
            }
        }

        /// <summary>
        /// Perfect time to refresh the level list on first entry.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void HandleSoloModeSelectionViewControllerDidSelectMode(SoloModeSelectionViewController arg1, SoloModeSelectionViewController.SubMenuType arg2)
        {
            _log.Trace("HandleSoloModeSelectionViewControllerDidSelectMode()");
            this._songBrowserUI.RefreshSongList();
        }

        /// <summary>
        /// Helper for invoking buttons.
        /// </summary>
        /// <param name="buttonName"></param>
        public static void InvokeBeatSaberButton(String buttonName)
        {
            Button buttonInstance = Resources.FindObjectsOfTypeAll<Button>().First(x => (x.name == buttonName));
            buttonInstance.onClick.Invoke();
        }

        /// <summary>
        /// Map some key presses directly to UI interactions to make testing easier.
        /// </summary>
        private void LateUpdate()
        {
            // z,x,c,v can be used to get into a song, b will hit continue button after song ends
            if (Input.GetKeyDown(KeyCode.Z))
            {
                InvokeBeatSaberButton("SoloButton");
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                InvokeBeatSaberButton("StandardButton");
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                InvokeBeatSaberButton("ContinueButton");
            }
        }
    }
}
