using System;
using System.Collections.Generic;
using System.Linq;
using KERBALISM;

namespace KerbalismInterstellarCompat.PartModules
{
    public class KFNGeneratorHandler : PartModule
    {
        [KSPField(isPersistant = true)]
        public string storedMaxPower = "0 W";

        [KSPField(isPersistant = true)] 
        public bool attachedGeneratorIsEnabled = false;
        
        // Which KSPIE generators do we want to handle
        private string[] handledGenerators = new string[] { 
            "ThermalElectricEffectGenerator", 
            "IntegratedThermalElectricPowerGenerator",
            "ThermalElectricPowerGenerator",
            "IntegratedChargedParticlesPowerGenerator",
            "ChargedParticlesPowerGenerator",
            "FNGenerator"
        };

        public void FixedUpdate()
        {
            // Find and save the last MaxPowerStr of our generator
            var generator = part.Modules.Cast<PartModule>().FirstOrDefault(m => handledGenerators.Contains(m.moduleName));
            if (generator != null)
            {
                var powerField = generator.Fields["MaxPowerStr"];
                if (powerField != null)
                {
                    storedMaxPower = powerField.GetValue<string>(generator) ?? "0 W";
                }

                var attachedEnabledField = generator.Fields["IsEnabled"];
                if (attachedEnabledField != null)
                {
                    attachedGeneratorIsEnabled = attachedEnabledField.GetValue<bool>(generator);
                }
            }
        }
        
        public static string BackgroundUpdate(Vessel v,
            ProtoPartSnapshot part_snapshot, ProtoPartModuleSnapshot module_snapshot,
            PartModule proto_part_module, Part proto_part,
            Dictionary<string, double> availableResources, List<KeyValuePair<string, double>> resourceChangeRequest,
            double elapsed_s)
        {
            bool enabled = Lib.Proto.GetBool(module_snapshot, "attachedGeneratorIsEnabled"); 
            //UnityEngine.Debug.Log("[KerbalismInterstellarCompat] Attached Generator enabled: " + enabled);
            if (enabled)
            {
                string maxPowerStr = Lib.Proto.GetString(module_snapshot, "storedMaxPower");
                
                // Shamelessly taken from Kerbalism's existing FNGenerator implementation and updated to handle TW + W
                //UnityEngine.Debug.Log("[KerbalismInterstellarCompat] Parsing storedMaxPower: " + maxPowerStr);
                double maxPower = 0;
                try
                {
                    if (maxPowerStr.Contains("TW"))
                        maxPower = double.Parse(maxPowerStr.Replace(" TW", "")) * 1000000000;
                    else if (maxPowerStr.Contains("GW"))
                        maxPower = double.Parse(maxPowerStr.Replace(" GW", "")) * 1000000;
                    else if (maxPowerStr.Contains("MW"))
                        maxPower = double.Parse(maxPowerStr.Replace(" MW", "")) * 1000;
                    else if (maxPowerStr.Contains("KW"))
                        maxPower = double.Parse(maxPowerStr.Replace(" KW", ""));
                    else
                        maxPower = double.Parse(maxPowerStr.Replace(" W", "")) * 1e-3;
                }
                catch
                {
                    return "KSPIE Generator";
                }
                
                ResourceInfo ec = KERBALISM.ResourceCache.GetResource(v, "ElectricCharge");
                
                // Match KSPIE's existing generator behavior for when Kerbalism is present:
                // only provide power if we're at 50% battery or less and aim to hold at 50%
                double targetEC = ec.Capacity / 2;
                double currentEC = ec.Amount;
                double providedPower = Math.Min(Math.Max(0, targetEC - currentEC), maxPower);
                //UnityEngine.Debug.Log("[KerbalismInterstellarCompat] Providing power: " + providedPower);
                resourceChangeRequest.Add(new KeyValuePair<string, double>("ElectricCharge", providedPower));
            }

            return "KSPIE Generator";
        }
    }
}