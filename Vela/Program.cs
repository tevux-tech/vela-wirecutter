using Tinkerforge;

namespace Vela {
    public class Program {
        public static void Main(string[] args) {

            // Running Tinkerforge daemon from within the container. This is a workaround until proper Dockerfiles are constructed.
            System.Diagnostics.Process.Start("brickd");
            Thread.Sleep(5000);

            var brickIp = "localhost";
            LoadEnvOrDie(brickIp, "localhost");
            var isConnected = false;

            IPConnection _brickConnection;
            _brickConnection = new IPConnection();
            _brickConnection.EnumerateCallback += (IPConnection sender, string UID, string connectedUID, char position, short[] hardwareVersion, short[] firmwareVersion, int deviceIdentifier, short enumerationType) => {
                Console.WriteLine($"Enumerated: {UID}");
            };

            _brickConnection.Connected += (IPConnection sender, short connectReason) => {
                isConnected = true;
                Console.WriteLine("Connection to BrickDaemon has been established. Doing the (re)initialization.");

                _brickConnection.Enumerate();
            };

            _brickConnection.SetAutoReconnect(true);


            while (isConnected == false) {
                try {
                    _brickConnection.Connect(brickIp, 4223);
                }
                finally {
                    Thread.Sleep(5000);
                }
            }

            Thread.Sleep(-1);
        }
        public static string LoadEnvOrDie(string envVariable, string defaultValue = "") {
            if (string.IsNullOrEmpty(envVariable)) { throw new ArgumentException("Requested ENV variable must be an non-empty string.", nameof(envVariable)); }

            var loadedVariable = Environment.GetEnvironmentVariable(envVariable);
            if (string.IsNullOrEmpty(loadedVariable) && string.IsNullOrEmpty(defaultValue)) {
                Console.WriteLine($"Evironment variable {envVariable} is not in the environment. Application will exit after a few seconds.");
                Thread.Sleep(20000); // <-- Preventing restart loops in docker containers, so the user at least could see the error messages.
                Environment.Exit(-1);
            }
            else if (string.IsNullOrEmpty(loadedVariable)) {
                Console.WriteLine($"Evironment variable {envVariable} is not provided. Using default value {defaultValue}.");
                loadedVariable = defaultValue;
            }
            else {
                Console.WriteLine($"Evironment variable {envVariable} is set to {loadedVariable}.");
            }

            return loadedVariable;
        }
    }
}
