using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace InputToControllerMapper
{
    public interface IButtonSink
    {
        void SetButtonState(string button, bool pressed);
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(PressAction), "press")]
    [JsonDerivedType(typeof(ReleaseAction), "release")]
    [JsonDerivedType(typeof(DelayAction), "delay")]
    public abstract class MacroAction
    {
        public abstract Task ExecuteAsync(IButtonSink sink, CancellationToken token);
    }

    public class PressAction : MacroAction
    {
        public string Button { get; set; } = string.Empty;

        public PressAction() { }
        public PressAction(string button) { Button = button; }

        public override Task ExecuteAsync(IButtonSink sink, CancellationToken token)
        {
            sink.SetButtonState(Button, true);
            return Task.CompletedTask;
        }
    }

    public class ReleaseAction : MacroAction
    {
        public string Button { get; set; } = string.Empty;

        public ReleaseAction() { }
        public ReleaseAction(string button) { Button = button; }

        public override Task ExecuteAsync(IButtonSink sink, CancellationToken token)
        {
            sink.SetButtonState(Button, false);
            return Task.CompletedTask;
        }
    }

    public class DelayAction : MacroAction
    {
        public int Delay { get; set; }

        public DelayAction() { }
        public DelayAction(int ms) { Delay = ms; }

        public override Task ExecuteAsync(IButtonSink sink, CancellationToken token)
        {
            return Task.Delay(Delay, token);
        }
    }

    public class Macro
    {
        public string Name { get; set; } = string.Empty;
        public List<MacroAction> Actions { get; set; } = new();
        public int Repeat { get; set; } = 1;
    }

    public enum TriggerMode
    {
        Timed,
        Repeat,
        Hold,
        Toggle
    }

    public class MacroTrigger
    {
        public string MacroName { get; set; } = string.Empty;
        public TriggerMode Mode { get; set; }
        internal CancellationTokenSource? Source;
    }

    public class MacroEngine
    {
        private readonly IButtonSink sink;
        private readonly object sync = new();
        private readonly Dictionary<string, Macro> macros = new();
        private readonly Dictionary<string, MacroTrigger> triggers = new();

        public MacroEngine(IButtonSink sink)
        {
            this.sink = sink;
        }

        public void AddMacro(Macro macro)
        {
            lock (sync)
            {
                macros[macro.Name] = macro;
            }
        }

        public void RemoveMacro(string name)
        {
            lock (sync)
            {
                macros.Remove(name);
            }
        }

        public void BindTrigger(string key, MacroTrigger trigger)
        {
            lock (sync)
            {
                triggers[key] = trigger;
            }
        }

        public void UnbindTrigger(string key)
        {
            lock (sync)
            {
                triggers.Remove(key);
            }
        }

        public void HandleEvent(string key, bool isDown)
        {
            MacroTrigger? trig;
            lock (sync)
            {
                if (!triggers.TryGetValue(key, out trig))
                    return;
            }

            switch (trig.Mode)
            {
                case TriggerMode.Hold:
                    if (isDown && trig.Source == null)
                    {
                        trig.Source = new CancellationTokenSource();
                        _ = RunMacroAsync(trig.MacroName, trig.Source.Token, -1);
                    }
                    else if (!isDown && trig.Source != null)
                    {
                        trig.Source.Cancel();
                        trig.Source = null;
                    }
                    break;
                case TriggerMode.Toggle:
                    if (isDown)
                    {
                        if (trig.Source == null)
                        {
                            trig.Source = new CancellationTokenSource();
                            _ = RunMacroAsync(trig.MacroName, trig.Source.Token, -1);
                        }
                        else
                        {
                            trig.Source.Cancel();
                            trig.Source = null;
                        }
                    }
                    break;
                case TriggerMode.Repeat:
                    if (isDown)
                        _ = RunMacroAsync(trig.MacroName, CancellationToken.None, -1);
                    break;
                default:
                    if (isDown)
                        _ = RunMacroAsync(trig.MacroName, CancellationToken.None);
                    break;
            }
        }

        private async Task RunMacroAsync(string name, CancellationToken token, int repeatOverride = 0)
        {
            Macro? macro;
            lock (sync)
            {
                macros.TryGetValue(name, out macro);
            }
            if (macro == null)
                return;

            int count = repeatOverride == 0 ? macro.Repeat : repeatOverride;
            for (int i = 0; count < 0 || i < count; i++)
            {
                foreach (var action in macro.Actions)
                {
                    token.ThrowIfCancellationRequested();
                    await action.ExecuteAsync(sink, token).ConfigureAwait(false);
                }
            }
        }

        public string SerializeMacros()
        {
            lock (sync)
            {
                return JsonSerializer.Serialize(macros.Values, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        public void LoadMacros(string json)
        {
            var loaded = JsonSerializer.Deserialize<List<Macro>>(json);
            if (loaded == null) return;
            lock (sync)
            {
                macros.Clear();
                foreach (var m in loaded)
                    macros[m.Name] = m;
            }
        }
    }
}

