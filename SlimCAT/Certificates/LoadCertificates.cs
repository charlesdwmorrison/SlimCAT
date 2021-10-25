using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SlimCAT
{
    public class AddCertificates
    {

        // This code is not fully working. It is an example only
        public X509Certificate2Collection LoadCertificates()
        {
            // This first line of code will put us in the Unit Test Directory if we launched from a unit test.
            // ToDo: Need JSON config file 
            string execAssemblyDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string binDir = Path.GetFullPath(Path.Combine(execAssemblyDir, @"..\..\..\..\..\")); // back up 
            string pathToCertDir = binDir + @"SlimCAT\bin\Debug\net5.0\Certificates";

            // PRod Partner Channel
            string certName = "<string of certname>";
            string certFileName = "<certFileName>";
            string certPass = "<certPassword>";         

            string pathToCertPfx = Path.Combine(pathToCertDir, certFileName);

            //Create a collection object and populate it using the PFX file
            X509Certificate2Collection x509Cert2coll = new X509Certificate2Collection();
            x509Cert2coll.Import(pathToCertPfx, certPass, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            X509Certificate2Enumerator certEnumerator = x509Cert2coll.GetEnumerator();   
         

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.AddRange(x509Cert2coll);
            store.Close();

            return x509Cert2coll;

        }


    }
}
