using System;
using Configs;
using Interfaces;
using JetBrains.Annotations;
using Scene;
using UnityEngine;

namespace Windows
{
    public class TimeWindowController : ISceneUpdatable, IDisposable
    {
        [CanBeNull] private readonly TimeDataManager _timeDataManager;
        [CanBeNull] private readonly TimeWindow _timeWindow;
        [CanBeNull] private readonly SceneListener _sceneListener;
        
        private DateTime _time;

        public TimeWindowController(ServiceLocator serviceLocator)
        {
            _sceneListener = serviceLocator.GetService<SceneListener>();
            _timeWindow = serviceLocator.GetService<TimeWindow>();
            _timeDataManager = serviceLocator.GetService<TimeDataManager>();

            _time = serviceLocator.GetService<TimeController>().LocalTime();
            UpdateTime(_time);

            if (_timeDataManager == null || _timeWindow == null)
                return;

            _timeDataManager.UpdateTimeEvent += UpdateTime;
            _timeWindow.ConfirmTextEvent += OnConfirmTextTextButtonClicked;
            _timeWindow.ConfirmArrowsEvent += OnConfirmArrowsButtonClicked;
            _timeWindow.EditEvent += OnEditButtonClicked;
        }

        public void Dispose()
        {
            if (_timeDataManager == null || _timeWindow == null)
                return;
            
            _timeDataManager.UpdateTimeEvent -= UpdateTime;
            _timeWindow.ConfirmTextEvent -= OnConfirmTextTextButtonClicked;
            _timeWindow.ConfirmArrowsEvent += OnConfirmArrowsButtonClicked;
            _timeWindow.EditEvent -= OnEditButtonClicked;
        }

        public void OnUpdate(float deltaTime)
        {
            _time = _time.AddSeconds(Time.deltaTime);
            UpdateTime(_time);
        }

        private void UpdateTime(DateTime dateTime)
        {
            if (_timeWindow == null)
                return;
            
            var daySeconds = (float)(dateTime - dateTime.Date).TotalSeconds;

            var minutesAngle = SettingsData.GetMinutesAngle(daySeconds);
            var hourAngle = SettingsData.GetHourAngle(daySeconds);
            var secondsAngle = SettingsData.GetSecondsAngle(daySeconds);

            _timeWindow.UpdateTime(dateTime, minutesAngle, hourAngle, secondsAngle);
        }

        private void OnEditButtonClicked()
        {
            if (_sceneListener == null)
                return;

            _sceneListener.Edit();
        }

        private void OnConfirmTextTextButtonClicked(string text)
        {
            if (_sceneListener == null || _timeWindow == null)
                return;

            if (!DateTimeOffset.TryParse(text, out var result))
            {
                _sceneListener.Confirm();
                return;
            }

            var alarmAngle = SettingsData.GetHourAngle((float)(result.DateTime - new DateTime().Date).TotalSeconds);
            _timeWindow.UpdateAlarmTime(result.DateTime, alarmAngle);

            _sceneListener.Confirm();
        }
        
        private void OnConfirmArrowsButtonClicked(Quaternion hoursRotation, Quaternion minutesRotation, Quaternion secondsRotation)
        {
            if (_sceneListener == null || _timeWindow == null)
                return;

            var alarmTime = new DateTime().AddSeconds(SettingsData.GetSeconds(hoursRotation.eulerAngles.z, minutesRotation.eulerAngles.z, secondsRotation.eulerAngles.z));
            _timeWindow.UpdateAlarmTime(alarmTime, hoursRotation.eulerAngles.z);

            _sceneListener.Confirm();
        }
    }
}
