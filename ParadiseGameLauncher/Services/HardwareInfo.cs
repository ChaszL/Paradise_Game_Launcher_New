using System;
using LibreHardwareMonitor.Hardware;

namespace ParadiseGameLauncher.Services
{
    // object used to hold current hardware readings and their display formatting
    public class HardwareInfo
    {
        // Raw percentages 
        public int CPUValue { get; set; } = 0;
        public int GPUValue { get; set; } = 0;
        public int RAMValue { get; set; } = 0;

        // String values for UI text
        public string CPUUsage => $"{CPUValue}%";
        public string GPUUsage => $"{GPUValue}%";
        public string RAMUsage => $"{RAMValue}%";

        // Dynamic colors based on usage for UI gauge coloring
        public string CPUColor => GetColor(CPUValue);
        public string GPUColor => GetColor(GPUValue);
        public string RAMColor => GetColor(RAMValue);

        // Returns a hex color based on usage percentage
        // Red:High Yellow:Medium Green:Low
        private string GetColor(int value)
        {
            if (value >= 80) 
                return "#FF4444"; // Red
            if (value >= 50) 
                return "#FFD700"; // Yellow

            return "#00FF7F"; // Green
        }
    }

    // service used to poll CPU, GPU, and RAM usage using LibreHardwareMonitor
    public class HardwareMonitorService : IDisposable
    {
        private Computer? _computer;
        private bool _available;

        public HardwareMonitorService()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true
                };
                _computer.Open();
                _available = true;
            }
            catch
            {
                _computer = null;
                _available = false;
            }
        }

        // Polls current hardware sensors and returns usage metrics
        // if sensor access is unavailable returns 0
        public HardwareInfo GetMetrics()
        {
            var info = new HardwareInfo();
            if (!_available || _computer == null) return info;

            float memUsed = 0;
            float memAvailable = 0;

            try
            {
                foreach (IHardware hardware in _computer.Hardware)
                {
                    hardware.Update();

                    // From each sensor check if its a CPU GPU or RAM and get the usage
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (hardware.HardwareType == HardwareType.Cpu &&
                            sensor.SensorType == SensorType.Load &&
                            sensor.Name == "CPU Total")
                        {
                            info.CPUValue = (int)(sensor.Value ?? 0);
                        }

                        if ((hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuNvidia) &&
                            sensor.SensorType == SensorType.Load &&
                            sensor.Name == "GPU Core")
                        {
                            info.GPUValue = (int)(sensor.Value ?? 0);
                        }

                        if (hardware.HardwareType == HardwareType.Memory)
                        {
                            if (sensor.Name == "Memory Used") 
                                memUsed = sensor.Value ?? 0;
                            if (sensor.Name == "Memory Available") 
                                memAvailable = sensor.Value ?? 0;
                        }
                    }
                }

                // Calculate RAM usage percentage
                float totalRam = memUsed + memAvailable;
                if (totalRam > 0)
                {
                    info.RAMValue = (int)((memUsed / totalRam) * 100);
                }
            }
            catch
            {
              
            }

            return info;
        }

        // Disposes of the hardware monitor service and releases resources
        public void Dispose()
        {
            try 
            { 
                _computer?.Close(); 
            } 
            catch 
            { 

            }
            _computer = null;
        }
    }
}