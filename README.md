# NameLogger

Fixes the issue of Counter-Strike 2 not notifying name changes.

## Description

Some cheat providers let their users change their name to another player on the server, and send a message as if they were another user. This plugin attempts to resolve that issue by logging name changes on sent messages, and checks for name updates every 5 seconds. This prevents players quickly changing their names and sending a message as another player, then changing back their name.