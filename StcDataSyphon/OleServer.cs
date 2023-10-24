using System;

namespace StcDataSyphon
{
    public class OleServer
    {
        private const string comProgId = "Enterprise.OLEServer";
        private object entServer;

        public bool IsActivated { get; set; }


        public OleServer() 
        {
            activate();
        }

        private void activate()
        {
            // Creating an instance of the COM object
            var comType = Type.GetTypeFromProgID(comProgId);
            entServer = Activator.CreateInstance(comType);
            IsActivated = true;
        }

        public double getDouble(int val_1, int val_2)
        {
            return this.callConvertInts(val_1, val_2);
        }


        private double callConvertInts(int val_1, int val_2)
        {
            // todo: needs a try/catch
            var output = entServer.GetType().InvokeMember("ConvertInts", System.Reflection.BindingFlags.InvokeMethod, null, entServer, new object[] { val_1, val_2 });
            var retVal = (double)output;

            return retVal;
        }
    }
}
