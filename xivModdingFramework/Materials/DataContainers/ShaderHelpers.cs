﻿using Lumina.Data.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using xivModdingFramework.Models.FileTypes;
using xivModdingFramework.Textures.Enums;

namespace xivModdingFramework.Materials.DataContainers
{
    public static class ShaderHelpers
    {
        const string _Default = "Default";

        /// <summary>
        /// Simple encapsulating struct for sanity.
        /// </summary>
        public struct ShaderConstantInfo
        {
            public string Name;
            public Dictionary<string, List<float>> KnownValues;
            public uint Id;

            public List<float> DefaultValues
            {
                get
                {
                    if (KnownValues.ContainsKey(_Default))
                    {
                        return KnownValues[_Default];
                    }
                    return new List<float>() { 0.0f };
                }
            }

            public ShaderConstantInfo(uint key, string name, List<float> defaultValue)
            {
                Id = key;
                Name = name;
                KnownValues = new Dictionary<string, List<float>>()
                {
                    { _Default, defaultValue }
                };
            }

            /// <summary>
            /// Returns a slightly prettier UI friendly version of the name that also includes the key.
            /// </summary>
            public string UIName
            {
                get
                {
                    if (String.IsNullOrWhiteSpace(Name))
                    {
                        return Id.ToString("X8");
                    }

                    return Id.ToString("X8") + " - " + Name;
                }
            }
        }

        /// <summary>
        /// Simple encapsulating struct for sanity.
        /// </summary>
        public struct ShaderKeyInfo
        {
            public uint Key;
            public string Name;
            public List<uint> KnownValues;
            public uint DefaultValue;

            public ShaderKeyInfo(uint key, string name, List<uint> values, uint defaultValue)
            {
                Key = key;
                if(values == null || values.Count == 0)
                {
                    values = new List<uint> { 0 };
                }
                Name = name;
                KnownValues = values;
                DefaultValue = defaultValue;
            }


            /// <summary>
            /// Returns a slightly prettier UI friendly version of the name that also includes the key.
            /// </summary>
            public string UIName
            {
                get
                {
                    if(String.IsNullOrWhiteSpace(Name))
                    {
                        return Key.ToString("X8");
                    }

                    return Key.ToString("X8") + " - " + Name;
                }
            }
        }


        /// <summary>
        /// Dictionary of known Shader Constants by ShaderPack
        /// </summary>
        public static Dictionary<EShaderPack, Dictionary<uint, ShaderConstantInfo>> ShaderConstants;

        /// <summary>
        /// Dictionary of known Shader Keys by ShaderPack
        /// </summary>
        public static Dictionary<EShaderPack, Dictionary<uint, ShaderKeyInfo>> ShaderKeys;

        // Load our Shader Constants and Shader Keys from JSON.
        static ShaderHelpers()
        {
            ShaderConstants = new Dictionary<EShaderPack, Dictionary<uint, ShaderConstantInfo>>();
            ShaderKeys = new Dictionary<EShaderPack, Dictionary<uint, ShaderKeyInfo>>();

            foreach (EShaderPack shpk in Enum.GetValues(typeof(EShaderPack)))
            {
                if (!ShaderConstants.ContainsKey(shpk))
                {
                    ShaderConstants.Add(shpk, new Dictionary<uint, ShaderConstantInfo>());
                }
            }

            foreach (EShaderPack shpk in Enum.GetValues(typeof(EShaderPack)))
            {
                if (!ShaderKeys.ContainsKey(shpk))
                {
                    ShaderKeys.Add(shpk, new Dictionary<uint, ShaderKeyInfo>());
                }
            }


            // Build our shader constants dictionaries.
            var files = Directory.GetFiles("./Resources/ShaderConstants");
            foreach (var file in files)
            {
                try
                {
                    var shpkString = Path.GetFileNameWithoutExtension(file) + ".shpk";
                    var shpk = GetShpkFromString(shpkString);

                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(file));
                    foreach(var kv in dict)
                    {
                        var key = UInt32.Parse(kv.Key);
                        var name = (string) kv.Value["name"];
                        var valueArr = (JArray)kv.Value["value"];
                        var values = new List<float>();
                        for(int i = 0; i < valueArr.Count; i++)
                        {
                            values.Add((float)valueArr[i]);
                        }
                        var info = new ShaderConstantInfo(key, name, values);
                        ShaderConstants[shpk].Add(key, info);
                    }
                } catch(Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
            try
            {
                var keyFile = "./Resources/ShaderKeys.json";
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(keyFile));
                foreach (var shpKv in dict)
                {
                    var shpk = GetShpkFromString(shpKv.Key + ".shpk");
                    var name = "";
                    var keyDict = shpKv.Value;
                    foreach(var kv in keyDict)
                    {
                        var key = UInt32.Parse(kv.Key);
                        var valueArr = (JArray)kv.Value;
                        var values = new HashSet<uint>();

                        uint def = 0;
                        for(int i =0; i < valueArr.Count; i++) {
                            var v = valueArr[i];
                            if(i == 0)
                            {
                                def = (uint)v;
                            }
                            values.Add((uint)v);
                        }
                        var list = values.ToList();
                        list.Sort();
                        var info = new ShaderKeyInfo(key, name, list, def);
                        ShaderKeys[shpk].Add(key, info);
                    }
                }
            } catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            AddCustomNamesAndValues();

        }

        /// Updates a given Shader Constant name if it exists and doesn't already have a name.
        private static void UpdateName(EShaderPack shpk, uint constantId, string name) {
            if(ShaderConstants.ContainsKey(shpk) && ShaderConstants[shpk].ContainsKey(constantId) && ShaderConstants[shpk][constantId].Name == "")
            {
                var sc = ShaderConstants[shpk][constantId];
                sc.Name = name;
                ShaderConstants[shpk][constantId] = sc;
            }
        }

        // Updates a given Shader Key name if it exists and doesn't already have a name.
        private static void UpdateKeyName(EShaderPack shpk, uint keyId, string name)
        {
            if (ShaderKeys.ContainsKey(shpk) && ShaderKeys[shpk].ContainsKey(keyId) && ShaderKeys[shpk][keyId].Name == "")
            {
                var sc = ShaderKeys[shpk][keyId];
                sc.Name = name;
                ShaderKeys[shpk][keyId] = sc;
            }
        }

        /// <summary>
        /// Adds custom hand-written names and values to shader keys/constants which
        /// are derived from observation or shader analysis, and not the direct
        /// memory dumps used to create the JSON files.
        /// </summary>
        private static void AddCustomNamesAndValues()
        {
            foreach(var shKv in ShaderKeys)
            {
                UpdateKeyName(shKv.Key, 4113354501, "Use Normal Map");
                UpdateKeyName(shKv.Key, 3531043187, "Use Decal Map");
                UpdateKeyName(shKv.Key, 3054951514, "Use Diffuse Map");
                UpdateKeyName(shKv.Key, 3367837167, "Use Specular Map");
                UpdateKeyName(shKv.Key, 940355280, "Is Skin");
            }

            UpdateName(EShaderPack.Skin, 1659128399, "Skin Fresnel");
            UpdateName(EShaderPack.Skin, 778088561, "Skin Tile Multiplier");
            UpdateName(EShaderPack.Skin, 740963549, "Skin Color");
            UpdateName(EShaderPack.Skin, 2569562539, "Skin Wetness Lerp");
            UpdateName(EShaderPack.Skin, 1112929012, "Skin Tile Material");

        }


        /// <summary>
        /// Converts Sampler usage to XixTexType
        /// - Note that this is not 1:1 reversable.
        /// XiVTexType should primarily only be used for naive user display purposes.
        /// </summary>
        /// <param name="samplerId"></param>
        /// <returns></returns>
        public static XivTexType SamplerIdToTexUsage(ESamplerId samplerId)
        {
            if(samplerId == ESamplerId.g_SamplerNormal || samplerId == ESamplerId.g_SamplerNormal2)
            {
                return XivTexType.Normal;
            } else if(samplerId == ESamplerId.g_SamplerMask || samplerId == ESamplerId.g_SamplerWrinklesMask || samplerId == ESamplerId.g_SamplerTileOrb)
            {
                return XivTexType.Mask;
            } else if(samplerId == ESamplerId.g_SamplerIndex)
            {
                return XivTexType.Index;
            } else if(samplerId == ESamplerId.g_SamplerDiffuse)
            {
                return XivTexType.Diffuse;
            } else if(samplerId == ESamplerId.g_SamplerReflectionArray || samplerId == ESamplerId.g_SamplerSphereMap)
            {
                return XivTexType.Reflection;
            } else if(samplerId == ESamplerId.g_SamplerDecal)
            {
                return XivTexType.Decal;
            }
            return XivTexType.Other;
        }

        /// <summary>
        /// Sampler IDs obtained via dumping RAM stuff.
        /// These are the same across all shpks/etc. so we can just use a simple enum here.
        /// </summary>
        public enum ESamplerId : uint
        {
            // This isn't ALL samplers in existence in FFXIV,
            // But it is all the samplers used with Material Textures,
            // So they're the only ones we care about.
            Unknown = 0,
            tPerlinNoise2D = 0xC06FEB5B,
            g_SamplerNormal = 0x0C5EC1F1,
            g_SamplerMask = 0x8A4E82B6,
            g_SamplerIndex = 0x565F8FD8,
            g_SamplerTable = 0x2005679F,
            g_SamplerTileOrb = 0x800BE99B,
            g_SamplerGBuffer = 0xEBBB29BD,
            g_SamplerSphereMap = 0x3334D3CA,
            g_SamplerReflectionArray = 0xC5C4CB3C,
            g_SamplerOcclusion = 0x32667BD7,
            g_SamplerDiffuse = 0x115306BE,
            g_SamplerFlow = 0xA7E197F6,
            g_SamplerDecal = 0x0237CB94,
            g_SamplerDither = 0x9F467267,
            g_SamplerNormal2 = 0x0261CDCB,
            g_SamplerWrinklesMask = 0xB3F13975,
        };

        // Enum representation of the format map data is used as.
        public enum ESamplerFormats : ushort
        {
            UsesColorset,
            NoColorset,
            Other
        };

        // Enum representation of the shader names used in mtrl files.
        public enum EShaderPack
        {
            [Description("INVALID")]
            Invalid,
            [Description("character.shpk")]
            Character,
            [Description("characterlegacy.shpk")]
            CharacterLegacy,
            [Description("characterglass.shpk")]
            CharacterGlass,
            [Description("skin.shpk")]
            Skin,
            [Description("skinlegacy.shpk")]
            SkinLegacy,
            [Description("hair.shpk")]
            Hair,
            [Description("iris.shpk")]
            Iris,
            [Description("bg.shpk")]
            Furniture,
            [Description("bgcolorchange.shpk")]
            DyeableFurniture,
            [Description("charactertattoo.shpk")]
            CharacterTatoo,
            [Description("characterocclusion.shpk")]
            CharacterOcclusion,
        };

        public static EShaderPack GetShpkFromString(string s)
        {
            return GetValueFromDescription<EShaderPack>(s);
        }

        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            return default(T);
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }


    }
}