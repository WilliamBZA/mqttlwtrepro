# MqttLWTRepro

Minimal .NET **nanoFramework** app that connects to WiFi and to an MQTT broker with a Last Will
and Testament (LWT) registered, then idles — so the device can be killed (power pulled, WiFi cut,
etc.) to observe how the broker handles the will.

## What it does

Connects to WiFi, connects to the broker as `ClientId` with a will on `mqttlwtrepro/status`
(`offline`), publishes a retained birth message (`online`), then sleeps forever.

To test how the broker handles the will across different client IDs, change `ClientId` and
redeploy for the next run. There's no reconnect or retry logic — if WiFi or the broker connection
has a problem, just rerun the repro.

## Configuration

Everything lives as `const` fields at the top of [`Program.cs`](Program.cs).

## Running it

1. Deploy to a WiFi-capable device from Visual Studio.
2. Watch the topic from another machine:
3. Kill the device (pull power, cut WiFi, etc.).
4. Start the device again before the LWT timeout elapses.
5. Monitor the MQTT broker to see whether `offline` shows up on `mqttlwtrepro/status`.
6. Device logs (CONNACK result) are visible in the Visual Studio *Output* window.

### When using a dynamic client ID

Comment out line `36` and uncomment line `35`.

When using a new client ID on each restart, the LWT is published after the first client disconnects even if the device has reconnected. This reconnect doesn't prevent the publish because the device is reconnecting with a new client ID resulting in a situation where the LWT message has been sent, but the device is actually online and still publishing events.

See the below gif as the LWT is published (`status` transitions to `offline`) while the device is still publishing new events, showing that it is in fact online.

![gif](./offlinebutonluine.gif)

### When using a deterministic client ID

Comment out line `35` and uncomment line `36`.

When using a deterministic client ID, when the client reconnects with the same ID the broker sees that as the same client reconnect and the LWT message is not published.

## Packages

Restored into `packages\` (packages.config layout): `nanoFramework.CoreLibrary`,
`nanoFramework.M2Mqtt` 5.1.221, `nanoFramework.System.Device.Wifi`, plus transitive dependencies.

The MSB3276 binding-conflict warning about `nanoFramework.Runtime.Events` 1.11.37 vs 1.11.39 comes
from the upstream packages (`System.Net` was built against the older one) and is harmless.
