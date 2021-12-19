using Tinkerforge;

namespace Vela {
    public class Program {
        private static BrickletLCD128x64 _brickletLCD128X64;
        private static BrickSilentStepper _brickSilentStepper;

        public static void Main(string[] args) {

            // Running Tinkerforge daemon from within the container. This is a workaround until proper Dockerfiles are constructed.
            System.Diagnostics.Process.Start("brickd");
            Thread.Sleep(5000);

            var brickIp = "localhost";
            //var brickIp = "172.16.0.199";
            // LoadEnvOrDie(brickIp, "localhost");
            var isConnected = false;

            IPConnection _brickConnection;
            _brickConnection = new IPConnection();
            _brickConnection.EnumerateCallback += (IPConnection sender, string UID, string connectedUID, char position, short[] hardwareVersion, short[] firmwareVersion, int deviceIdentifier, short enumerationType) => {
                Console.WriteLine($"Enumerated: {UID}");

                if (deviceIdentifier == BrickletLCD128x64.DEVICE_IDENTIFIER) {
                    _brickletLCD128X64 = new BrickletLCD128x64(UID, sender);
                }

                if (deviceIdentifier == BrickSilentStepper.DEVICE_IDENTIFIER) {
                    _brickSilentStepper = new BrickSilentStepper(UID, sender);
                }
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

            while ((_brickletLCD128X64 == null) && (_brickSilentStepper == null)) {
                Thread.Sleep(100);
            }

            _brickSilentStepper.Enable();
            _brickSilentStepper.SetSpeedRamping(5000, 5000);
            _brickSilentStepper.SetMaxVelocity(10000);
            _brickSilentStepper.SetStepConfiguration(BrickSilentStepper.STEP_RESOLUTION_16, false);
            _brickletLCD128X64.ClearDisplay();
            _brickletLCD128X64.RemoveAllGUI();
            var position = 0;
            _brickletLCD128X64.GUIButtonPressedCallback += (BrickletLCD128x64 sender, byte index, bool pressed) => {
                position += 5000;
                _brickSilentStepper.SetTargetPosition(position);
            };

            _brickletLCD128X64.SetGUIButton(0, 34, 20, 60, 20, "Forward!");

            _brickletLCD128X64.SetGUIButtonPressedCallbackConfiguration(100, true);

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
