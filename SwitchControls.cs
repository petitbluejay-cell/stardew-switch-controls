using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework.Input;
using HarmonyLib;
using System.Reflection;

namespace SwitchControls
{
    internal class ObjectPatches
    {
        private static IMonitor? Monitor = null;

        // call this method from your Entry class
        internal static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
        
        internal static void UpdateStates_postfix(StardewValley.InputState __instance)
        {
            try
            {
                if (StardewValley.Game1.currentMinigame is StardewValley.Minigames.AbigailGame) {
                    return;
                }
                
                Traverse field = Traverse.Create(__instance).Field("_currentGamepadState");
                GamePadState state = field.GetValue<GamePadState>();
                
                GamePadButtons buttonState = state.Buttons;
                Buttons orgButtons = Traverse.Create(buttonState).Field("_buttons").GetValue<Buttons>();
                Buttons newButtons = orgButtons;
                
                if ((orgButtons & Buttons.X) == Buttons.X) {
                    newButtons &= (~Buttons.X);
                    newButtons |= (Buttons.Y);
                }
                
                if ((orgButtons & Buttons.Y) == Buttons.Y) {
                    newButtons &= (~Buttons.Y);
                    newButtons |= (Buttons.X);
                }
                
                GamePadState newState = new GamePadState(
                    state.ThumbSticks,
                    state.Triggers,
                    new GamePadButtons(newButtons),
                    state.DPad
                );
                
                field.SetValue(newState);
            }
            catch (Exception ex)
            {
                if (Monitor != null) {
                    Monitor.Log($"Failed in {nameof(UpdateStates_postfix)}:\n{ex}", LogLevel.Error);
                }
            }
        }
    }
    
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ObjectPatches.Initialize(this.Monitor);
            var harmony = new Harmony(this.ModManifest.UniqueID);
            
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.InputState), nameof(StardewValley.InputState.UpdateStates)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.UpdateStates_postfix))
            );
        }
    }
}

