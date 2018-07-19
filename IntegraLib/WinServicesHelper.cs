using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


namespace IntegraLib
{
    public static class WinServicesHelper
    {
        private static ServiceController[] _scServices;

        public static ServiceController[] AllServices { get { return _scServices; } }

        static WinServicesHelper()
        {
            _scServices = ServiceController.GetServices();
        }

        public static bool IsServiceExists(string serviceName)
        {
            bool retVal = false;

            lock (_scServices)
            {
                foreach (ServiceController item in _scServices)
                {
                    if (item.ServiceName == serviceName) { retVal = true; break; }
                }
            }
            return retVal;
        }

        #region all services public methods
        public static ServiceControllerStatus GetServiceStatus(string serviceName)
        {
            ServiceControllerStatus retVal = ServiceControllerStatus.Stopped;

            using (ServiceController sc = getServiceController(serviceName))
            {
                if (sc != null) retVal = sc.Status;
            }

            return retVal;
        }

        public static bool ServiceStop(string serviceName)
        {
            bool retVal = false;

            using (ServiceController sc = getServiceController(serviceName))
            {
                if ((sc != null) && (sc.CanStop))
                {
                    try
                    {
                        sc.Stop();
                        retVal = true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return retVal;
        }

        public static bool ServicePause(string serviceName)
        {
            bool retVal = false;

            using (ServiceController sc = getServiceController(serviceName))
            {
                if ((sc != null) && (sc.CanPauseAndContinue))
                {
                    try
                    {
                        sc.Pause();
                        retVal = true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return retVal;
        }

        public static bool ServiceStart(string serviceName)
        {
            bool retVal = false;

            using (ServiceController sc = getServiceController(serviceName))
            {
                try
                {
                    if (sc != null) sc.Start();
                    retVal = true;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return retVal;
        }
        #endregion

        private static ServiceController getServiceController(string serviceName)
        {
            ServiceController retVal = null;
            lock (_scServices)
            {
                retVal = _scServices.FirstOrDefault(s => s.ServiceName == serviceName);
            }
            return retVal;
        }

    }  // class WinServicesHelper
}
