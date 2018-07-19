using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace IntegraLib
{
    public static class MSSQLService
    {
        private static double _actionTimeout = 10.0d;
        private static ServiceController _service;

        public static ServiceController Controller { get { return _service; } }

        private static string _errorMessage;

        public static string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public static ServiceControllerStatus Status
        {
            get
            {
                ServiceControllerStatus retVal = ServiceControllerStatus.Stopped;
                if (_service == null)
                {
                    _errorMessage = "MSSQLServer is not defined in private field yet. Call MSSQLService.FindService() before.";
                }
                else
                {
                    retVal = _service.Status;
                    _errorMessage = null;
                }
                return retVal;
            }
        }

        public static bool IsExists
        {
            get { return (_service != null); }
        }



        static MSSQLService()
        {
            _findFirstMSSQLService();
        }

        private static bool _findFirstMSSQLService()
        {
            ServiceController[] scArr = WinServicesHelper.AllServices;
            foreach (ServiceController item in scArr)
            {
                if (item.ServiceName.StartsWith("MSSQL"))
                {
                    _service = item;
                    return true; ;
                }
            }
            return false;
        }

        public static bool FindService(string serviceName = null)
        {
            if (serviceName == null)
            {
                return _findFirstMSSQLService();
            }

            ServiceController[] scArr = WinServicesHelper.AllServices;
            foreach (ServiceController item in scArr)
            {
                if (item.ServiceName == serviceName)
                {
                    _service = item;
                    return true; ;
                }
            }
            return false;
        }

        #region all services public methods
        public static void Refresh()
        {
            if (_service != null) _service.Refresh();
        }
        public static bool Stop()
        {
            bool retVal = false;

            if (_service != null)
            {
                if (_service.CanStop)
                {
                    try
                    {
                        _service.Stop();
                        _service.Refresh();
                        DateTime dtTmr = DateTime.Now;
                        // цикл ожидания закрытия службы
                        while (_service.Status == ServiceControllerStatus.StopPending)
                        {
                            System.Threading.Thread.Sleep(500);  // задержка в 500 мсек
                            _service.Refresh();
                            if ((DateTime.Now - dtTmr).TotalSeconds >= _actionTimeout)
                                throw new Exception(string.Format("Истек период в {0} секунд для остановки службы MS SQL Server ({1})", _actionTimeout, _service.ServiceName));
                        }
                        retVal = (_service.Status == ServiceControllerStatus.Stopped);
                        _errorMessage = null;
                    }
                    catch (Exception ex)
                    {
                        _errorMessage = ErrorHelper.GetShortErrMessage(ex);
                    }
                }
            }

            return retVal;
        }

        public static bool Pause()
        {
            bool retVal = false;

            if (_service != null)
            {
                if (_service.CanPauseAndContinue)
                {
                    try
                    {
                        _service.Pause();
                        _service.Refresh();
                        DateTime dtTmr = DateTime.Now;
                        // цикл ожидания приостановки службы
                        while (_service.Status == ServiceControllerStatus.PausePending)
                        {
                            System.Threading.Thread.Sleep(500);  // задержка в 500 мсек
                            _service.Refresh();
                            if ((DateTime.Now - dtTmr).TotalSeconds >= _actionTimeout)
                                throw new Exception(string.Format("Истек период в {0} секунд для приостановки службы MS SQL Server ({1})", _actionTimeout, _service.ServiceName));
                        }
                        retVal = (_service.Status == ServiceControllerStatus.Paused);
                        _errorMessage = null;
                    }
                    catch (Exception ex)
                    {
                        _errorMessage = ErrorHelper.GetShortErrMessage(ex);
                    }
                }
            }

            return retVal;
        }

        public static bool Start()
        {
            bool retVal = false;

            if (_service != null)
            {
                try
                {
                    _service.Start();
                    _service.Refresh();
                    DateTime dtTmr = DateTime.Now;
                    // цикл ожидания приостановки службы
                    while (_service.Status != ServiceControllerStatus.Running)
                    {
                        System.Threading.Thread.Sleep(500);  // задержка в 500 мсек
                        _service.Refresh();
                        if ((DateTime.Now - dtTmr).TotalSeconds >= _actionTimeout)
                            throw new Exception(string.Format("Истек период в {0} секунд для запуска службы MS SQL Server ({1})", _actionTimeout, _service.ServiceName));
                    }
                    retVal = (_service.Status == ServiceControllerStatus.Running);
                    _errorMessage = null;
                }
                catch (Exception ex)
                {
                    _errorMessage = ErrorHelper.GetShortErrMessage(ex);
                }
            }

            return retVal;
        }
        #endregion


        public static void Dispose()
        {
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }


    }  // class MSSQLService
}
